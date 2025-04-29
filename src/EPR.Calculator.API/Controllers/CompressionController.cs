using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EPR.Calculator.API.Controllers
{
    [Route("spike")]
    [AllowAnonymous]
    public class CompressionController : ControllerBase
    {
        [HttpGet]
        [Route("CompressedTest")]
        public async Task<IActionResult> GetCompressedValue()
        {
            var columns = new Dictionary<string, string>();

            var someValue = new string('*', 5000);
            for (var index = 0; index < 1000; index++)
            {
                columns.Add($"v{index}", someValue);
            }

            var serializedValue = JsonSerializer.Serialize(columns);
            return new ObjectResult(serializedValue);
        }
    }
}
