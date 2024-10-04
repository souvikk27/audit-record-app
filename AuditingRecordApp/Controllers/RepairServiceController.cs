using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditingRecordApp.Controllers
{
    [Route("api/repairservice")]
    [ApiController]
    public class RepairServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RepairServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        [Route("create")]   
        public async Task<IActionResult> CreateService([FromBody] RepairServiceParameter parameter)
        {
            var electrician = _context.
                Electricians.
                FirstOrDefault(x =>
                    x.Name == parameter.ElectricianName);

            var repairService = new Repair
            {
                Description = parameter.Description,
                Date = parameter.date,
                Status = Enum.Parse<RepairStatus>(parameter.RepairStatus),
                Electrician = electrician
            };

            await _context.Repairs.AddAsync(repairService);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpGet]
        [Authorize]
        [Route("get")]
        public async Task<IActionResult> Get()
        {
            var repairServices = await _context.Repairs.AsNoTracking().ToListAsync();
            return Ok(repairServices);
        }

        [HttpPut]
        [Authorize]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] RepairServiceParameter parameter)
        {
            var repairService = await _context.Repairs.FindAsync(parameter.Description);
            if (repairService == null)
            {
                return NotFound();
            }

            repairService.Description = parameter.Description;
            repairService.Date = parameter.date;
            repairService.Status = Enum.Parse<RepairStatus>(parameter.RepairStatus);
            _context.Repairs.Entry(repairService).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Repair Service has been updated");
        }
    }

    public record RepairServiceParameter(
        string Description,
        DateTime date,
        string RepairStatus,
        string ElectricianName);
}