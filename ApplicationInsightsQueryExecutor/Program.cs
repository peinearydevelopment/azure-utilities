using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApplicationInsightsQueryExecutor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*
             * https://dev.applicationinsights.io/quickstart
             * https://docs.microsoft.com/en-us/azure/kusto/api/connection-strings/kusto
             * https://docs.microsoft.com/en-us/azure/azure-monitor/platform/rest-api-walkthrough#code-try-1
             */
            var appId = "{AZURE_APP_ID_(GUID)}";
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", "{API_KEY_FROM_AZURE}");
            var response = await httpClient.GetAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query?timespan=P1D&query=traces | limit 50").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


            //// Currently not working.
            //// https://api.applicationinsights.io/v1/apps/{appId}/query?timespan=P1D
            //// {"query":"set query_take_max_records=10001;set truncationmaxsize=67108864;\ntraces\n| limit 50\n","workspaceFilters":{"regions":[]}}
            //var client = KustoClientFactory.CreateCslQueryProvider(new KustoConnectionStringBuilder($"https://api.applicationinsights.io/v1/apps/{appId}"));
            //var reader = client.ExecuteQuery("traces | limit 50", new ClientRequestProperties());
        }
    }
}
