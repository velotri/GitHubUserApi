using GitHubUserApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var rateLimitReachedErrorMessage = "GitHub API rate limit has been reached. Try again later.";
var notFoundErrorResponse = new
{
    Status = 404,
    Detail = "User does not exist."
};

app.MapGet("/repositories/{user}", async (string user, UserService userService) =>
{
    try
    {
        var repositories = await userService.GetUserRepositories(user);
        return Results.Ok(repositories);
    }
    catch (Octokit.RateLimitExceededException)
    {
        return Results.Problem(statusCode: 503, detail: rateLimitReachedErrorMessage);
    }
    catch (Octokit.NotFoundException)
    {
        return Results.NotFound(notFoundErrorResponse);
    }
})
.WithName("GetUserRepositories")
.Produces<UserRepository[]>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status503ServiceUnavailable);

app.MapGet("/languages/{user}", async (string user, UserService userService) =>
{
    try
    {
        var repositories = await userService.GetUserLanguages(user);
        return Results.Ok(repositories);
    }
    catch (Octokit.RateLimitExceededException)
    {
        return Results.Problem(statusCode: 503, detail: rateLimitReachedErrorMessage);
    }
    catch (Octokit.NotFoundException)
    {
        return Results.NotFound(notFoundErrorResponse);
    }
})
.WithName("GetUserLanguages")
.Produces<UserLanguage[]>()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status503ServiceUnavailable);

app.Run();