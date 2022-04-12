namespace Todd.Middleware.Redirector;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Todd.Redirector;

public class RedirectorMiddleware
{
	private readonly IConfiguration configuration;
	private readonly ILogger logger;
	private readonly IRedirectApiService apiService;
	private readonly RequestDelegate nextMiddleware;
	private readonly object dataLock = new object();

	private Dictionary<string, Redirect> relativeRedirects;
	private Dictionary<string, Redirect> absoluteRedirects;
	private Timer refreshTimer;

	public RedirectorMiddleware(
		IConfiguration config,
		ILogger<RedirectorMiddleware> log,
		IRedirectApiService api,
		RequestDelegate next)
	{
		configuration = config;
		logger = log;
		apiService = api;
		nextMiddleware = next;

		absoluteRedirects = new Dictionary<string, Redirect>();
		relativeRedirects = new Dictionary<string, Redirect>();

		int timerDelay = Constants.DefaultApiRefreshDelay;
		var timerDelayConfig =
			config.GetSection($"{Constants.ConfigNamespace}:RefreshDelay").Value;

		if (!string.IsNullOrEmpty(timerDelayConfig?.Trim())
			&& !int.TryParse(timerDelayConfig, out timerDelay))
		{
			var message = $"Unable to parse {Constants.ConfigNamespace}:RefreshDelay value";
			log.LogCritical(message);
			throw new InvalidCastException(message);
		}

		refreshTimer = new Timer((_) => { RefreshData(); }, null, 0, timerDelay);
	}

	private void RefreshData()
	{
		IEnumerable<Redirect> redirects;

		try
		{
			redirects = apiService.GetRedirects();
		}
		catch(Exception ex)
		{
			logger.LogError($"Exception from API: {ex.Message}");
			return;
		}

		lock(dataLock)
		{
			absoluteRedirects.Clear();
			relativeRedirects.Clear();

			foreach (var redirect in redirects)
			{
				if (redirect.useRelative)
				{
					relativeRedirects.Add(redirect.redirectUrl, redirect);
				}
				else
				{
					absoluteRedirects.Add(redirect.redirectUrl, redirect);
				}
			}
		}

		logger.LogInformation("Data refreshed");
	}

	public async Task InvokeAsync(HttpContext context)
	{
		Dictionary<string, Redirect> absolutes;
		Dictionary<string, Redirect> relatives;

		lock(dataLock)
		{
			absolutes = new Dictionary<string, Redirect>(absoluteRedirects);
			relatives = new Dictionary<string, Redirect>(relativeRedirects);
		}

		var entry = absolutes.FirstOrDefault(x => context.Request.Path == x.Key);

		// no matching absolute redirect found; check relative redirects
		if (entry.Key == null)
		{
			entry = relativeRedirects
				.FirstOrDefault(x => context.Request.Path.StartsWithSegments(x.Key));

			// no redirect found; proceed with next middleware
			if (entry.Key == null)
			{
				await nextMiddleware(context);
				return;
			}
		}

		var redirect = entry.Value;
		var permanent = (redirect.redirectType == 301);
		var targetUrl = redirect.targetUrl;

		// relative URLs should include trailing path when redirecting
		if (redirect.useRelative)
		{
			targetUrl +=
				context.Request.Path.Value?.Substring(redirect.redirectUrl.Length);
		}

		logger.LogInformation(
			$"Redirecting {context.Request.Path} to {targetUrl} ({redirect.redirectType})");
		context.Response.Redirect(targetUrl, permanent);
	}
}
