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
    private static readonly IEnumerable<Redirect> Redirects = new Redirect[] {
			new Redirect {
				RedirectUrl = "/campaignA",
				TargetUrl = "/campaigns/targetcampaign",
				RedirectType = 302,
				UseRelative = false,
			},
			new Redirect {
				RedirectUrl = "/campaignB",
				TargetUrl = "/campaigns/targetcampaign/channelB",
				RedirectType = 302,
				UseRelative = false,
			},
			new Redirect {
				RedirectUrl = "/product-directory",
				TargetUrl = "/products",
				RedirectType = 301,
				UseRelative = true,
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
    public ActionResult<IEnumerable<Redirect>> Get()
    {
			// 20% chance of failure
			// if (Random.Shared.Next(0, 9) < 2) {
			// 	logger.LogError("Intermittent failure");
			// 	return Problem();
			// }

			return Ok(Redirects);
    }
}
