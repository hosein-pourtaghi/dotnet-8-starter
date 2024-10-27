namespace MyApiProject.Middlewares
{
    public class SwaggerCustomMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger/index.html"))
            {
                // Capture the original response body
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    // Inject custom JavaScript for automatic login
                    var injectedScript = @"
                    <script>
                        const login = async () => {
                            const response = await fetch('/api/account/login', {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json'
                                },
                                body: JSON.stringify({ username: 'b@b.cc', password: 'Sa@123456' }) // Replace with actual credentials
                            });

                            if (response.ok) {
                                const data = await response.json();
                                const token = data.token;
                                window.ui.preauthorizeApiKey('Bearer', token);
                            } else {
                                console.error('Login failed');
                            }
                        };

                        window.onload = login;
                    </script>";

                    // Inject the script before the closing body tag
                    responseBodyText = responseBodyText.Replace("</body>", injectedScript + "</body>");

                    await context.Response.WriteAsync(responseBodyText);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

}
