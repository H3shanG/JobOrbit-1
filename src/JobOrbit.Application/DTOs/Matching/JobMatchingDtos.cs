using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.Matching;

public static class JobMatchAlgorithm { public const string Version="1.0"; }
public static class JobMatchWeights { public const int Skills=45,Experience=20,Education=10,Title=10,Location=10,EmploymentType=5; }
public sealed record JobMatchScoreBreakdownDto(int Skills,int Experience,int Education,int TitleRelevance,int LocationCompatibility,int EmploymentTypeCompatibility);
public sealed record JobMatchResultDto(int MatchScore,int ConfidenceScore,IReadOnlyList<string> MatchedSkills,IReadOnlyList<string> MissingSkills,IReadOnlyList<string> AdditionalCandidateSkills,IReadOnlyList<string> Strengths,IReadOnlyList<string> Gaps,JobMatchScoreBreakdownDto ScoreBreakdown,string Summary,string AlgorithmVersion=JobMatchAlgorithm.Version);
public sealed record JobRecommendationDto(int JobId,string Title,string OrganizationName,string DepartmentName,string Location,string? WorkplaceType,string EmploymentType,int MatchScore,int ConfidenceScore,IReadOnlyList<string> MatchedSkills,IReadOnlyList<string> MissingSkills,string Summary,DateTime? ClosingDate);
public sealed class CandidateRecommendationFilter { public int Limit{get;set;}=6;public int?MinimumScore{get;set;}public string?Location{get;set;}public string?WorkplaceType{get;set;}public string?EmploymentType{get;set;} }
public sealed class CandidateRankingFilter { public string?Search{get;set;}public string?ApplicationStatus{get;set;}public int?MinimumScore{get;set;}public string Sort{get;set;}="match";public int Page{get;set;}=1;public int PageSize{get;set;}=10; }
public sealed record RankedCandidateDto(int ApplicationId,int CandidateId,string CandidateName,string? ProfessionalTitle,string ApplicationStatus,int MatchScore,int ConfidenceScore,IReadOnlyList<string> MatchedSkills,IReadOnlyList<string> MissingSkills,string Summary,DateTime AppliedAt);
