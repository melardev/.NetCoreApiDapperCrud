using System.Net;
using System.Threading.Tasks;
using ApiCoreDapperCrud.Entities;
using ApiCoreDapperCrud.Enums;
using ApiCoreDapperCrud.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiCoreDapperCrud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : Controller
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var todos = await _todoService.FetchMany();
            return new OkObjectResult(todos);
        }

        [HttpGet]
        [Route("pending")]
        public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var todos = await _todoService.FetchMany(TodoShow.Pending);
            return Ok(todos);
        }

        [HttpGet]
        [Route("completed")]
        public async Task<IActionResult> GetCompleted([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var todos = await _todoService.FetchMany(TodoShow.Completed);
            return Json(todos);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetTodoDetails(int id)
        {
            var todo = await _todoService.GetById(id);
            if (todo == null)
                // return StatusCode((int) HttpStatusCode.NotFound, new ErrorDtoResponse("Not found"));
                return StatusCode((int) HttpStatusCode.NotFound, new
                {
                    Success = false,
                    FullMessages = new[]
                    {
                        "Not Found"
                    }
                });
            return Json(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Todo todo)
        {
            if (ModelState.IsValid)
            {
                await _todoService.CreateTodo(todo);
                var response = new ObjectResult(todo) {StatusCode = (int) HttpStatusCode.Created};
                return response;
            }

            return new NotFoundObjectResult(new
            {
                Success = false,
                FullMessages = new[]
                {
                    "Not Found"
                }
            });
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] Todo todo)
        {
            var todoFromDb = await _todoService.GetProxyById(id);
            if (todoFromDb == null)
                return NotFound(new
                {
                    Success = false,
                    FullMessages = new[]
                    {
                        "Not Found"
                    }
                });
            return new OkObjectResult(await _todoService.Update(todoFromDb, todo));
        }


        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todoFromDb = await _todoService.GetProxyById(id);
            if (todoFromDb == null)
                return new NotFoundObjectResult(new
                {
                    Success = false,
                    FullMessages = new[]
                    {
                        "Not Found"
                    }
                });
            await _todoService.Delete(id);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            await _todoService.DeleteAll();
            return new NoContentResult();
        }
    }
}