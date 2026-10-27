namespace CarRental.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware (RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var exMessage = new { message = "Something went wrong on the server", error = ex.Message };

                await context.Response.WriteAsJsonAsync(exMessage);
            }
        }
    }
}
