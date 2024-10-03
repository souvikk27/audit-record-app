using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            _context.Repairs.Add(repairService);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public record RepairServiceParameter(
        string Description,
        DateTime date,
        string RepairStatus,
        string ElectricianName);
}