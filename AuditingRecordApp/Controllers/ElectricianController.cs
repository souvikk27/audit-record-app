using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var office = Office.Create("JJ Consulting", "123 Main St Nashville", "123-456-7890");
            _context.Offices.Add(office);
            await _context.SaveChangesAsync();

            var electrician = new Electrician
            {
                Name = parameter.Name,
                Phone = parameter.PhoneNumber,
                Email = parameter.Email,
                IsAvailable = parameter.IsAvailable,
                Office = office
            };

            _context.Electricians.Add(electrician); 
            await _context.SaveChangesAsync();

            return Ok("electrician created");
        }
    }

    public record ElectricianParameter(
        string Name,
        string PhoneNumber,
        string Email,
        bool IsAvailable
    );
}