namespace Todd.Redirector;

using Microsoft.Extensions.Configuration;
using System.Text.Json;

/// <summary>
/// Service for retrieving <see cref="Redirect" /> list from REST API
/// </summary>
public class RedirectApiService : IRedirectApiService
{
	private readonly HttpClient httpClient;
	private readonly object dataLock = new object();

	private Redirect[]? redirects;

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
	/// <see cref="IRedirectAPIservice.CachedRedirects" />
	/// </summary>
	public IEnumerable<Redirect> CachedRedirects()
	{
		// lock while retrieving redirects for thread safety
		lock (dataLock)
		{
			return redirects ?? new Redirect[] { };
		}
	}

	/// <summary>
	/// <see cref="IRedirectAPIservice.GetRedirects" />
	/// </summary>
	public IEnumerable<Redirect> GetRedirects()
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

		// lock while assigning redirects for thread safety
		lock (dataLock)
		{
			redirects = JsonSerializer.Deserialize<Redirect[]>(read.Result)
				?? new Redirect[] { };
			return redirects;
		}
	}
}
