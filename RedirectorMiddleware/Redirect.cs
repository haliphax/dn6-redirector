namespace Todd.Redirector;

public class Redirect
{
  public string redirectUrl { get; set; }
  public string targetUrl { get; set; }
  public uint redirectType { get; set; }
  public bool useRelative { get; set; }
}
