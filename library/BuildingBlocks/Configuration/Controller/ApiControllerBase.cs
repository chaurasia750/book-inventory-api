using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.BuildingBlocks.Configuration.Controller
{
    /// <summary>
    /// Base for all API controllers. Secure by default: a valid JWT is required unless the action opts out with
    /// <c>[AllowAnonymous]</c>. Return plain results (<c>Ok(dto)</c>, <c>NotFound()</c>, <c>CreatedAtAction(..)</c>);
    /// errors become RFC 9457 problem details (see <c>GlobalExceptionHandler</c>). Get the caller through an
    /// injected <c>ICurrentUser</c>.
    /// </summary>
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public abstract class ApiControllerBase : ControllerBase;
}
