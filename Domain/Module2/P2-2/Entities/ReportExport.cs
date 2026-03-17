namespace ProRental.Domain.Entities;
using ProRental.Domain.Enums;
public partial class ReportExport
{
	public VisualType type { get; private set; }
    public FileFormat format { get; private set; }
}