using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.BusinessSchedules.Repositories;
using MediatR;
using DomainBusinessSchedule = Mangefy.Domain.BusinessSchedules.BusinessSchedule;

namespace Mangefy.Application.Settings.BusinessSchedule.Commands.UpdateBusinessSchedule;

public sealed class UpdateBusinessScheduleCommandHandler : IRequestHandler<UpdateBusinessScheduleCommand>
{
    private readonly IBusinessScheduleRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateBusinessScheduleCommandHandler(IBusinessScheduleRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateBusinessScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (schedule is null)
        {
            schedule = DomainBusinessSchedule.Create(request.TenantId);
            await _repository.AddAsync(schedule, cancellationToken);
        }

        if (request.WeeklySchedule is not null)
        {
            foreach (var day in request.WeeklySchedule)
            {
                var dayOfWeek = Enum.Parse<DayOfWeek>(day.DayOfWeek);
                if (day.IsClosed)
                    schedule.CloseDayOfWeek(dayOfWeek);
                else
                    schedule.SetDaySchedule(dayOfWeek,
                        TimeOnly.Parse(day.OpenTime!),
                        TimeOnly.Parse(day.CloseTime!));
            }
        }

        if (request.SpecialDays is not null)
        {
            foreach (var sd in request.SpecialDays)
            {
                var date = DateOnly.Parse(sd.Date);
                if (sd.IsClosed)
                    schedule.AddHoliday(date, sd.Reason);
                else
                    schedule.AddSpecialDayWithCustomHours(
                        date, TimeOnly.Parse(sd.OpenTime!), TimeOnly.Parse(sd.CloseTime!), sd.Reason);
            }
        }

        if (request.ClosingPolicy is not null)
        {
            schedule.UpdateClosingPolicy(
                request.ClosingPolicy.GracePeriodMinutes,
                request.ClosingPolicy.AllowFinishOpenTabs,
                request.ClosingPolicy.BlockedActions);
        }

        await _repository.UpdateAsync(schedule, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
