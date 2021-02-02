using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Game.Services.RealTimeCommunications
{
    public static class GameTableStorageEventProcessor
    {
        [FunctionName("GameTableStorageEventProcessor")]
        public static async Task Run([BlobTrigger("tables/{name}", Connection = "AZURE_STORAGE_CONNECTION_STRING")] Stream s,
                                [Blob("tables/{name}", Connection = "AZURE_STORAGE_CONNECTION_STRING")]ICloudBlob blob,
                                string name,
                                [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages,
                                ILogger log)
        {
            //if (blob.Properties.Created != blob.Properties.LastModified) return;
            //var sr = new StreamReader(s);
            
            //var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Table>(sr.ReadToEnd());
            //await signalRMessages.AddAsync(
            //    new SignalRMessage
            //    {
            //        Target = "newtable",
            //        Arguments = new[]
            //        {
            //            new Entities.TableCreationOrDeletionMessage()
            //            {
            //                Action = Entities.TableAction.Added,
            //                Table = table
            //            }
            //        }
            //    });
            //log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}");
        }
    }
}
