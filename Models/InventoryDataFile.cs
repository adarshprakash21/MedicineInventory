namespace MedicineInventory.Models;

public class InventoryDataFile
{
    public List<Medicine> Medicines { get; set; } = [];

    public List<SaleRecord> Sales { get; set; } = [];
}
