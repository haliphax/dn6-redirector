namespace Todd.Redirector;

/// <summary>
/// Two <see cref="Redirect" /> dictionaries, split between relative and
/// absolute redirects
/// </summary>
public class RedirectMap
{
	/// <summary>
	/// Relative redirects; special path processing takes place
	/// </summary>
  public IReadOnlyDictionary<string, Redirect> AbsoluteRedirects { get; set; }
	/// <summary>
	/// Absolute redirect; <c>TargetUrl</c> is used as-is
	/// </summary>
	public IReadOnlyDictionary<string, Redirect> RelativeRedirects { get; set; }
}
