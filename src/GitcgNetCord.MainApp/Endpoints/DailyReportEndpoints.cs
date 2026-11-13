using GitcgNetCord.MainApp.Infrastructure;
using GitcgNetCord.MainApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GitcgNetCord.MainApp.Endpoints;

public static class DailyReportEndpoints
{
    public static IEndpointRouteBuilder MapDailyReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reports")
            .WithTags("Daily Reports");

        group.MapPost("/daily/trigger", async (
            [AsParameters] DailyReportTriggerRequest queryParams,
            DailySnapshotProcessor processor,
            CancellationToken cancellationToken,
            HttpContext httpContext
        ) =>
        {
            var uid = queryParams.Uid;
            var forceUpdate = queryParams.ForceUpdate;

            if (httpContext.Request.HasJsonContentType())
            {
                var body = await httpContext.Request.ReadFromJsonAsync<DailyReportTriggerRequest>(cancellationToken);
                if (body != null)
                {
                    if (!string.IsNullOrWhiteSpace(body.Uid))
                    {
                        uid = body.Uid;
                    }
                    forceUpdate = body.ForceUpdate;
                }
            }

            var request = new DailyReportTriggerRequest(uid, forceUpdate);
            var response = await processor.ProcessSnapshotsAsync(request, cancellationToken);
            return response.Success
                ? Results.Ok(response)
                : Results.NotFound(response);
        })
        .WithName("TriggerDailyReport")
        .WithSummary("Trigger manual daily snapshot and report delivery")
        .WithDescription("Manually captures or refreshes today's card snapshot for active Hoyolab accounts and delivers daily statistics reports to subscribed Discord threads.")
        .Produces<DailyReportTriggerResponse>(StatusCodes.Status200OK)
        .Produces<DailyReportTriggerResponse>(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
