using Microsoft.Graph;

namespace MicrosoftGraphSDK


{
    public class Team
    {
        public static async Task<IUserJoinedTeamsCollectionPage> GetJoinedTeams(
            GraphServiceClient client
            )
        {
            try
            {
                return await client.Me.JoinedTeams
                 .Request()
                 .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetJoinedTeams call: {ex.Message}");
            }
        }
    }
}
