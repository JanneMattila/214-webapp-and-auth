using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp.Pages
{
    [Authorize]
    public class WarehouseModel : PageModel
    {
        public int AzureADGroupsCount { get; set; }
        public bool UserIsPartOfGroup { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public WarehouseModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnGet()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var graphserviceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
                        return Task.FromResult(0);
                    }));

            var groupName = "Finance Admins";

            // Find example group by name
            var groupRequest = graphserviceClient
                .Groups
                .Request()
                .Filter($"displayName eq '{groupName}'")
                .Select(g => new { g.Id, g.DisplayName });
            var groupResponse = await groupRequest.GetAsync();

            // Take group identifier (you could also store this in configuration store)
            var groupId = groupResponse.First().Id;

            // From: https://docs.microsoft.com/en-us/graph/api/user-checkmembergroups?view=graph-rest-1.0
            // "Check for membership in the specified list of groups."
            // "Returns from the list those groups of which the user"
            // "has a direct or transitive membership."
            var checkMembershipRequest = graphserviceClient
                .Me
                .CheckMemberGroups(new List<string>() { groupId })
                .Request();

            var checkMembershipResponse = await checkMembershipRequest.PostAsync();
            if (checkMembershipResponse.Any())
            {
                UserIsPartOfGroup = true;
            }

            await DifferentGraphAPIClientExamples(graphserviceClient);
        }

        private async Task DifferentGraphAPIClientExamples(GraphServiceClient graphserviceClient)
        {
            // 1. Get *all* groups
            var groupsRequest = graphserviceClient
                .Groups
                .Request();
            var groupResponse = await groupsRequest.GetAsync();

            do
            {
                if (groupResponse.NextPageRequest != null)
                {
                    groupResponse = await groupResponse.NextPageRequest.GetAsync();
                }

                AzureADGroupsCount += groupResponse.Count;
            }
            while (groupResponse.NextPageRequest != null && groupResponse.Count > 0);

            // 2. Get user group memberships
            var getUserMemberGroupsRequest = graphserviceClient
                .Me // OR .Users[exampleUserId]
                .GetMemberGroups(true)
                .Request();
            var getUserMemberGroupsResponse = await getUserMemberGroupsRequest.PostAsync();

            // 3. Get user properties
            var getUserRequest = graphserviceClient
                .Me // OR .Users[exampleUserId]
                .Request()
                .Select(u => new { u.DisplayName, u.Country, u.JobTitle, u.Mail, u.Department });
            var getUserResponse = await getUserRequest.GetAsync();

            // 4. Get group members based on group name
            var groupName = "SuperAdmins";
            var groupMembersRequest = graphserviceClient
                .Groups
                .Request()
                .Filter($"displayName eq '{groupName}'")
                .Expand(g => g.Members);
            var groupMembersResponse = await groupMembersRequest.GetAsync();
        }
    }
}