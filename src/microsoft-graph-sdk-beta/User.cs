using Microsoft.Graph.Beta;

namespace microsoft_graph_sdk
{
    public class User
    {
        public static async Task<Microsoft.Graph.Beta.Models.User> getUserInfo(
            GraphServiceClient client)
        {
            return await client.Me
                .GetAsync();
        }
    }
}
