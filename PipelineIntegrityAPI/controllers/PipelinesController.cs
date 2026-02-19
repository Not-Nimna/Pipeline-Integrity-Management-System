using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Data;
using PipelineIntegrityAPI.Dtos;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PipelinesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PipelinesController(AppDbContext db) => _db = db;

        // GET: api/pipelines
        [HttpGet]
        public async Task<ActionResult<List<PipelineListDto>>> GetAll()
        {
            var items = await _db.Pipelines
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new PipelineListDto(
                    p.PipelineId,
                    p.Name,
                    p.Operator,
                    p.Status,
                    p.CreatedAt,
                    p.Segments.Count
                ))
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/pipelines/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PipelineDetailDto>> GetById(Guid id)
        {
            var pipeline = await _db.Pipelines
                .AsNoTracking()
                .Include(p => p.Segments)
                    .ThenInclude(s => s.RiskScore)
                .FirstOrDefaultAsync(p => p.PipelineId == id);

            if (pipeline is null) return NotFound();

            var dto = new PipelineDetailDto(
                pipeline.PipelineId,
                pipeline.Name,
                pipeline.Operator,
                pipeline.Status,
                pipeline.CreatedAt,
                pipeline.Segments
                    .OrderBy(s => s.Name)
                    .Select(s => new SegmentMapDto(
                        s.SegmentId,
                        s.PipelineId,
                        s.Name,
                        s.StartLat, s.StartLng,
                        s.EndLat, s.EndLng,
                        s.LengthKm,
                        s.RiskScore?.Score,
                        s.RiskScore?.Severity
                    ))
                    .ToList()
            );

            return Ok(dto);
        }

        // POST: api/pipelines
        [HttpPost]
        public async Task<ActionResult<PipelineListDto>> Create([FromBody] PipelineCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var pipeline = new Pipeline
            {
                Name = dto.Name.Trim(),
                Operator = string.IsNullOrWhiteSpace(dto.Operator) ? null : dto.Operator.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim()
            };

            _db.Pipelines.Add(pipeline);
            await _db.SaveChangesAsync();

            var result = new PipelineListDto(
                pipeline.PipelineId, pipeline.Name, pipeline.Operator, pipeline.Status, pipeline.CreatedAt, 0
            );

            return CreatedAtAction(nameof(GetById), new { id = pipeline.PipelineId }, result);
        }

        // PUT: api/pipelines/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PipelineUpdateDto dto)
        {
            var pipeline = await _db.Pipelines.FindAsync(id);
            if (pipeline is null) return NotFound();

            pipeline.Name = dto.Name.Trim();
            pipeline.Operator = string.IsNullOrWhiteSpace(dto.Operator) ? null : dto.Operator.Trim();
            pipeline.Status = string.IsNullOrWhiteSpace(dto.Status) ? pipeline.Status : dto.Status.Trim();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/pipelines/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var pipeline = await _db.Pipelines.FindAsync(id);
            if (pipeline is null) return NotFound();

            _db.Pipelines.Remove(pipeline);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
