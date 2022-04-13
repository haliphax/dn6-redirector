namespace Todd.Redirector;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    uint timerDelay = Constants.DefaultApiRefreshDelay;
    var timerDelayConfig =
	    config.GetSection($"{Constants.ConfigNamespace}:RefreshDelay").Value;

    if (!string.IsNullOrEmpty(timerDelayConfig?.Trim())
	    && !uint.TryParse(timerDelayConfig, out timerDelay))
    {
      throw new InvalidCastException(
				$"Unable to parse {Constants.ConfigNamespace}:RefreshDelay value");
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
    catch (Exception ex)
    {
      logger.LogError($"Exception from API: {ex.Message}");
      return;
    }

    lock (dataLock)
    {
      absoluteRedirects.Clear();
      relativeRedirects.Clear();

      foreach (var redirect in redirects)
      {
				var key = redirect.redirectUrl.ToLower();

				if (redirect.useRelative)
				{
					relativeRedirects.Add(key, redirect);
				}
				else
				{
					absoluteRedirects.Add(key, redirect);
				}
      }
    }

    logger.LogInformation("Data refreshed");
  }

  public async Task InvokeAsync(HttpContext context)
  {
    Dictionary<string, Redirect> absolutes;
    Dictionary<string, Redirect> relatives;

    // lock while reading current redirects for thread safety
    lock (dataLock)
    {
      absolutes = new Dictionary<string, Redirect>(absoluteRedirects);
      relatives = new Dictionary<string, Redirect>(relativeRedirects);
    }

    var lowerUrl = new PathString(context.Request.Path.ToString().ToLower());
    Redirect redirect;
		string targetUrl;

    // first check for absolute redirect match
    if (absolutes.ContainsKey(lowerUrl))
    {
      redirect = absolutes[lowerUrl];
			targetUrl = redirect.targetUrl;
    }
    else
    {
      // no matching absolute redirect found; check relative redirects
      redirect = relativeRedirects
	      .FirstOrDefault(x => lowerUrl.StartsWithSegments(x.Key))
	      .Value;

      // no redirect found; proceed with next middleware
      if (redirect == null)
      {
				await nextMiddleware(context);
				return;
      }

			// relative URLs should include trailing path when redirecting
      targetUrl = redirect.targetUrl
	      + context.Request.Path.Value?.Substring(redirect.redirectUrl.Length);
    }

    var permanent = (redirect.redirectType == 301);
    logger.LogInformation(
	    $"Redirecting {context.Request.Path} to {targetUrl} ({redirect.redirectType})");
    context.Response.Redirect(targetUrl, permanent);
  }
}
