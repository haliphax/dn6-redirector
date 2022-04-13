# .NET 6 Redirector Middleware / API

This project is a proof of concept for both a Middleware component and an
accompanying API. The API is responsible for providing a redirect map, while
the Middleware is responsible for checking the current request URL against the
redirect map and taking appropriate action.

The `lock` flow control is used for thread safety when reading from or assiging
to the redirect map. A copy of the redirect map is available upon request by
invoking the `CachedRedirects()` method of the `IRedirectApiService` singleton.

Barebones [class documentation] is available.

## Run the demo

First, start the demo API application:

```shell
dotnet run --project DemoAPI
```

Next, start the demo web site:

```shell
dotnet run --project DemoSite
```

Finally, visit the demonstration endpoint at http://localhost:5000.

Some quick test URLs:

- http://localhost:5000/campaignA
  *(to demonstrate absolute redirection)*
- http://localhost:5000/CAMPAiGnB
  *(to demonstrate URL normalization)*
- http://localhost:5000/product-directory/hammer/sledge
  *(to demonstrate relative redirection)*

## Configuration

The Middleware deals with two configuration values in `appsettings.json`:

- `Todd.Redirector:ApiBaseUrl` The base URL of the API web application
- `Todd.Redirector:RefreshDelay` The time (in milliseconds) between calls to
  refresh the redirect map from the API

The refresh delay may be ommitted, and will default to 10 seconds for
demonstration purposes. The base URL of the API web application is required if
you are using the `IConfiguration` constructor for the `IRedirectApiService`
singleton. If you use the `HttpClient` constructor, it is assumed that the
`BaseAddress` property of the client has already been configured.

```json
{
	"Todd.Redirector": {
		"ApiBaseUrl": "http://localhost:5177/",
		"RefreshDelay": 15000
	}
}
```

## Use in your own projects

For inclusion in your own projects, you must set up the API service:

```csharp
using Todd.Redirector;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IRedirectApiService>(
	new RedirectApiService(builder.Configuration));
```

If you want to use the `HttpClient` constructor for the `IRedirectApiService`
singleton, you should set the `BaseAddress` property yourself:

```csharp
using Todd.Redirector;

var client = new HttpClient();
client.BaseAddress = new Uri("http://some.host:1234");
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IRedirectApiService>(
	new RedirectApiService(client));
```

Next, you must set up the Middleware:

```csharp
var app = builder.Build();
app.UseMiddleware<RedirectorMiddleware>();
```

## Disclaimers

This solution is presented for demonstration purposes only. There is an
intentional intermittent failure chance of 20% for every API service call in
order to exhibit fault tolerance.


[class documentation]: https://haliphax.github.io/dn6-redirector/api/Todd.Redirector.html
