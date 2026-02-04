namespace CoreX
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("custom middleware before");

                await next();

                await context.Response.WriteAsync("custom middleware after");
            });

            app.Run();
        }
    }
}
