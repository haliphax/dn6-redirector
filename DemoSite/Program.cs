using Todd.Redirector;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddSingleton<IRedirectApiService>(
	new RedirectApiService(builder.Configuration));

var app = builder.Build();
app.UseMiddleware<RedirectorMiddleware>();

string summarize(Redirect r)
{
	var rel = r.useRelative ? " relative" : "";
	return $"{r.redirectUrl} => {r.targetUrl} ({r.redirectType}){rel}";
}

// demo endpoint for displaying cached redirects from service
app.MapGet("/", () => {
	var svc = app.Services.GetService<IRedirectApiService>();

	if (svc == null)
	{
		return "No service";
	}

	var abs = svc.CachedRedirects.AbsoluteRedirects.Values.Select(
		r => summarize(r));
	var rel = svc.CachedRedirects.RelativeRedirects.Values.Select(
		r => summarize(r));

	return string.Join("\n", abs.Concat(rel));
});

app.Run();
