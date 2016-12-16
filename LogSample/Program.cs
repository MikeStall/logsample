// WebJobs Logging dmemo 
using Microsoft.Azure.WebJobs.Logging; // https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Logging/2.0.0-beta2 
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSample
{
    class Program
    {
        // See Logging implementation at https://github.com/Azure/azure-webjobs-sdk/tree/dev/src/Microsoft.Azure.WebJobs.Logging
        static void Main(string[] args)
        {
            var accountConnectionString = args[0];

            // Show recent function invocations in this range. All times are UTC!
            var startDate = DateTime.UtcNow.AddDays(-2);
            var endDate = DateTime.UtcNow;

            Reader(accountConnectionString, startDate, endDate).Wait();
        }

        static async Task Reader(string accountConnectionString, DateTime startDate, DateTime endDate)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(accountConnectionString);
            CloudTableClient client = account.CreateCloudTableClient();
            ILogTableProvider tableProvider = LogFactory.NewLogTableProvider(client);

            ILogReader reader = LogFactory.NewReader(tableProvider);

            await Reader(reader, startDate, endDate);
        }

        static async Task Reader(ILogReader reader, DateTime startDate, DateTime endDate)
        {
            Segment<IFunctionDefinition> definitions = await reader.GetFunctionDefinitionsAsync(null, null);
            foreach (IFunctionDefinition definition in definitions.Results)
            {
                Console.WriteLine("Function:  {0}", definition.Name);

                var query = new RecentFunctionQuery
                {
                    FunctionId = definition.FunctionId,
                    MaximumResults = 20,
                    Start = startDate,
                    End = endDate
                };
                Segment<IRecentFunctionEntry> instances = await reader.GetRecentFunctionInstancesAsync(query, null);
                foreach (IRecentFunctionEntry instance in instances.Results)
                {
                    Console.WriteLine("  {2}: start={0}, end={1}", instance.StartTime, instance.EndTime, instance.Status);
                }
                Console.WriteLine();
            }
        }

    }
}
