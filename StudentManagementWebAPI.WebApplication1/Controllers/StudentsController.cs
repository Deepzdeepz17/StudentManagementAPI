
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementWebAPI.WebApplication1.Data;
using StudentManagementWebAPI.WebApplication1.Models;

namespace StudentManagementWebAPI.WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/students?search=computer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(
            [FromQuery] string? search)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Name.Contains(search) ||
                    s.Department.Contains(search));
            }

            return Ok(await query.ToListAsync());
        }

        // GET: api/students/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            return Ok(student);
        }

        // POST: api/students
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            if (await _context.Students.AnyAsync(s => s.Email == student.Email))
                return Conflict(new { message = "Email already exists." });

            _context.Students.Add(student);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await _context.Students.AnyAsync(s => s.Email == student.Email))
                    return Conflict(new { message = "Email already exists." });

                throw;
            }

            return CreatedAtAction(
                nameof(GetStudent),
                new { id = student.Id },
                student);
        }

        // PUT: api/students/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, Student input)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            if (await _context.Students.AnyAsync(
                s => s.Email == input.Email && s.Id != id))
            {
                return Conflict(new { message = "Email already exists." });
            }

            student.Name = input.Name;
            student.Email = input.Email;
            student.Department = input.Department;
            student.Age = input.Age;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await _context.Students.AnyAsync(
                    s => s.Email == input.Email && s.Id != id))
                {
                    return Conflict(new { message = "Email already exists." });
                }

                throw;
            }

            return Ok(student);
        }

        // DELETE: api/students/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
