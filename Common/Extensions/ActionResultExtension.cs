using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions
{
    public static class ActionResultExtension
    {
        public static IActionResult Ok(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        public static IActionResult BadRequest(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        public static IActionResult NotFound(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        public static IActionResult Created(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        public static IActionResult UnprocessableEntity(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
        }

        public static IActionResult Conflict(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }

        public static IActionResult InternalServerError(this object data)
        {
            return new ObjectResult(data)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
