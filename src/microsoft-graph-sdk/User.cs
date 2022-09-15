using Microsoft.Graph;

namespace microsoft_graph_sdk
{
    public class User
    {
        public static async Task<Microsoft.Graph.User> getUserInfo(
            GraphServiceClient client)
        {
            return await client.Me
                .Request()
                .GetAsync();
        }
    }
}
