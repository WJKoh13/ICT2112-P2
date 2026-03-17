namespace ProRental.Domain.Entities;
using ProRental.Domain.Enums;
public partial class PurchaseOrder
{
	public PurchaseOrderStatus status { get; private set; }
}