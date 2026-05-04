using System.ComponentModel.DataAnnotations;

namespace MedicineInventory.Models;

public class CreateSaleRequest
{
    [Required]
    public Guid MedicineId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantitySold { get; set; }
}
