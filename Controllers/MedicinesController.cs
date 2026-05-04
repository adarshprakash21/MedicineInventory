using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicineInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly InventoryStore _store;

    public MedicinesController(InventoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Medicine>>> Get([FromQuery] string? search = null)
    {
        var medicines = await _store.GetMedicinesAsync(search);
        return Ok(medicines);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Medicine>> GetById(Guid id)
    {
        var medicine = await _store.GetMedicineByIdAsync(id);
        if (medicine is null)
        {
            return NotFound(new { message = "Medicine not found." });
        }

        return Ok(medicine);
    }

    [HttpPost]
    public async Task<ActionResult<Medicine>> Post([FromBody] CreateMedicineRequest request)
    {
        var medicine = await _store.AddMedicineAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Medicine>> Put(Guid id, [FromBody] UpdateMedicineRequest request)
    {
        var medicine = await _store.UpdateMedicineAsync(id, request);
        if (medicine is null)
        {
            return NotFound(new { message = "Medicine not found." });
        }

        return Ok(medicine);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var removed = await _store.DeleteMedicineAsync(id);
        if (!removed)
        {
            return NotFound(new { message = "Medicine not found." });
        }

        return NoContent();
    }
}
