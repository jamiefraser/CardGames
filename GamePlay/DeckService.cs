using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Game.Entities;
using Azure.Storage.Blobs;
using System.Text;

namespace Game.Play
{
    public static class DeckService
    {
        [FunctionName("CreateUnoDeck")]
        public static async Task<IActionResult>CreateDeckWithSpecialCards(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deck/{deckType}")] HttpRequest req,
            ILogger log, string deckType = "uno")
        {
            return await Run(req, log, deckType, true);
        }
        [FunctionName("CreateDeck")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deck/{deckType}/{includeWilds}")] HttpRequest req,
            ILogger log, string deckType="standard", bool includeWilds = false)
        {
            DeckBase deck = new DeckBase();
            if(deckType.ToLower().Equals("standard"))
            {
                deck = new StandardDeck(includeWilds);
            }
            else
            {
                deck = new UnoDeck();
            }
            //var deck = new StandardDeck();
            deck.Shuffle();
            log.LogInformation(deck.Id.ToString(), null);
            await deck.Save();
            return new OkObjectResult(deck);
        }
        [FunctionName("GetDeck")]
        public static async Task<IActionResult>RetrieveDeck([HttpTrigger(AuthorizationLevel.Function, "get", Route = "deck/{deckId}")] HttpRequest req, Guid deckId, ILogger log)
        {
            var deck = await Load(deckId);
            return new OkObjectResult(deck);
        }
        private static async Task<DeckBase>Load(Guid deckId)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("decks");
            var deckStream = (await blobContainerClient.GetBlobClient(deckId.ToString()).DownloadAsync()).Value.Content;
            var sr = new StreamReader(deckStream);
            var json = await sr.ReadToEndAsync();
            var deck = Newtonsoft.Json.JsonConvert.DeserializeObject<DeckBase>(json);
            return deck;
        }

        private static async Task Save(this DeckBase deck)
        {
            var id = deck.Id;
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "decks";

            // Create the container and return a container client object
            
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(id.ToString());
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(deck);
            var bytes = Encoding.ASCII.GetBytes(json);
            var ms = new MemoryStream(bytes);
            await blobClient.UploadAsync(ms);
        }
    }
}
