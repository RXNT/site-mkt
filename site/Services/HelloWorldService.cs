using MarketingSite.Client;
using Microsoft.Extensions.Logging;

namespace MarketingSite.Services;

public interface IHelloWorldService
{
    Task<string> GenerateHtmlContent();
}

public class HelloWorldService(IHtmlTemplateGenerator htmlGenerator, ICacheService cache, IMarketingApiClient dtService, ILogger<HelloWorldService> logger)
    : IHelloWorldService
{
    public async Task<string> GenerateHtmlContent()
    {
        try
        {
            var modelBag = await cache.GetAsync<DateTimeModel>("currentDateTime");

            if (modelBag is null)
            {
                modelBag = new DateTimeModel(await dtService.GetCurrentDateTimeAsync(), false);
                await cache.SetAsync("currentDateTime", modelBag with { IsCached = true }, TimeSpan.FromSeconds(5));
            }

            var htmlContent = htmlGenerator.GenerateHelloWorld(
                "Hello World",
                $"<h1>Hello World</h1><p>{modelBag.CurrentDateTime:F} <span style='color: {(modelBag.IsCached ? "green" : "red")};'>{(modelBag.IsCached ? "hit" : "miss")}</span></p>"
            );
            return htmlContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while generating hello world content.");
            var htmlError = htmlGenerator.GenerateHelloWorld(
                "Error",
                "<h1>Error</h1><p style='color: red; background-color: yellow;'>An unexpected error occurred. Please try again later.</p>"
            );
            return htmlError;
        }
    }

    private record DateTimeModel(DateTime CurrentDateTime, bool IsCached);
}