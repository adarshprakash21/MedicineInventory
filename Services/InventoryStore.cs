using System.Text.Json;
using MedicineInventory.Models;

namespace MedicineInventory.Services;

public class InventoryStore: IInventoryStore
{
    private readonly string _dataFilePath;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public InventoryStore(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "inventory-data.json");
    }

    public async Task<IReadOnlyList<Medicine>> GetMedicinesAsync(string? searchTerm)
    {
        var data = await ReadDataAsync();
        IEnumerable<Medicine> medicines = data.Medicines;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            medicines = medicines.Where(m =>
                m.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return medicines
            .OrderBy(m => m.FullName)
            .ToList();
    }

    public async Task<Medicine?> GetMedicineByIdAsync(Guid id)
    {
        var data = await ReadDataAsync();
        return data.Medicines.FirstOrDefault(m => m.Id == id);
    }

    public async Task<Medicine> AddMedicineAsync(CreateMedicineRequest request)
    {
        await _sync.WaitAsync();
        try
        {
            var data = await ReadDataInternalAsync();

            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                ExpiryDate = request.ExpiryDate,
                Quantity = request.Quantity,
                Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero),
                Brand = request.Brand.Trim()
            };

            data.Medicines.Add(medicine);
            await WriteDataInternalAsync(data);
            return medicine;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<Medicine?> UpdateMedicineAsync(Guid id, UpdateMedicineRequest request)
    {
        await _sync.WaitAsync();
        try
        {
            var data = await ReadDataInternalAsync();
            var medicine = data.Medicines.FirstOrDefault(m => m.Id == id);
            if (medicine is null)
            {
                return null;
            }

            medicine.FullName = request.FullName.Trim();
            medicine.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            medicine.ExpiryDate = request.ExpiryDate;
            medicine.Quantity = request.Quantity;
            medicine.Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero);
            medicine.Brand = request.Brand.Trim();

            await WriteDataInternalAsync(data);
            return medicine;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<bool> DeleteMedicineAsync(Guid id)
    {
        await _sync.WaitAsync();
        try
        {
            var data = await ReadDataInternalAsync();
            var medicine = data.Medicines.FirstOrDefault(m => m.Id == id);
            if (medicine is null)
            {
                return false;
            }

            data.Medicines.Remove(medicine);
            data.Sales.RemoveAll(s => s.MedicineId == id);
            await WriteDataInternalAsync(data);
            return true;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<(SaleRecord? sale, string? error)> AddSaleAsync(CreateSaleRequest request)
    {
        await _sync.WaitAsync();
        try
        {
            var data = await ReadDataInternalAsync();
            var medicine = data.Medicines.FirstOrDefault(m => m.Id == request.MedicineId);
            if (medicine is null)
            {
                return (null, "Medicine not found.");
            }

            if (request.QuantitySold > medicine.Quantity)
            {
                return (null, "Not enough quantity in stock.");
            }

            medicine.Quantity -= request.QuantitySold;

            var sale = new SaleRecord
            {
                Id = Guid.NewGuid(),
                MedicineId = request.MedicineId,
                QuantitySold = request.QuantitySold,
                SoldAtUtc = DateTime.UtcNow
            };

            data.Sales.Add(sale);
            await WriteDataInternalAsync(data);
            return (sale, null);
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<IReadOnlyList<SaleSummary>> GetSalesAsync()
    {
        var data = await ReadDataAsync();
        var medicineLookup = data.Medicines.ToDictionary(m => m.Id, m => m.FullName);

        return data.Sales
            .OrderByDescending(s => s.SoldAtUtc)
            .Select(s => new
            {
                s.Id,
                s.MedicineId,
                MedicineName = medicineLookup.TryGetValue(s.MedicineId, out var name) ? name : "Unknown",
                s.QuantitySold,
                s.SoldAtUtc
            })
            .Select(s => new SaleSummary
            {
                Id = s.Id,
                MedicineId = s.MedicineId,
                MedicineName = s.MedicineName,
                QuantitySold = s.QuantitySold,
                SoldAtUtc = s.SoldAtUtc
            })
            .ToList();
    }

    public async Task<SaleSummary?> GetSaleByIdAsync(Guid id)
    {
        var sales = await GetSalesAsync();
        return sales.FirstOrDefault(s => s.Id == id);
    }

    public async Task<InventoryDataFile> ReadDataAsync()
    {
        await _sync.WaitAsync();
        try
        {
            return await ReadDataInternalAsync();
        }
        finally
        {
            _sync.Release();
        }
    }

    private async Task<InventoryDataFile> ReadDataInternalAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return new InventoryDataFile();
        }

        await using var stream = File.OpenRead(_dataFilePath);
        var data = await JsonSerializer.DeserializeAsync<InventoryDataFile>(stream, JsonOptions);
        return data ?? new InventoryDataFile();
    }

    public async Task WriteDataInternalAsync(InventoryDataFile data)
    {
        await using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
    }
}
