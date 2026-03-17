namespace ProRental.Domain.Entities;
using ProRental.Domain.Enums;
public partial class Supplier
{
	public SupplierCategory category { get; private set; }
	public VettingDecision decision { get; private set; }
}