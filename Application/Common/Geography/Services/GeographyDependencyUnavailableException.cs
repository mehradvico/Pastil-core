using System;

namespace Application.Common.Geography.Services
{
    /// <summary>
    /// The routing dependency did not answer while there was still time to serve the API request.
    /// Callers should return a recoverable message rather than persist a price they could not verify.
    /// </summary>
    public sealed class GeographyDependencyUnavailableException : Exception
    {
        public GeographyDependencyUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
