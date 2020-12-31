using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Graph;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Game.Services.Graph.Helpers;
using Game.Entities;

namespace Game.Services.Graph
{
    public static class Service
    {
        private static GraphServiceClient _graphServiceClient;
        [FunctionName("GetUserList")]
        public static async Task<List<EasyAuthUserInfo>> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route ="users")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var users = new List<EasyAuthUserInfo>();
            //Query using Graph SDK (preferred when possible)
            GraphServiceClient graphClient = GetAuthenticatedGraphClient();
            IGraphServiceUsersCollectionPage result;
            List<QueryOption> options = new List<QueryOption>
            {
                new QueryOption("$top", "1")
            };
            IGraphServiceUsersCollectionPage graphResult = await graphClient.Users.Request(options).GetAsync();

            result = graphResult;
            do
            {
                graphResult = await graphResult.NextPageRequest.GetAsync();
                foreach(var u in graphResult.CurrentPage)
                {
                    result.Add(u);
                }
                //graphResult = await graphClient.Users.Request(options).GetAsync();
            }
            while (graphResult.NextPageRequest != null);
            ResultToEasyAuthUserInfoList(result, users);

            return users;
        }
        private static List<EasyAuthUserInfo> ResultToEasyAuthUserInfoList(IGraphServiceUsersCollectionPage result,  List<EasyAuthUserInfo> target)
        {
            foreach(var u in result.CurrentPage)
            {
                target.Add(new EasyAuthUserInfo()
                {
                    PrincipalName = u.DisplayName,
                    PrincipalId = u.Id,
                    PrincipalIdp = u.UserPrincipalName
                });
            }
            return target;
        }
        private static GraphServiceClient GetAuthenticatedGraphClient()
        {
            var authenticationProvider = CreateAuthorizationProvider();
            _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }

        private static IAuthenticationProvider CreateAuthorizationProvider()
        {
            var clientId = System.Environment.GetEnvironmentVariable("AzureADAppClientId", EnvironmentVariableTarget.Process);
            var clientSecret = System.Environment.GetEnvironmentVariable("AzureADAppClientSecret", EnvironmentVariableTarget.Process);
            var redirectUri = System.Environment.GetEnvironmentVariable("AzureADAppRedirectUri", EnvironmentVariableTarget.Process);
            var tenantId = System.Environment.GetEnvironmentVariable("AzureADAppTenantId", EnvironmentVariableTarget.Process);
            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            //this specific scope means that application will default to what is defined in the application registration rather than using dynamic scopes
            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                              .WithAuthority(authority)
                                              .WithRedirectUri(redirectUri)
                                              .WithClientSecret(clientSecret)
                                              .Build();

            return new MsalAuthenticationProvider(cca, scopes.ToArray()); ;
        }
    }
}
