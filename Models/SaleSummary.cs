namespace MedicineInventory.Models;

public class SaleSummary
{
    public Guid Id { get; set; }

    public Guid MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int QuantitySold { get; set; }

    public DateTime SoldAtUtc { get; set; }
}
