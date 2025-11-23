using Microsoft.AspNetCore.Mvc;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Api.Controllers.Base
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Value == null ? NoContent() : Ok(result.Value);
            }

            return Problem(result.Error);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return Problem(result.Error);
        }

        protected IActionResult HandleResultCreated<T>(Result<T> result, string actionName, object? routeValues = null)
        {
            if (result.IsSuccess)
            {
                return CreatedAtAction(actionName, routeValues, result.Value);
            }

            return Problem(result.Error);
        }

        private IActionResult Problem(Error error)
        {
            var statusCode = error.Code switch
            {
                "NotFound" => StatusCodes.Status404NotFound,
                "Validation" => StatusCodes.Status400BadRequest,
                "Conflict" => StatusCodes.Status409Conflict,
                "Unauthorized" => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(statusCode: statusCode, title: error.Code, detail: error.Message);
        }
    }
}
