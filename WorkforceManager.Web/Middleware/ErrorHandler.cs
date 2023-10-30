namespace WorkforceManager.Web.Middleware
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using WorkforceManager.Models;
    using WorkforceManager.Services.Exceptions;

    public class ErrorHandler
    {
        private readonly RequestDelegate next;

        public ErrorHandler(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case ArgumentException:
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        break;
                    case KeyNotFoundException e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case UnauthorizedAccessException ue:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case NullReferenceException ne:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case InvalidRequestStatusTransitionException re:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new ErrorMessageResponse{ Message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}

