using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using System.Linq;
using System.Net;
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

            var groupId = groupResponse.First().Id;

            // Fetch users transitive group memberships
            var getUserMemberGroupsRequest = graphserviceClient
                .Me
                .TransitiveMemberOf
                .Request()
                .Filter($"id eq '{groupId}'")
                .Top(1);
            try
            {
                await getUserMemberGroupsRequest.GetAsync();

                // Use is transitive member of this group.
                UserIsPartOfGroup = true;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    // Other failure scenario
                    throw;
                }
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