namespace ProRental.Domain.Entities;

public partial class Customerreward
{
    public string GetFormattedValue()
        => $"${Rewardvalue:F0} {Rewardtype}";
}