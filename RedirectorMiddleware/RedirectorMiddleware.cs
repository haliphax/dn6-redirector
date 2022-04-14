namespace Todd.Redirector;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware which matches each request against a <see cref="RedirectMap" />
/// and, if a match is found, takes appropriate action to redirect the visitor
/// </summary>
public class RedirectorMiddleware
{
  private readonly IConfiguration configuration;
  private readonly ILogger logger;
  private readonly IRedirectApiService apiService;
  private readonly RequestDelegate nextMiddleware;

  private RedirectMap redirectMap;
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

    redirectMap = new RedirectMap();

    uint timerDelay = Constants.DefaultApiRefreshDelay;
    var timerDelayConfig =
	    config.GetSection($"{Constants.ConfigSection}:RefreshDelay").Value;

    if (!string.IsNullOrEmpty(timerDelayConfig?.Trim())
	    && !uint.TryParse(timerDelayConfig, out timerDelay))
    {
      throw new InvalidCastException(
				$"Unable to parse {Constants.ConfigSection}:RefreshDelay value");
    }

    refreshTimer = new Timer(_ => { RefreshData(); }, null, 0, timerDelay);
  }

  private void RefreshData()
  {
    try
    {
      redirectMap = apiService.GetRedirects();
    }
    catch (Exception ex)
    {
      logger.LogError($"Exception from API: {ex.Message}");
      return;
    }

    logger.LogInformation("Data refreshed");
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var lowerUrl = new PathString(context.Request.Path.ToString().ToLower());
    Redirect? redirect = null;
		string targetUrl;

    // first check for absolute redirect match
    if (redirectMap.AbsoluteRedirects != null
			&& redirectMap.AbsoluteRedirects.ContainsKey(lowerUrl))
    {
      redirect = redirectMap.AbsoluteRedirects[lowerUrl];
			targetUrl = redirect.TargetUrl;
    }
    else if (redirectMap.RelativeRedirects != null)
    {
      // no matching absolute redirect found; check relative redirects
      // by building up a potential key one URL segment at a time and checking
      // for its existence in the dictionary
      var segments = lowerUrl.ToString().Split('/');
      var key = new System.Text.StringBuilder();

      // first URL segment is always "/", so skip it
      for (var i = 1; i < segments.Length; i++)
			{
				key.Append($"/{segments[i]}");

				if (redirectMap.RelativeRedirects
					.TryGetValue(key.ToString(), out redirect))
				{
					break;
				}
      }

      // no redirect found; proceed with next middleware
      if (redirect == null)
      {
				await nextMiddleware(context);
				return;
      }

			// relative URLs should include trailing path when redirecting
      targetUrl = redirect.TargetUrl
	      + context.Request.Path.Value?.Substring(redirect.RedirectUrl.Length);
    }
		else
		{
      await nextMiddleware(context);
      return;
    }

    var permanent = (redirect.RedirectType == 301);
    logger.LogInformation(
	    $"Redirecting {context.Request.Path} to {targetUrl} ({redirect.RedirectType})");
    context.Response.Redirect(targetUrl, permanent);
  }
}
