using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.API.Services;

public sealed class JobAssistantService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<JobAssistantService> logger) : IJobAssistantService
{
    private const string GeminiUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string OpenAiUrl = "https://api.openai.com/v1/responses";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<JobAssistantResponse> GenerateAsync(
        JobDetailsDto job,
        string mode,
        CancellationToken cancellationToken = default)
    {
        var normalizedMode = JobAssistantModes.Normalize(mode) ?? JobAssistantModes.Explain;

        if (TryGetGeminiSettings(out var geminiApiKey, out var geminiModel))
        {
            try
            {
                var geminiResponse = await GenerateWithGeminiAsync(
                    job,
                    normalizedMode,
                    geminiApiKey,
                    geminiModel,
                    cancellationToken);

                if (geminiResponse is not null)
                {
                    return FinalizeResponse(
                        geminiResponse,
                        normalizedMode,
                        "Generated with Gemini from this job post. Review the original listing before you apply.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Gemini job assistant failed for job {JobId}.",
                    job.JobId);
                throw new JobAssistantUnavailableException(
                    "The AI job assistant is temporarily unavailable. Please try again in a moment.",
                    ex);
            }
        }

        if (TryGetOpenAiSettings(out var apiKey, out var model))
        {
            try
            {
                var aiResponse = await GenerateWithOpenAiAsync(
                    job,
                    normalizedMode,
                    apiKey,
                    model,
                    cancellationToken);

                if (aiResponse is not null)
                {
                    return FinalizeResponse(
                        aiResponse,
                        normalizedMode,
                        "Generated from this job post. Review the original listing before you apply.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "OpenAI job assistant failed for job {JobId}.",
                    job.JobId);
                throw new JobAssistantUnavailableException(
                    "The AI job assistant is temporarily unavailable. Please try again in a moment.",
                    ex);
            }
        }

        throw new JobAssistantUnavailableException(
            "The AI job assistant is not configured. Add a valid API key and try again.");
    }

    private bool TryGetGeminiSettings(out string apiKey, out string model)
    {
        apiKey =
            configuration["Gemini:ApiKey"]
            ?? configuration["GEMINI_API_KEY"]
            ?? string.Empty;

        model =
            configuration["Gemini:Model"]
            ?? configuration["GEMINI_MODEL"]
            ?? "gemini-3.5-flash";

        return !string.IsNullOrWhiteSpace(apiKey);
    }

    private bool TryGetOpenAiSettings(out string apiKey, out string model)
    {
        apiKey =
            configuration["OpenAI:ApiKey"]
            ?? configuration["OPENAI_API_KEY"]
            ?? string.Empty;

        model =
            configuration["OpenAI:Model"]
            ?? configuration["OPENAI_MODEL"]
            ?? "gpt-5-mini";

        return !string.IsNullOrWhiteSpace(apiKey);
    }

    private async Task<JobAssistantResponse?> GenerateWithGeminiAsync(
        JobDetailsDto job,
        string mode,
        string apiKey,
        string model,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{GeminiUrl}/{Uri.EscapeDataString(model)}:generateContent");
        request.Headers.Add("x-goog-api-key", apiKey);
        request.Content = JsonContent.Create(
            new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = BuildGeminiPrompt(job, mode)
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4
                }
            });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);
        var outputText = TryReadGeminiText(document.RootElement);

        if (string.IsNullOrWhiteSpace(outputText))
        {
            return null;
        }

        var parsed = JsonSerializer.Deserialize<GeneratedJobAssistantPayload>(
            CleanJson(outputText),
            JsonOptions);

        if (parsed is null || string.IsNullOrWhiteSpace(parsed.Intro))
        {
            return null;
        }

