namespace Todd.Redirector;

using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Text.Json;

/// <summary>
/// Service for retrieving a list of <see cref="Redirect" /> objects from a
/// REST API endpoint and producing a <see cref="RedirectMap" />
/// </summary>
public class RedirectApiService : IRedirectApiService
{
	private readonly HttpClient httpClient;

	private RedirectMap? redirectMap;

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
	/// See <see cref="IRedirectApiService.CachedRedirects" />
	/// </summary>
	public RedirectMap? CachedRedirects
	{
		get
		{
			return redirectMap;
		}
	}

	/// <summary>
	/// See <see cref="IRedirectApiService.GetRedirects" />
	/// </summary>
	/// <exception cref="HttpRequestException"></exception>
	public RedirectMap? GetRedirects()
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

		var absolutes = new Dictionary<string, Redirect>();
		var relatives = new Dictionary<string, Redirect>();

		var redirects = JsonSerializer.Deserialize<Redirect[]>(read.Result)
			?? new Redirect[] { };

		foreach (var redirect in redirects)
		{
			var key = redirect.RedirectUrl.ToLower();

			if (redirect.UseRelative)
			{
				relatives.Add(key, redirect);
			}
			else
			{
				absolutes.Add(key, redirect);
			}
		}

		if (redirectMap == null)
		{
			redirectMap = new RedirectMap
			{
				AbsoluteRedirects =
					new ReadOnlyDictionary<string, Redirect>(absolutes),
				RelativeRedirects =
					new ReadOnlyDictionary<string, Redirect>(relatives),
			};
		}
		else
		{
			redirectMap.AbsoluteRedirects =
				new ReadOnlyDictionary<string, Redirect>(absolutes);
			redirectMap.RelativeRedirects =
				new ReadOnlyDictionary<string, Redirect>(relatives);
		}

		return redirectMap;
	}
}
