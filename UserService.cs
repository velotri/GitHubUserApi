namespace GitHubUserApi
{
    public class UserService
    {
        public IEnumerable<UserRepository> GetUserRepositories(string user)
        {
            return new UserRepository[] {
                new UserRepository("GitHubUserApi", new string[] {
                    "C#"
                })
            };
        }

        public IEnumerable<UserLanguage> GetUserLanguages(string user)
        {
            return new UserLanguage[]
            {
                new UserLanguage("C#", 100.0)
            };
        }
    }
}
