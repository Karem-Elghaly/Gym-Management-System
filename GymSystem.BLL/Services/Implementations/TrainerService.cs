using AutoMapper;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Trainers;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class TrainerService : ITrainerService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<TrainerService> _logger;

    public TrainerService(IUnitOfWork uow, IMapper mapper, ILogger<TrainerService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TrainerListItemViewModel>> GetAllAsync()
    {
        // Include(Classes) عشان الـ Mapping يحسب عدد الحصص القادمة
        var trainers = await _uow.Trainers.Query()
            .AsNoTracking()
            .Include(t => t.Classes)
            .OrderBy(t => t.FullName)
            .ToListAsync();

        return _mapper.Map<List<TrainerListItemViewModel>>(trainers);
    }

    public async Task<TrainerViewModel> GetByIdAsync(int id)
    {
        var trainer = await _uow.Trainers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Trainer), id);
        return _mapper.Map<TrainerViewModel>(trainer);
    }

    public async Task<int> CreateAsync(TrainerViewModel model)
    {
        var trainer = _mapper.Map<Trainer>(model);
        await _uow.Trainers.AddAsync(trainer);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Trainer {TrainerId} created", trainer.Id);
        return trainer.Id;
    }

    public async Task UpdateAsync(TrainerViewModel model)
    {
        var trainer = await _uow.Trainers.GetByIdAsync(model.Id)
            ?? throw new NotFoundException(nameof(Trainer), model.Id);

        _mapper.Map(model, trainer);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Trainer {TrainerId} updated", trainer.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var trainer = await _uow.Trainers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Trainer), id);

        // ممنوع نمسح مدرب عنده حصص لسه جاية
        if (await _uow.Trainers.HasUpcomingClassesAsync(id))
            throw new BusinessRuleException("Cannot delete a trainer who has upcoming classes.");

        _uow.Trainers.Delete(trainer);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Trainer {TrainerId} deleted (soft)", id);
    }

    public async Task<IReadOnlyList<LookupItem>> GetLookupAsync()
        => await _uow.Trainers.Query()
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .Select(t => new LookupItem(t.Id, t.FullName + " (" + t.Specialty + ")"))
            .ToListAsync();
}