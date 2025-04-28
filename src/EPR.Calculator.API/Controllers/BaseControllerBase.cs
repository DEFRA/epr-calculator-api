using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Serves as the base class for all API controllers in the application.
    /// Provides common functionality and configuration for derived controllers.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1")]
    public abstract class BaseControllerBase : ControllerBase
    {
    }
}
