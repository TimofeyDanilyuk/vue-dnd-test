using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaletteController : ControllerBase
    {
        private Guid GetUserId() =>
                Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PaletteController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Получение всех элементов палитры из базы
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaletteItem>>> GetPalette()
        {
            return await _context.PaletteItems
                .Where(p => p.UserId == GetUserId())
                .ToListAsync();
        }

        // Метод для загрузки новой картинки
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string name, [FromForm] int w, [FromForm] int h)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var newItem = new PaletteItem
            {
                Id = Guid.NewGuid(),
                Name = name,
                ImageUrl = $"/uploads/{fileName}",
                Width = w,
                Height = h,
                UserId = GetUserId()
            };

            _context.PaletteItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(newItem);
        }
    }
}