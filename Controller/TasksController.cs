using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Work360.Domain.DTO;
using Work360.Domain.Entity;
using Work360.Infrastructure.Context;
using Work360.Infrastructure.Services;

namespace Work360.Controller
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly Work360Context _context;
        private readonly IHateoasService _hateoasService;

        public TasksController(Work360Context context, IHateoasService hateoasService)
        {
            _context = context;
            _hateoasService = hateoasService;
        }

        /// <summary>
        /// Get all Taskss with pagination
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <response code="200">Return paginated Taskss</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet(Name = "GetTasks")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetTaskss(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };

            var totalItens = await _context.Tasks.CountAsync();
            var Taskss = await _context.Tasks
                .Skip((paginParams.PageNumber - 1) * paginParams.PageSize)
                .Take(paginParams.PageSize)
                .ToListAsync();

            var result = new PagedResult<Tasks>
            {
                Items = Taskss,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "Tasks", Url);

            return Ok(result);
        }

        /// <summary>
        /// Get a specific Tasks by ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="200">Return the Tasks</response>
        /// <response code="404">Tasks not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}", Name = "GetTask")]
        public async Task<ActionResult<Tasks>> GetTasks(Guid id)
        {
            var Tasks = await _context.Tasks.FindAsync(id);

            if (Tasks == null)
            {
                return NotFound(new { error = "Usuário não encontrado", TasksId = id });
            }

            return Ok(Tasks);
        }

        /// <summary>
        /// Update an existing Tasks
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="Tasks">Dados do usuário para atualização</param>
        /// <response code="204">Tasks updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Tasks not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}", Name = "UpdateTasks")]
        public async Task<IActionResult> PutTasks(Guid id, Tasks Tasks)
        {
            if (id != Tasks.TaskID)
            {
                return BadRequest();
            }

            _context.Entry(Tasks).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TasksExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new Tasks
        /// </summary>
        /// <param name="Tasks">Dados do usuário para criação</param>
        /// <response code="201">Tasks created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "CreateTasks")]
        public async Task<ActionResult<Tasks>> PostTasks(Tasks Tasks)
        {
            _context.Tasks.Add(Tasks);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTasks", new { id = Tasks.TaskID }, Tasks);
        }

        /// <summary>
        /// Delete a Tasks
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="204">Tasks successfully removed</response>
        /// <response code="404">Tasks not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}", Name = "DeleteTasks")]
        public async Task<IActionResult> DeleteTasks(Guid id)
        {
            var Tasks = await _context.Tasks.FindAsync(id);
            if (Tasks == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(Tasks);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TasksExists(Guid id)
        {
            return _context.Tasks.Any(e => e.TaskID == id);
        }
    }
}
