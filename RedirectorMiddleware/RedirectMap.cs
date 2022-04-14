namespace Todd.Redirector;

using System.Collections.Concurrent;

/// <summary>
/// Collection of <see cref="Redirect" /> dictionaries, split between relative
/// and absolute redirects
/// </summary>
public class RedirectMap
{
	/// <summary>
	/// Relative redirects; special path processing takes place
	/// </summary>
  public IDictionary<string, Redirect> AbsoluteRedirects { get; set; }
	/// <summary>
	/// Absolute redirect; <c>TargetUrl</c> is used as-is
	/// </summary>
	public IDictionary<string, Redirect> RelativeRedirects { get; set; }
}
