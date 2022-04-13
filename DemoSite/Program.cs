using Todd.Redirector;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddSingleton<IRedirectApiService>(
	new RedirectApiService(builder.Configuration));

var app = builder.Build();
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
