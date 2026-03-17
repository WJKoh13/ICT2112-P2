namespace ProRental.Domain.Entities;
using ProRental.Domain.Enums;
public partial class LineItem
{
	public ReplenishmentReason reason { get; private set; }
}