using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.API.Controllers;

public class RaizesDoNordesteController : ControllerBase
{
    protected IActionResult Error<T>(
        string message,
        T data
    )
    where T : Result
    {
        var errorResponse = data.ToErrorResponse(message);
        return StatusCode((int)data.StatusCode, errorResponse);
    }
}
