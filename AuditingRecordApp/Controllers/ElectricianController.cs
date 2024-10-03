using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditingRecordApp.Controllers
{
    [Route("api/electrician")]
    [ApiController]
    public class ElectricianController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ElectricianController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Authorize]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] ElectricianParameter parameter)
        {
            var office = _context.Offices.FirstOrDefault(x => x.Name == parameter.Name);
            if (office == null)
            {
                return NotFound($"Office {parameter.Name} not found");
            }

            var electrician = new Electrician
            {
                Name = parameter.Name,
                Phone = parameter.PhoneNumber,
                Email = parameter.Email,
                IsAvailable = parameter.IsAvailable,
                Office = office
            };

            await _context.Electricians.AddAsync(electrician);
            await _context.SaveChangesAsync();

            return Ok("electrician created");
        }
        
        [HttpGet]
        [Authorize]
        [Route("get")]
        public async Task<IActionResult> Get()
        {
            var electricians = await _context.Electricians.AsNoTracking().ToListAsync();
            return Ok(electricians);
        }

        [HttpPut]
        [Authorize]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] ElectricianParameter parameter)
        {
            var electrician = await _context.Electricians.FindAsync(parameter.Name);
            if (electrician == null)
            {
                return NotFound();
            }

            electrician.Name = parameter.Name;
            electrician.Phone = parameter.PhoneNumber;
            electrician.Email = parameter.Email;
            electrician.IsAvailable = parameter.IsAvailable;
            _context.Electricians.Entry(electrician).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("electrician updated");
        }
    }

    public record ElectricianParameter(
        string Name,
        string PhoneNumber,
        string Email,
        bool IsAvailable
    );
}