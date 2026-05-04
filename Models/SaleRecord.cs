using System.ComponentModel.DataAnnotations;

namespace MedicineInventory.Models;

public class SaleRecord
{
    public Guid Id { get; set; }

    [Required]
    public Guid MedicineId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantitySold { get; set; }

    public DateTime SoldAtUtc { get; set; }
}
