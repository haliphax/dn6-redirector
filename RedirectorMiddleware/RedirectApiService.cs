namespace Todd.Middleware.Redirector;

using System.Text.Json;
using Todd.Redirector;

public class RedirectApiService : IRedirectApiService
{
	private readonly HttpClient httpClient;
	private readonly object dataLock = new object();

	private Redirect[]? redirects;

	public RedirectApiService(HttpClient client)
	{
		httpClient = client;
	}

	public IEnumerable<Redirect> CachedRedirects()
	{
		lock (dataLock)
		{
			return redirects ?? new Redirect[] { };
		}
	}

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

		lock (dataLock)
		{
			redirects = JsonSerializer.Deserialize<Redirect[]>(read.Result);
			return redirects ?? new Redirect[] { };
		}
	}
}
