namespace Todd.Redirector;

/// <summary>
/// Interface for the API service
/// </summary>
public interface IRedirectApiService
{
	/// <summary>
	/// Retrieve a copy of the currently cached redirect map
	/// </summary>
	/// <returns>
	/// A <see cref="RedirectMap" /> object split by relative/absolute
	/// </returns>
	public RedirectMap? CachedRedirects { get; }
  /// <summary>
  /// Pull a new redirect map from the REST API
  /// </summary>
  /// <returns>
  /// A <see cref="RedirectMap" /> object split by relative/absolute
  /// </returns>
  /// <exception cref="HttpRequestException"></exception>
  public RedirectMap? GetRedirects();
}
