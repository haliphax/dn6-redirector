namespace Todd.Redirector;

/// <summary>
/// Constant values that don't belong in configuration (defaults, etc.)
/// </summary>
public static class Constants
{
	/// <summary>
	/// The configuration section containing the middleware/API configuration
	/// </summary>
	/// <value>
	/// Todd.Redirector
	/// </value>
  public const string ConfigSection = "Todd.Redirector";
	/// <summary>
	/// Default API refresh delay (10 seconds)
	/// </summary>
	/// <value>
	/// 10000
	/// </value>
  public const uint DefaultApiRefreshDelay = 10 * 1000;
}
