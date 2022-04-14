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
	/// A list of <see cref="Redirect" /> objects
	/// </returns>
	public RedirectMap CachedRedirects { get; }
  /// <summary>
  /// Pull a new redirect map from the REST API
  /// </summary>
  /// <returns>
  /// A list of <see cref="Redirect" /> objects
  /// </returns>
  /// <exception cref="HttpRequestException"></exception>
  public RedirectMap GetRedirects();
}
