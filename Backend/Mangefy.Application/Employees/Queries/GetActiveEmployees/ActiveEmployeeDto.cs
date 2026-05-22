namespace Mangefy.Application.Employees.Queries.GetActiveEmployees;

public sealed record ActiveEmployeeDto(
    Guid EmployeeId,
    string Name,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    DateTime? TemporaryAccessUntil
);
