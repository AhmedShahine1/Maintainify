using System.Text;

namespace Maintainify.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            // Replace the response body stream with a MemoryStream
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                // Log the request details
                var requestDetails = new StringBuilder();
                requestDetails.AppendLine($"Request: {context.Request.Method} {context.Request.Path}");
                requestDetails.AppendLine($"Host: {context.Request.Host}");
                requestDetails.AppendLine($"Protocol: {context.Request.Protocol}");
                requestDetails.AppendLine($"Remote IP: {context.Connection.RemoteIpAddress}");

                // Write the request details to the response body stream
                var bytesToWrite = Encoding.UTF8.GetBytes(requestDetails.ToString());
                await responseBody.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);

                // Call the next middleware in the pipeline
                await _next(context);

                // Restore the original response body stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