        return new JobAssistantResponse
        {
            Intro = parsed.Intro.Trim(),
            Highlights = parsed.Highlights
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Take(6)
                .ToArray(),
            InterviewQuestions = parsed.InterviewQuestions
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Take(10)
                .ToArray()
        };
    }

    private async Task<JobAssistantResponse?> GenerateWithOpenAiAsync(
        JobDetailsDto job,
        string mode,
        string apiKey,
        string model,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(
            new
            {
                model,
                instructions = BuildInstructions(mode),
                input = BuildPrompt(job, mode)
            });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);

        var outputText = TryReadOutputText(document.RootElement);
        if (string.IsNullOrWhiteSpace(outputText))
        {
            return null;
        }

        var parsed = JsonSerializer.Deserialize<GeneratedJobAssistantPayload>(
            CleanJson(outputText),
            JsonOptions);

        if (parsed is null || string.IsNullOrWhiteSpace(parsed.Intro))
        {
            return null;
        }

        var highlights = parsed.Highlights
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(6)
            .ToArray();

        var questions = parsed.InterviewQuestions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(10)
            .ToArray();

        return new JobAssistantResponse
        {
            Intro = parsed.Intro.Trim(),
            Highlights = highlights,
            InterviewQuestions = questions
        };
    }

    private static JobAssistantResponse FinalizeResponse(
        JobAssistantResponse response,
        string mode,
        string note)
    {
        response.Mode = mode;
        response.UsedAi = true;
        response.Note = note;

        switch (mode)
        {
            case JobAssistantModes.Explain:
            case JobAssistantModes.DailyWork:
                response.InterviewQuestions = [];
                break;
            case JobAssistantModes.InterviewQuestions:
                response.Highlights = [];
                break;
        }

        return response;
    }

    private static string BuildInstructions(string mode)
    {
        var modeRule = mode switch
        {
            JobAssistantModes.Explain =>
                "Return a simple explanation of the role plus 3 to 5 short highlights.",
            JobAssistantModes.DailyWork =>
                "Return what the candidate will likely be doing day to day plus 4 to 6 short task bullets.",
            JobAssistantModes.InterviewQuestions =>
                "Return exactly 10 likely interview questions for the role and keep highlights empty.",
            _ =>
                "Return a simple explanation of the role plus concise highlights."
        };

        return
            "You are JobOrbit's candidate-side AI Job Assistant. " +
            "Use only the job details provided. Do not invent company policies, salary details, or required experience that are not present. " +
            "Keep the tone practical, friendly, and easy to understand. " +
            $"{modeRule} " +
            "Respond as valid JSON only with this shape: " +
            "{\"intro\":\"string\",\"highlights\":[\"string\"],\"interviewQuestions\":[\"string\"]}.";
    }

    private static string BuildGeminiPrompt(JobDetailsDto job, string mode)
    {
        return BuildInstructions(mode) + Environment.NewLine + Environment.NewLine + BuildPrompt(job, mode);
    }

    private static string BuildPrompt(JobDetailsDto job, string mode)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Mode: {mode}");
        builder.AppendLine($"Job title: {job.Title}");
        builder.AppendLine($"Company: {job.CompanyName}");
        builder.AppendLine($"Department: {job.DepartmentName}");
        builder.AppendLine($"Location: {job.Location}");
        builder.AppendLine($"Employment type: {job.EmploymentType}");
        builder.AppendLine($"Description: {job.Description}");
        builder.AppendLine($"Responsibilities: {job.Responsibilities ?? "Not provided"}");
        builder.AppendLine($"Requirements: {job.Requirements ?? "Not provided"}");
        builder.AppendLine($"Skills: {(job.Skills.Count > 0 ? string.Join(", ", job.Skills) : "Not provided")}");
        builder.AppendLine("Keep the answer grounded in the job post only.");
        return builder.ToString();
    }

    private static string? TryReadOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText)
            && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString();
        }

        if (!root.TryGetProperty("output", out var output)
            || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var piece in content.EnumerateArray())
            {
                if (piece.TryGetProperty("text", out var text)
                    && text.ValueKind == JsonValueKind.String)
                {
                    parts.Add(text.GetString() ?? string.Empty);
                }
            }
        }

        return parts.Count > 0 ? string.Join(Environment.NewLine, parts) : null;
    }

    private static string? TryReadGeminiText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates)
            || candidates.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!candidate.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var contentParts)
                || contentParts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in contentParts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text)
                    && text.ValueKind == JsonValueKind.String)
                {
                    parts.Add(text.GetString() ?? string.Empty);
                }
            }
        }

        return parts.Count > 0 ? string.Join(Environment.NewLine, parts) : null;
    }

    private static string CleanJson(string value)
    {
        var cleaned = value.Trim();
        if (cleaned.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = cleaned.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                cleaned = cleaned[(firstNewLine + 1)..];
            }

            cleaned = cleaned.Replace("```", string.Empty, StringComparison.Ordinal);
        }

        return cleaned.Trim();
    }

    private sealed class GeneratedJobAssistantPayload
    {
        public string Intro { get; set; } = string.Empty;
        public List<string> Highlights { get; set; } = [];
        public List<string> InterviewQuestions { get; set; } = [];
    }
}
