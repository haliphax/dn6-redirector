namespace Todd.Redirector;

using Microsoft.Extensions.Configuration;
using System.Text.Json;

/// <summary>
/// Service for retrieving a list of <see cref="Redirect" /> objects from a
/// REST API endpoint and producing a <see cref="RedirectMap" />
/// </summary>
public class RedirectApiService : IRedirectApiService
{
	private readonly HttpClient httpClient;
  private readonly object dataLock = new object();

  private RedirectMap redirectMap = new RedirectMap
  {
    AbsoluteRedirects = new Dictionary<string, Redirect>(),
    RelativeRedirects = new Dictionary<string, Redirect>(),
  };

  public RedirectApiService(IConfiguration config)
	{
    httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(
			config.GetSection($"{Constants.ConfigSection}:ApiBaseUrl").Value);
	}

	public RedirectApiService(HttpClient client)
	{
		httpClient = client;
	}

	/// <summary>
	/// <see cref="IRedirectApiService.CachedRedirects" />
	/// </summary>
	public RedirectMap CachedRedirects
	{
		get
		{
			// lock while providing copy of redirects for thread safety
      lock (dataLock)
      {
				return new RedirectMap
				{
					AbsoluteRedirects =
						new Dictionary<string, Redirect>(redirectMap.AbsoluteRedirects),
					RelativeRedirects =
						new Dictionary<string, Redirect>(redirectMap.RelativeRedirects),
				};
      }
    }
	}

	/// <summary>
	/// <see cref="IRedirectApiService.GetRedirects" />
	/// </summary>
	public RedirectMap GetRedirects()
	{
		var response = httpClient.GetAsync("redirects");
		response.Wait();

		if (!response.Result.IsSuccessStatusCode)
		{
			throw new HttpRequestException(
				$"Invalid API response: {response.Result.StatusCode}");
		}

		var read = response.Result.Content.ReadAsStringAsync();
		read.Wait();

    redirectMap.AbsoluteRedirects.Clear();
    redirectMap.RelativeRedirects.Clear();

		var redirects = JsonSerializer.Deserialize<Redirect[]>(read.Result)
			?? new Redirect[] { };

		// lock while assigning redirects for thread safety
		lock (dataLock)
		{
			foreach (var redirect in redirects)
			{
				var key = redirect.RedirectUrl.ToLower();

				if (redirect.UseRelative)
				{
					redirectMap.RelativeRedirects.Add(key, redirect);
				}
				else
				{
					redirectMap.AbsoluteRedirects.Add(key, redirect);
				}
			}

			return redirectMap;
    }
  }
}
