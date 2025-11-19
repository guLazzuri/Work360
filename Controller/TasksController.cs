using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Work360.Domain.DTO;
using Work360.Domain.Entity;
using Work360.Domain.Enum;
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
        private readonly ILogger<TasksController> _logger;

        public static readonly ActivitySource ActivitySource = new ActivitySource("Work360");

        public TasksController(Work360Context context, IHateoasService hateoasService, ILogger<TasksController> logger)
        {
            _context = context;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Taskss with pagination
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <response code="200">Return paginated Taskss</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpGet(Name = "GetTasks")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetTaskss(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var activity = ActivitySource.StartActivity("TasksController.GetTasks");

            activity?.SetTag("tasks.pageNumber", pageNumber);
            activity?.SetTag("tasks.pageSize", pageSize);

            _logger.LogInformation("Listando tarefas: Página {Page}, Tamanho {Size}", pageNumber, pageSize);
            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };

            var totalItens = await _context.Tasks.CountAsync();
            var Taskss = await _context.Tasks
                .Skip((paginParams.PageNumber - 1) * paginParams.PageSize)
                .Take(paginParams.PageSize)
                .ToListAsync();

            activity?.SetTag("tasks.count", Taskss.Count);
            activity?.SetTag("tasks.totalItems", totalItens);

            var result = new PagedResult<Tasks>
            {
                Items = Taskss,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "Tasks", Url);

            activity?.SetTag("tasks.result", result.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get a specific Tasks by ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="200">Return the Tasks</response>
        /// <response code="404">Tasks not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpGet("{id}", Name = "GetTask")]
        public async Task<ActionResult<Tasks>> GetTasks(Guid id)
        {
            var activity = ActivitySource.StartActivity("TasksController.GetTask");

            activity?.SetTag("tasks.id", id);

            _logger.LogInformation("Buscando tarefa com ID: {TasksId}", id);
            var Tasks = await _context.Tasks.FindAsync(id);

            if (Tasks == null)
            {
                activity?.SetTag("tasks.notFound", true);
                return NotFound(new { error = "Usuário não encontrado", TasksId = id });
            }

            activity?.SetTag("tasks.found", true);

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
        [Authorize]
        [HttpPut("{id}", Name = "UpdateTasks")]
        public async Task<IActionResult> PutTasks(Guid id, Tasks Tasks)
        {
            var activity = ActivitySource.StartActivity("TasksController.UpdateTasks");

            activity?.SetTag("tasks.id", id);
            activity?.SetTag("tasks.tasks", Tasks.ToString());

            _logger.LogInformation("Atualizando tarefa com ID: {TasksId}", id);
            if (id != Tasks.TaskID)
            {
                activity?.SetTag("tasks.badRequest", true);
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
                    activity?.SetTag("tasks.notFound", true);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            activity?.SetTag("tasks.updated", true);

            return NoContent();
        }

        /// <summary>
        /// Update an existing Events
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="Events">Dados do usuário para atualização</param>
        /// <response code="204">Events updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Events not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpPut("cancel/{id}", Name = "EndTasks")]
        public async Task<IActionResult> EndTasks(Guid id)
        {
            using var activity = ActivitySource.StartActivity("TasksController.EndTasks");

            var Tasks = await _context.Tasks.FirstOrDefaultAsync(x => x.TaskID == id);
            activity?.SetTag("user", Tasks.ToString());
            _logger.LogInformation("Finalizando um Tasks {eventid}", Tasks.TaskID);


            if (Tasks == null)
            {
                return NotFound();
            }


            Tasks.TaskSituation = TaskSituation.COMPLETED;
            Tasks.FinalDateTask = DateTime.Now;

            if (Tasks.FinalDateTask.HasValue)
            {
                Tasks.SpentMinutes = (int)(Tasks.FinalDateTask.Value - Tasks.CreatedTask).TotalMinutes;
            }
            else
            {
                Tasks.SpentMinutes = 0;
            }


            await _context.SaveChangesAsync();

            activity?.SetTag("event.found", true);


            return NoContent();
        }

        /// <summary>
        /// Create a new Tasks
        /// </summary>
        /// <param name="Tasks">Dados do usuário para criação</param>
        /// <response code="201">Tasks created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpPost(Name = "CreateTasks")]
        public async Task<ActionResult<Tasks>> PostTasks(Tasks Tasks)
        {
            var activity = ActivitySource.StartActivity("TasksController.CreateTasks");

            activity?.SetTag("tasks.tasks", Tasks.ToString());

            _logger.LogInformation("Criando nova tarefa");
            _context.Tasks.Add(Tasks);
            await _context.SaveChangesAsync();

            activity?.SetTag("tasks.created", true);

            return CreatedAtAction("GetTasks", new { id = Tasks.TaskID }, Tasks);
        }

        /// <summary>
        /// Delete a Tasks
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="204">Tasks successfully removed</response>
        /// <response code="404">Tasks not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpDelete("{id}", Name = "DeleteTasks")]
        public async Task<IActionResult> DeleteTasks(Guid id)
        {
            var activity = ActivitySource.StartActivity("TasksController.DeleteTasks");

            activity?.SetTag("tasks.id", id);

            _logger.LogInformation("Removendo tarefa com ID: {TasksId}", id);
            var Tasks = await _context.Tasks.FindAsync(id);
            if (Tasks == null)
            {
                activity?.SetTag("tasks.notFound", true);
                return NotFound();
            }

            _context.Tasks.Remove(Tasks);
            await _context.SaveChangesAsync();

            activity?.SetTag("tasks.deleted", true);

            return NoContent();
        }

        private bool TasksExists(Guid id)
        {
            var activity = ActivitySource.StartActivity("TasksController.TasksExists");

            activity?.SetTag("tasks.id", id);

            _logger.LogInformation("Verificando existência da tarefa com ID: {TasksId}", id);
            return _context.Tasks.Any(e => e.TaskID == id);
        }
    }
}
