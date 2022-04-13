namespace Todd.DemoAPI;

using Microsoft.AspNetCore.Mvc;
using Todd.Redirector;

/// <summary>
/// RESTful service endpoint that manages the <see cref="Redirect" /> list
/// </summary>
[ApiController]
[Route("[controller]")]
public class RedirectsController : ControllerBase
{
	  // mock data
    private static readonly Redirect[] Redirects = new Redirect[] {
			new Redirect {
				redirectUrl = "/campaignA",
				targetUrl = "/campaigns/targetcampaign",
				redirectType = 302,
				useRelative = false,
			},
			new Redirect {
				redirectUrl = "/campaignB",
				targetUrl = "/campaigns/targetcampaign/channelB",
				redirectType = 302,
				useRelative = false,
			},
			new Redirect {
				redirectUrl = "/product-directory",
				targetUrl = "/products",
				redirectType = 301,
				useRelative = true,
			},
		};

    private readonly ILogger<RedirectsController> logger;

    public RedirectsController(ILogger<RedirectsController> log)
    {
        logger = log;
    }

    /// <summary>
    /// Retrieve <see cref="Redirect" /> list
    /// </summary>
    /// <returns>
    /// A list of <see cref="Redirect" /> objects
    /// </returns>
    [HttpGet]
    public ActionResult Get()
    {
			// 20% chance of failure
			if (Random.Shared.Next(0, 9) < 2) {
				logger.LogError("Intermittent failure");
				return Problem();
			}

			return Ok(Redirects);
    }
}
