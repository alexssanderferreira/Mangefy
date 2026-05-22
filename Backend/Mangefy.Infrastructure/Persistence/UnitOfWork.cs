using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mangefy.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MangefyDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ICurrentUser _currentUser;

    public UnitOfWork(MangefyDbContext context, IPublisher publisher, ICurrentUser currentUser)
    {
        _context = context;
        _publisher = publisher;
        _currentUser = currentUser;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditInfo();

        int result;

        try
        {
            result = await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.FirstOrDefault();
            var entityName = entry?.Metadata.Name?.Split('.').LastOrDefault() ?? "entidade";
            throw new ConflictException(
                $"O {entityName} foi modificado por outro usuário. Recarregue e tente novamente.");
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new ConflictException(
                "Conflito de dados: registro duplicado. Verifique e tente novamente.");
        }

        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();
            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, ct);
        }

        // Persiste mudanças feitas pelos event handlers (ex.: Table.Occupy() em TabOpenedEventHandler)
        if (_context.ChangeTracker.HasChanges())
        {
            ApplyAuditInfo();
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.FirstOrDefault();
                var entityName = entry?.Metadata.Name?.Split('.').LastOrDefault() ?? "entidade";
                throw new ConflictException(
                    $"O {entityName} foi modificado por outro usuário. Recarregue e tente novamente.");
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new ConflictException(
                    "Conflito de dados: registro duplicado. Verifique e tente novamente.");
            }
        }

        return result;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private void ApplyAuditInfo()
    {
        var employeeId = _currentUser.EmployeeId;
        if (employeeId is null) return;

        foreach (var entry in _context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedByEmployee(employeeId.Value);
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedByEmployee(employeeId.Value);
        }
    }
}
