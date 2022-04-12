using Todd.Middleware.Redirector;
using Todd.Redirector;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();

var client = new HttpClient();

try
{
	client.BaseAddress = new Uri(
		builder.Configuration.GetValue<string>(
			$"{Todd.Site.Constants.ConfigNamespace}:ApiBaseUrl"));
}
catch(Exception ex) {
	Console.WriteLine(
		$"Error configuring Redirects API client: {ex.Message}");
	Environment.ExitCode = 1;
	return;
}

var apiService = new RedirectApiService(client);
builder.Services.AddSingleton<IRedirectApiService>(apiService);

var app = builder.Build();
app.MapControllers();
app.UseMiddleware<RedirectorMiddleware>();

// demo endpoint for displaying cached redirects from service
app.MapGet("/", () => {
	var svc = app.Services.GetService<IRedirectApiService>();

	if (svc == null)
	{
		return "No service";
	}

	return string.Join(
		"\n",
		svc.CachedRedirects().Select(
			r =>
			{
				var rel = r.useRelative ? " relative" : "";
				return $"{r.targetUrl} => {r.redirectUrl} ({r.redirectType}){rel}";
			}));
});

app.Run();
