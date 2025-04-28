using System.Net;

namespace EPR.Calculator.API.Services
{
    /// <summary>
    /// Represents the response from a service process.
    /// </summary>
    public class ServiceProcessResponseDto
    {
        /// <summary>
        /// Gets or sets the status code of the service process.
        /// </summary>
        /// <value>
        /// The HTTP status code representing the result of the service process.
        /// </value>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the service process.
        /// </summary>
        /// <value>
        /// The message providing details about the service process.
        /// </value>
        public string? Message { get; set; }
    }
}
