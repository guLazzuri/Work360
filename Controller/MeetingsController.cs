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
    public class MeetingController : ControllerBase
    {
        private readonly Work360Context _context;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<MeetingController> _logger;

        public static readonly ActivitySource ActivitySource = new ActivitySource("Work360");


        public MeetingController(Work360Context context, IHateoasService hateoasService, ILogger<MeetingController> logger)
        {
            _context = context;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Meetings with pagination
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <response code="200">Return paginated Meetings</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet(Name = "GetMeetings")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetMeetings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var activity = ActivitySource.StartActivity("MeetingController.GetMeetings");

            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };

            _logger.LogInformation("Listando reuniões: Página {Page}, Tamanho {Size}", pageNumber, pageSize);

            var totalItens = await _context.Meetings.CountAsync();
            var Meetings = await _context.Meetings
                .Skip((paginParams.PageNumber - 1) * paginParams.PageSize)
                .Take(paginParams.PageSize)
                .ToListAsync();
            
            activity?.SetTag("meetings.count", Meetings.Count);
            activity?.SetTag("meetings.totalItems", totalItens);

            var result = new PagedResult<Meeting>
            {
                Items = Meetings,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "Meeting", Url);

            activity?.SetTag("meetings.result", result.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get a specific Meeting by ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="200">Return the Meeting</response>
        /// <response code="404">Meeting not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}", Name = "GetMeeting")]
        public async Task<ActionResult<Meeting>> GetMeeting(Guid id)
        {
            var activity = ActivitySource.StartActivity("MeetingController.GetMeeting");

            _logger.LogInformation("Obtendo reunião com ID: {MeetingId}", id);
            activity?.SetTag("meeting.id", id);

            var Meeting = await _context.Meetings.FindAsync(id);

            if (Meeting == null)
            {
                activity?.SetTag("meeting.notFound", true);
                return NotFound(new { error = "Usuário não encontrado", MeetingId = id });
            }

            activity?.SetTag("meeting.found", true);

            return Ok(Meeting);
        }

        /// <summary>
        /// Update an existing Meeting
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="Meeting">Dados do usuário para atualização</param>
        /// <response code="204">Meeting updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Meeting not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}", Name = "UpdateMeeting")]
        public async Task<IActionResult> PutMeeting(Guid id, Meeting Meeting)
        {
            var activity = ActivitySource.StartActivity("MeetingController.UpdateMeeting");

            activity?.SetTag("meeting.id", id);
            activity?.SetTag("meeting.meeting", Meeting.ToString());

            _logger.LogInformation("Atualizando reunião com ID: {MeetingId}", id);
            if (id != Meeting.MeetingID)
            {
                activity?.SetTag("meeting.badRequest", true);
                return BadRequest();
            }

            _context.Entry(Meeting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeetingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            activity?.SetTag("meeting.updated", true);
            return NoContent();
        }

        /// <summary>
        /// Update an existing meeting
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="meeting">Dados do usuário para atualização</param>
        /// <response code="204">meeting updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">meeting not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("cancel/{id}", Name = "Endmeeting")]
        public async Task<IActionResult> Endmeeting(Guid id)
        {
            using var activity = ActivitySource.StartActivity("meetingController.Endmeeting");

            var meeting = await _context.Meetings.FirstOrDefaultAsync(x => x.MeetingID == id);
            activity?.SetTag("user", meeting.ToString());
            _logger.LogInformation("Finalizando um meeting {meetingid}", meeting.MeetingID);


            if (meeting == null)
            {
                return NotFound();
            }

            meeting.EndDate = DateTime.Now;

            if (meeting.EndDate.HasValue)
            {
                meeting.MinutesDuration = (int)(meeting.EndDate.Value - meeting.StartDate).TotalMinutes;
            }
            else
            {
                meeting.MinutesDuration = 0;
            }


            await _context.SaveChangesAsync();

            activity?.SetTag("meeting.found", true);


            return NoContent();
        }

        /// <summary>
        /// Create a new Meeting
        /// </summary>
        /// <param name="Meeting">Dados do usuário para criação</param>
        /// <response code="201">Meeting created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "CreateMeeting")]
        public async Task<ActionResult<Meeting>> PostMeeting(Meeting Meeting)
        {
            var activity = ActivitySource.StartActivity("MeetingController.CreateMeeting");

            activity?.SetTag("meeting.meeting", Meeting.ToString());

            _logger.LogInformation("Criando nova reunião");
            _context.Meetings.Add(Meeting);
            await _context.SaveChangesAsync();

            activity?.SetTag("meeting.created", true);

            return CreatedAtAction("GetMeeting", new { id = Meeting.MeetingID }, Meeting);
        }

        /// <summary>
        /// Delete a Meeting
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="204">Meeting successfully removed</response>
        /// <response code="404">Meeting not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}", Name = "DeleteMeeting")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var activity = ActivitySource.StartActivity("MeetingController.DeleteMeeting");

            activity?.SetTag("meeting.id", id);

            _logger.LogInformation("Removendo reunião com ID: {MeetingId}", id);
            var Meeting = await _context.Meetings.FindAsync(id);
            if (Meeting == null)
            {
                activity?.SetTag("meeting.notFound", true);
                return NotFound(new { error = "Reunião não encontrada", MeetingId = id });
            }

            _context.Meetings.Remove(Meeting);
            await _context.SaveChangesAsync();

            activity?.SetTag("meeting.deleted", true);

            return NoContent();
        }

        private bool MeetingExists(Guid id)
        {
            _logger.LogInformation("Verificando existência da reunião com ID: {MeetingId}", id);
            return _context.Meetings.Any(e => e.MeetingID == id);
        }
    }
}
