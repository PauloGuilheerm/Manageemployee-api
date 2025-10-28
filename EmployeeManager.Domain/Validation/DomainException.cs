﻿namespace EmployeeManager.Domain.Validation;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
