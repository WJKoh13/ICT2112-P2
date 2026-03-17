namespace ProRental.Domain.Entities;
using ProRental.Domain.Enums;
public partial class ReplenishmentRequest
{
	public ReplenishmentStatus status { get; private set; }
}