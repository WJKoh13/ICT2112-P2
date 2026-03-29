namespace ProRental.Domain.Module3.P2_1.Interfaces;

public interface ICarbonReturnService
{
    decimal HandleCarbonSurcharge(int returnId, int stageId);
}
