namespace Todd.Redirector;

/// <summary>
/// A redirect map entry; instructs the middleware which requests to intercept,
/// where to redirect them to, and what behavior to express
/// </summary>
public class Redirect
{
	/// <summary>
	/// The request URL to match against
	/// </summary>
  public string redirectUrl { get; set; }
	/// <summary>
	/// The URL to redirect the visitor to
	/// </summary>
  public string targetUrl { get; set; }
	/// <summary>
	/// The type/status code of the redirect (301, 302)
	/// </summary>
  public uint redirectType { get; set; }
	/// <summary>
	/// Whether the redirect is "relative" (i.e. should only replace matched
	/// portion of request URL and not truncate remainder)
	/// </summary>
  public bool useRelative { get; set; }
}
