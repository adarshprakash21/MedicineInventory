using MedicineInventory.Models;

namespace MedicineInventory.Services
{
    public interface IInventoryStore
    {
        Task<IReadOnlyList<Medicine>> GetMedicinesAsync(string? searchTerm);
        Task<Medicine?> GetMedicineByIdAsync(Guid id);
        Task<Medicine> AddMedicineAsync(CreateMedicineRequest request);
        Task<Medicine?> UpdateMedicineAsync(Guid id, UpdateMedicineRequest request);
        Task<bool> DeleteMedicineAsync(Guid id);
        Task<(SaleRecord? sale, string? error)> AddSaleAsync(CreateSaleRequest request);
        Task<IReadOnlyList<SaleSummary>> GetSalesAsync();
        Task<SaleSummary?> GetSaleByIdAsync(Guid id);
        Task<InventoryDataFile> ReadDataAsync();
        Task WriteDataInternalAsync(InventoryDataFile data);

    }
}
