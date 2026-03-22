using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IReturnStageGateway
{
    ReturnStage? FindById(int stageId);
    List<ReturnStage> FindByReturnId(int returnId);
    List<ReturnStage> FindHighCarbonStages(double threshold);
    List<ReturnStage> FindAll();
    List<ReturnStage> FindByStageType(CarbonStageType stageType);
    bool Insert(ReturnStage stage);
    bool Update(ReturnStage stage);
    bool Delete(int stageId);
}
