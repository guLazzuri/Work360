using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
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
    public class UserController : ControllerBase
    {
        private readonly Work360Context _context;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<UserController> _logger;
        public static readonly ActivitySource ActivitySource = new ActivitySource("Work360");

        public UserController(Work360Context context, IHateoasService hateoasService, ILogger<UserController> logger)
        {
            _context = context;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <response code="200">Return paginated users</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpGet(Name = "GetUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var activity = ActivitySource.StartActivity("UserController.GetUsers");

            activity?.SetTag("users.pageNumber", pageNumber);
            activity?.SetTag("users.pageSize", pageSize);

            var paginParams = new PagingParameters { PageNumber = pageNumber, PageSize = pageSize };

            _logger.LogInformation("Listando usuários: Página {Page}, Tamanho {Size}", pageNumber, pageSize);

            var totalItens = await _context.Users.CountAsync();         
            var users =  await _context.Users
                .Skip((paginParams.PageNumber - 1) * paginParams.PageSize)
                .Take(paginParams.PageSize)
                .ToListAsync();

            var result = new PagedResult<User>
            {
                Items = users,
                CurrentPage = paginParams.PageNumber,
                PageSize = paginParams.PageSize,
                TotalItems = totalItens
            };

            // Adicionar links HATEOAS
            result.Links = _hateoasService.GeneratePaginationLinks(result, "User", Url);

            activity?.SetTag("users.result", result.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="200">Return the user</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var activity = ActivitySource.StartActivity("UserController.GetUser");

            activity?.SetTag("users.id", id);

            _logger.LogInformation("Buscando usuário com ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                activity?.SetTag("users.notFound", true);
                return NotFound(new { error = "Usuário não encontrado", userId = id });
            }

            activity?.SetTag("users.found", true);

            return Ok(user);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="user">Dados do usuário para atualização</param>
        /// <response code="204">User updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize]
        [HttpPut("{id}", Name = "UpdateUser")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {   
            var activity = ActivitySource.StartActivity("UserController.UpdateUser");
            activity?.SetTag("users.id", id);
            _logger.LogInformation("Atualizando usuário com ID: {UserId}", id);
            if (id != user.UserID)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            activity?.SetTag("users.updated", true);

            return NoContent();
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user">Dados do usuário para criação</param>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "CreateUser")]
        public async Task<ActionResult<User>> PostUser(User user)
        {   
            var activity = ActivitySource.StartActivity("UserController.CreateUser");
            activity?.SetTag("users.user", user.ToString());

            _logger.LogInformation("Criando novo usuário");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            activity?.SetTag("users.created", true);
            return CreatedAtAction("GetUser", new { id = user.UserID }, user);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <response code="204">User successfully removed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}", Name = "DeleteUser")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var activity = ActivitySource.StartActivity("UserController.DeleteUser");
            activity?.SetTag("users.id", id);

            _logger.LogInformation("Removendo usuário com ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                activity?.SetTag("users.notFound", true);
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            activity?.SetTag("users.deleted", true);

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            var activity = ActivitySource.StartActivity("UserController.UserExists");
            activity?.SetTag("users.id", id);

            _logger.LogInformation("Verificando existência do usuário com ID: {UserId}", id);
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
