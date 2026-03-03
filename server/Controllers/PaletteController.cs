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
    /// <summary>
    /// Управление палитрой элементов пользователя
    /// </summary>
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

        /// <summary>Получить все элементы палитры текущего пользователя</summary>
        /// <returns>Список элементов палитры</returns>
        /// <response code="200">Список элементов</response>
        /// <response code="401">Не авторизован</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaletteItem>>> GetPalette()
        {
            return await _context.PaletteItems
                .Where(p => p.UserId == GetUserId())
                .ToListAsync();
        }

        /// <summary>Загрузить новое изображение в палитру</summary>
        /// <param name="file">Файл изображения (PNG, JPG, SVG, WEBP)</param>
        /// <param name="name">Название элемента</param>
        /// <param name="w">Ширина на сетке (кратно 80)</param>
        /// <param name="h">Высота на сетке (кратно 80)</param>
        /// <returns>Созданный элемент палитры</returns>
        /// <response code="200">Элемент успешно создан</response>
        /// <response code="400">Файл не выбран</response>
        /// <response code="401">Не авторизован</response>
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

        /// <summary>Удалить элемент из палитры</summary>
        /// <param name="id">ID элемента</param>
        /// <response code="204">Элемент удалён</response>
        /// <response code="404">Элемент не найден</response>
        /// <response code="401">Не авторизован</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var item = await _context.PaletteItems
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == GetUserId());

            if (item == null) return NotFound();

            // Удаляем файл с диска
            var filePath = Path.Combine(_env.ContentRootPath, "wwwroot", item.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.PaletteItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}