using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1.Gateways;

public class ReturnStageGateway : IReturnStageGateway
{
    private readonly AppDbContext _context;

    public ReturnStageGateway(AppDbContext context)
    {
        _context = context;
    }

    public ReturnStage? FindById(int stageId)
    {
        return _context.ReturnStages
            .FirstOrDefault(s => EF.Property<int>(s, "StageId") == stageId);
    }

    public List<ReturnStage> FindByReturnId(int returnId)
    {
        return _context.ReturnStages
            .Where(s => EF.Property<int>(s, "ReturnId") == returnId)
            .ToList();
    }

    public List<ReturnStage> FindHighCarbonStages(double threshold)
    {
        // Fetch all then filter in memory since resource fields are private backing fields
        return _context.ReturnStages
            .AsEnumerable()
            .Where(s => s.GetTotalResourceCount() >= threshold)
            .ToList();
    }

    public List<ReturnStage> FindAll()
    {
        return _context.ReturnStages.ToList();
    }

    public List<ReturnStage> FindByStageType(CarbonStageType stageType)
    {
        return _context.ReturnStages
            .Where(s => EF.Property<CarbonStageType?>(s, "StageType") == stageType)
            .ToList();
    }

    public bool Insert(ReturnStage stage)
    {
        try
        {
            _context.ReturnStages.Add(stage);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Update(ReturnStage stage)
    {
        try
        {
            _context.ReturnStages.Update(stage);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Delete(int stageId)
    {
        try
        {
            var stage = FindById(stageId);
            if (stage == null) return false;

            _context.ReturnStages.Remove(stage);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
