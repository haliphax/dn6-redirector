namespace Todd.Middleware.Redirector;

using Todd.Redirector;

public interface IRedirectApiService
{
	public IEnumerable<Redirect> CachedRedirects();
	public IEnumerable<Redirect> GetRedirects();
}
