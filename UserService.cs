using Octokit;

namespace GitHubUserApi
{
    public record UserRepository(string Name, string[] Languages);
    public record UserLanguage(string Name, double Percentage);

    /// <summary>
    /// A service for fetching information about repositories and languages of GitHub users.
    /// </summary>
    public class UserService
    {
        private record UserData(UserRepository[] UserRepositories, UserLanguage[] UserLanguages, DateTime CacheExpiration);

        private readonly IConfiguration configuration;
        private readonly GitHubClient client;
        private readonly Dictionary<string, UserData> userDataCache;

        public UserService(IConfiguration configuration)
        {
            this.configuration = configuration;
            client = new GitHubClient(new ProductHeaderValue(configuration["GitHubClient:UserAgent"]));
            var personalAccessToken = configuration["GitHubClient:PersonalAccessToken"];
            if (!string.IsNullOrEmpty(personalAccessToken))
            {
                client.Credentials = new Credentials(personalAccessToken);
            }
            userDataCache = new Dictionary<string, UserData>();
        }

        /// <summary>
        /// Gets all repositories of the specified user.
        /// </summary>
        /// <remarks>
        /// Results are cached for configurable amount of time. If cache is expired or does not exists, new data is fetched from GitHub API.
        /// </remarks>
        /// <returns>
        /// Repository names and languages used in a given repository.
        /// </returns>
        /// <exception cref="RateLimitExceededException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserRepository[]> GetUserRepositories(string user)
        {
            await FetchUserDataIfNeeded(user);

            return userDataCache[user].UserRepositories;
        }

        /// <summary>
        /// Gets all languages used by the specified user. 
        /// </summary>
        /// <remarks>
        /// Results are cached for configurable amount of time. If cache is expired or does not exists, new data is fetched from GitHub API.
        /// </remarks>
        /// <returns>
        /// Language names and their percentage share in total bytes of all repositories of the specified user.
        /// </returns>
        /// <exception cref="RateLimitExceededException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserLanguage[]> GetUserLanguages(string user)
        {
            await FetchUserDataIfNeeded(user);

            return userDataCache[user].UserLanguages;
        }

        private async Task FetchUserDataIfNeeded(string user)
        {
            if (userDataCache.ContainsKey(user) && userDataCache[user].CacheExpiration > DateTime.UtcNow)
            {
                return;
            }

            var repos = await client.Repository.GetAllForUser(user);

            var userRepositories = new List<UserRepository>();
            var languagesToBytes = new Dictionary<string, long>();

            foreach (var repo in repos)
            {
                var languages = await client.Repository.GetAllLanguages(repo.Id);

                foreach (var language in languages)
                {
                    if (languagesToBytes.ContainsKey(language.Name))
                    {
                        languagesToBytes[language.Name] += language.NumberOfBytes;
                    }
                    else
                    {
                        languagesToBytes.Add(language.Name, language.NumberOfBytes);
                    }
                }

                var languageNames = languages.Select(o => o.Name).ToArray();
                userRepositories.Add(new UserRepository(repo.Name, languageNames));
            }

            var totalBytes = languagesToBytes.Sum(o => o.Value);
            var userLanguages = languagesToBytes
                .Select(o => new UserLanguage(o.Key, 100.0 * o.Value / totalBytes))
                .OrderByDescending(o => o.Percentage)
                .ToArray();

            var dataExpirationInMinutes = int.Parse(configuration["GitHubClient:DataExpirationInMinutes"]);
            userDataCache[user] = new UserData(userRepositories.ToArray(), userLanguages, DateTime.UtcNow.AddMinutes(dataExpirationInMinutes));
        }
    }
}
