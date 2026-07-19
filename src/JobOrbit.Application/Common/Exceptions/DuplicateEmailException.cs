namespace JobOrbit.Application.Common.Exceptions;

public sealed class DuplicateEmailException(string email)
    : Exception($"An account with email '{email}' already exists.");
