using ModelContextProtocol.Server;
using System.ComponentModel;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using System.Text;
using Azure;

namespace ClaudeMCP.McpTools.Azure.BigData;

[McpServerToolType]
public sealed class QueryRelated
{
    private readonly LogsQueryClient logs;
    private readonly ILogger<QueryRelated> logger;

    
    public QueryRelated(
        LogsQueryClient logs,
        ILogger<QueryRelated> logger
        )
    {
        this.logs = logs;
        this.logger = logger;
    }

     /// <summary>
    /// Executes a KQL (Kusto Query Language) query in Azure Log Analytics and returns the results as a formatted
    /// string.
    /// </summary>
    /// <remarks>This method logs the executed KQL query for informational purposes. The results are formatted
    /// based on the value of the <paramref name="asCsv"/> parameter.</remarks>
    /// <param name="workspaceId">The unique identifier of the Log Analytics workspace where the query will be executed.</param>
    /// <param name="kql">The KQL query to execute.</param>
    /// <param name="timespan">The time range for the query, specified as an ISO 8601 duration (e.g., "P1D" for one day). Defaults to "P1D".</param>
    /// <param name="asCsv">A boolean value indicating the format of the returned results.  <see langword="true"/> to return the results as
    /// a CSV-formatted string; <see langword="false"/> to return the results as a pipe-delimited string. Defaults to
    /// <see langword="true"/>.</param>
    /// <returns>A string containing the query results. If no results are found, the method returns "No results".</returns>
    [McpServerTool, Description("Executes a KQL query in Log Analytics and returns results as text/CSV")]
    public async Task<string> QueryMonitorLogsAsync(
        string workspaceId,
        string kql,
        string timespan = "P1D",
        bool asCsv = true
        )
    {
        var ts = (QueryTimeRange)TimeSpan.Parse(timespan);
        this.logger.LogInformation("KQL: {kql}", kql);
        Response<LogsQueryResult> response = await this.logs.QueryWorkspaceAsync(workspaceId, kql, ts);

        if (response.Value.AllTables.Count == 0)
        {
            return "No results";
        }

        LogsTable t = response.Value.AllTables[0];

        if (!asCsv)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(" | ", t.Columns.Select(c => c.Name)));

            foreach (LogsTableRow row in t.Rows)
            {
                sb.AppendLine(string.Join(" | ", row.Select(v => v?.ToString() ?? "")));
            }

            return sb.ToString();
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", t.Columns.Select(c => c.Name)));

            foreach (LogsTableRow row in t.Rows)
            {
                sb.AppendLine(string.Join(",", row.Select(v => (v?.ToString() ?? "").Replace(",", ";"))));
            }

            return sb.ToString();
        }
    }
}
