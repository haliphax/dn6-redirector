namespace Todd.Redirector;

/// <summary>
/// Constant values that don't belong in configuration (defaults, etc.)
/// </summary>
public static class Constants
{
	/// <summary>
	/// The configuration section containing the middleware/API configuration
	/// </summary>
  public const string ConfigSection = "Todd.Redirector";
	/// <summary>
	/// Default API refresh delay (10 seconds)
	/// </summary>
  public const uint DefaultApiRefreshDelay = 10 * 1000;
}
