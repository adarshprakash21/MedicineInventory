using System.ComponentModel.DataAnnotations;

namespace MedicineInventory.Models;

public class CreateMedicineRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public DateOnly ExpiryDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(200)]
    public string Brand { get; set; } = string.Empty;
}
