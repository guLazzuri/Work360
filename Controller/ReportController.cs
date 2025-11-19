using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class ReportController : ControllerBase
    {
        private readonly Work360Context _context;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger _logger;


        public ReportController(Work360Context context, IHateoasService hateoasService, ILogger logger)
        {
            _context = context;
            _hateoasService = hateoasService;
            _logger = logger;
        }


        /// <summary>
        /// GET QUE VAI FAZER UMA BUSCA POR USERID, E DATA
        /// <summary>
        /// vai ertornar as seguintes obj:
        // Userid
        // Data Inicio
        // Data Fim
        // Tarefas concuidas neste periodo
        // Taraefas pendentes
        // Reunioes Realizadas
        // Minutos de foco
        // Percentual de conclusao de tarefas
        // Risco Burnout
        // Tenedencia de produtividade
        // Tendencia Foco
        // Insights
        [HttpGet(Name = "GetReports")]
        public async Task<ActionResult<Report>> GetReport(
            [FromQuery] Guid userId,
            [FromQuery] DateOnly StartDate,
            [FromQuery] DateOnly EndDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10

            )
        {
            _logger.LogInformation("Gerando relatório para o usuário {UserId} de {StartDate} a {EndDate}", userId, StartDate, EndDate);
            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };


            var totalItens = await _context.Tasks.CountAsync();

            // Converter DateOnly para DateTime para comparação no banco
            DateTime startDateTime = StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime endDateTime = EndDate.ToDateTime(TimeOnly.MaxValue);


            List<Tasks> tasks = await _context.Tasks
                .Where(t => t.UserID == userId
                    && t.CreatedTask >= startDateTime
                    && t.FinalDateTask <= endDateTime)
                .ToListAsync();

            List<Meeting> Meetings = await _context.Meetings.Where(m => m.UserID == userId 
            && m.StartDate >= startDateTime 
            && m.EndDate <= endDateTime)
                .ToListAsync();

            List<Events> Events = await _context.Events.Where(e => e.UserID == userId 
            && e.StartDate >= startDateTime 
            && e.EndDate <= endDateTime)
                .ToListAsync();

            Report report = new Report
            {
                UserID = userId,
                StartDate = StartDate,
                EndDate = EndDate,
                CompletedTasks = tasks.Count(t => t.TaskSituation.ToString() == TaskSituation.COMPLETED.ToString()),
                InProgressTasks = tasks.Count(t => t.TaskSituation.ToString() == TaskSituation.IN_PROGRESS.ToString()),
                FinishedMeetings = Meetings.Count(m => m.EndDate!.HasValue),
                FocusMinutes = Events.Where(m => m.EventType.ToString() ==  EventType.END_FOCUS_SESSION.ToString()).Sum(e => e.Duration) ,
                CompletionPercentage = tasks.Count == 0 ? 0 : Math.Round((double)(tasks.Count(t => t.TaskSituation.ToString() == TaskSituation.COMPLETED.ToString()) * 100) / tasks.Count, 2),
                BurnoutRisk = null,
                ProductivityTrend = null,
                FocusTrend = null,
                Insights = null
            };

            var result = new PagedResult<Report>
            {
                Item = report,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "Report", Url);

            return Ok(result);
        }






    }
}
