# GitHubUserApi

API for retrieving information about repositories and programming languages of GitHub users.

Implemented as Minimal API in NET 6.0.

Live demo available at:  
[https://githubuserapi.azurewebsites.net/swagger/index.html](https://githubuserapi.azurewebsites.net/swagger/index.html)

## Run locally

- Install [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

- Enter project directory with command line.

- (Optional) Provide GitHub personal access key. 

    Unauthorized access to GitHub API is limited to 60 requests per hour from single IP.
    
    - Enter [https://github.com/settings/tokens](https://github.com/settings/tokens) and generate new token with default (public) permissions.
    
    - Run command:
    
    ```console
    dotnet user-secrets set "GitHubClient:PersonalAccessToken" "your-token"
    ```
    
    Where *your-token* is your generated personal access token.

- Run application with:

```console
dotnet run
```

## Endpoints

Results of both endpoints are cached for configurable amount of time - 10 minutes by default. If cache is expired or does not exists, new data is fetched from GitHub API.

If GitHub API rate limit is reached, endpoints return *503 Service Unavailable*.

If specified user does not exists, endpoints return *404 Not Found*.

SwaggerUI is available at:

```
/swagger/index.html
```

### GetUserRepositories

Gets all repositories of the specified user.

Returns repository names and languages used in a given repository.

```http
GET /repositories/{user}
```

### GetUserLanguages

Gets all languages used by the specified user. 

Returns language names and their percentage share in total bytes of all repositories of the specified user.

```http
GET /languages/{user}
```

## Configuration

Cache lifetime can be changed in `appsettings.json` with `GitHubClient:DataExpirationInMinutes` key.

By default it is set to 10 minutes.

## Potential Improvements

It is possible to support conditional requests with `ETag` & `If-None-Match` headers. Conditional requests for already fetched unchanged data would not count against rate limit.

It would require implementing custom GitHub API client, as official [Octokit.NET](https://github.com/octokit/octokit.net) does not support conditional requests (as of 05.01.22).
