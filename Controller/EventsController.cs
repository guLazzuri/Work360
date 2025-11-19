using System.Diagnostics;
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
    public class EventsController : ControllerBase
    {
        private readonly Work360Context _context;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<EventsController> _logger;
        public static readonly ActivitySource ActivitySource = new ActivitySource("Work360");


        public EventsController(Work360Context context, IHateoasService hateoasService, ILogger<EventsController> logger)
        {
            _context = context;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Events with pagination
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <response code="200">Return paginated Events</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet(Name = "GetEvents")]
        public async Task<ActionResult<IEnumerable<Events>>> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var activity = ActivitySource.StartActivity("EventsController.GetEvents");

            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };

            _logger.LogInformation("Listando eventos: Página {Page}, Tamanho {Size}", pageNumber, pageSize);


            var totalItens = await _context.Events.CountAsync();
            var Events = await _context.Events
                .Skip((paginParams.PageNumber - 1) * paginParams.PageSize)
                .Take(paginParams.PageSize)
                .ToListAsync();

            activity?.SetTag("events.count", Events.Count);
            activity?.SetTag("events.totalItems", totalItens);

            var result = new PagedResult<Events>
            {
                Items = Events,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "Events", Url);

            return Ok(result);
        }

        /// <summary>
        /// Get a specific Events by ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="200">Return the Events</response>
        /// <response code="404">Events not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}", Name = "GetEvent")]
        public async Task<ActionResult<Events>> GetEvents(Guid id)
        {
            var activity = ActivitySource.StartActivity("EventsController.GetEvent");

            _logger.LogInformation("Obtendo evento com ID: {EventId}", id);

            var Events = await _context.Events.FindAsync(id);

            if (Events == null)
            {
                return NotFound(new { error = "Usuário não encontrado", EventsId = id });
            }

            activity?.SetTag("event.found", true);


            return Ok(Events);
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
        [HttpPut("cancel/{id}", Name = "EndEvents")]
        public async Task<IActionResult> EndEvents(Guid id)
        {
            using var activity = ActivitySource.StartActivity("EventsController.EndEvents");

            var events = await _context.Events.FirstOrDefaultAsync(x => x.EventID == id);
            activity?.SetTag("user", events.ToString());
            _logger.LogInformation("Finalizando um events {eventid}", events.EventID);


            if (events == null)
            {
                return NotFound();
            }


            events.EventType = EventType.END_FOCUS_SESSION;
            events.EndDate = DateTime.Now;

            if (events.EndDate.HasValue)
            {
                events.Duration = (int)(events.EndDate.Value - events.StartDate).TotalMinutes;
            }
            else
            {
                events.Duration = 0; 
            }


            await _context.SaveChangesAsync();

            activity?.SetTag("event.found", true);


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
        [HttpPut("{id}", Name = "UpdateEvents")]
        public async Task<IActionResult> PutEvents(Guid id, Events Events)
        {
            using var activity = ActivitySource.StartActivity("EventsController.Update");
            activity?.SetTag("user", Events.ToString());

            _logger.LogInformation("Atualizando events para o usuário ID: {UserId}", Events.UserID);

            if (id != Events.EventID)
            {
                return BadRequest();
            }

            _context.Entry(Events).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            activity?.SetTag("event.found", true);


            return NoContent();
        }

        /// <summary>
        /// Create a new Events
        /// </summary>
        /// <param name="Events">Dados do usuário para criação</param>
        /// <response code="201">Events created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "CreateEvents")]
        public async Task<ActionResult<Events>> PostEvents(Events Events)
        {
            using var activity = ActivitySource.StartActivity("EventsController.CreateEvent");
            activity?.SetTag("user:", Events.ToString());

            _logger.LogInformation("Criando um novo evento para o usuário ID: {UserId}", Events.UserID);

            _context.Events.Add(Events);
            await _context.SaveChangesAsync();

            activity?.SetTag("event:", Events.ToString());


            return CreatedAtAction("GetEvents", new { id = Events.EventID }, Events);
        }

        /// <summary>
        /// Delete a Events
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="204">Events successfully removed</response>
        /// <response code="404">Events not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}", Name = "DeleteEvents")]
        public async Task<IActionResult> DeleteEvents(Guid id)
        {
            using var activity = ActivitySource.StartActivity("EventsController.DeleteEvents");

            _logger.LogInformation("Removendo evento com ID: {EventId}", id);
            var Events = await _context.Events.FindAsync(id);

            if (Events == null)
            {
                activity?.SetTag("event:", Events.ToString());
                return NotFound();
            }

            _context.Events.Remove(Events);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventsExists(Guid id)
        {
            _logger.LogInformation("Verificando existência do evento com ID: {EventId}", id);
            return _context.Events.Any(e => e.EventID == id);
        }
    }
}
