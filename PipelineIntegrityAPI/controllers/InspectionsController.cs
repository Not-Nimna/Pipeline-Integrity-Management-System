using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Data;
using PipelineIntegrityAPI.Dtos;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public InspectionsController(AppDbContext db) => _db = db;

        // GET: api/inspections?segmentId=...
        [HttpGet]
        public async Task<ActionResult<List<InspectionListDto>>> GetAll([FromQuery] Guid segmentId)
        {
            if (segmentId == Guid.Empty)
                return BadRequest("segmentId is required.");

            var exists = await _db.Segments.AnyAsync(s => s.SegmentId == segmentId);
            if (!exists) return NotFound("Segment not found.");

            var items = await _db.Inspections
                .AsNoTracking()
                .Where(i => i.SegmentId == segmentId)
                .OrderByDescending(i => i.InspectionDate)
                .Select(i => new InspectionListDto(
                    i.InspectionId,
                    i.SegmentId,
                    i.InspectionDate,
                    i.Method,
                    i.MaxDepthPct,
                    i.Notes
                ))
                .ToListAsync();

            return Ok(items);
        }

        // POST: api/inspections
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InspectionCreateDto dto)
        {
            var seg = await _db.Segments.FindAsync(dto.SegmentId);
            if (seg is null) return BadRequest("Invalid SegmentId.");

            var inspection = new Inspection
            {
                SegmentId = dto.SegmentId,
                InspectionDate = dto.InspectionDate,
                Method = dto.Method.Trim(),
                MaxDepthPct = dto.MaxDepthPct,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
            };

            _db.Inspections.Add(inspection);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { segmentId = dto.SegmentId }, new { inspection.InspectionId });
        }

        // DELETE: api/inspections/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var inspection = await _db.Inspections.FindAsync(id);
            if (inspection is null) return NotFound();

            _db.Inspections.Remove(inspection);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
