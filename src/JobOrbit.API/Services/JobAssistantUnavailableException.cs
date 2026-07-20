namespace JobOrbit.API.Services;

public sealed class JobAssistantUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
