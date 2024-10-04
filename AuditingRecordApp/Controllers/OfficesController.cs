using Microsoft.AspNetCore.Mvc;
using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuditingRecordApp.Controllers
{
    [Route("api/office")]
    [ApiController]
    public class OfficesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OfficesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] OfficeParameter parameter)
        {
            var office = Office.Create(
                Guid.NewGuid(), 
                parameter.Name, 
                parameter.Address, 
                parameter.Phone);

            await _context.Offices.AddAsync(office);
            await _context.SaveChangesAsync();
            return Ok("Office has been created");
        }

        [HttpGet]
        [Authorize]
        [Route("get")]
        public async Task<IActionResult> Get()
        {
            var offices = await _context.Offices.AsNoTracking().ToListAsync();
            return Ok(offices);
        }

        [HttpPut]
        [Authorize]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] OfficeParameter parameter)
        {
            var office = await _context.Offices.FindAsync(parameter.Name);
            if (office == null)
            {
                return NotFound();
            }

            office.Name = parameter.Name;
            office.Address = parameter.Address;
            office.Phone = parameter.Phone;
            _context.Offices.Entry(office).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Office has been updated");
        }
    }

    public record OfficeParameter(
        string Name,
        string Address,
        string Phone);
}