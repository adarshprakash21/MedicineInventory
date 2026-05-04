using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicineInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly InventoryStore _store;

    public SalesController(InventoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleSummary>>> Get()
    {
        var sales = await _store.GetSalesAsync();
        return Ok(sales);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleSummary>> GetById(Guid id)
    {
        var sale = await _store.GetSaleByIdAsync(id);
        if (sale is null)
        {
            return NotFound(new { message = "Sale not found." });
        }

        return Ok(sale);
    }

    [HttpPost]
    public async Task<ActionResult<SaleRecord>> Post([FromBody] CreateSaleRequest request)
    {
        var (sale, error) = await _store.AddSaleAsync(request);
        if (sale is null)
        {
            return BadRequest(new { message = error });
        }

        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }
}
