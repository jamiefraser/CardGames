using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Game.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace Game.Services.Table
{
    public  class Service
    {
        private IConfiguration _config;
        public Service(IConfiguration configuration)
        {
            _config = configuration;
        }
        [FunctionName("CreateTable")]
        public async Task<IActionResult> CreateTable(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "tables")] HttpRequest req,
            ILogger log)
        {
            string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
            var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
            var owner = req.UserInfo(_config);
            table.TableOwner = owner;
            table.Id = Guid.NewGuid();
            await table.Save();
            return new OkObjectResult(table);
        }

    }
}
