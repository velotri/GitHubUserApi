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

app.MapGet("/repositories/{user}", (string user, UserService userService) =>
{
    return userService.GetUserRepositories(user);
})
.WithName("GetUserRepositories");

app.MapGet("/languages/{user}", (string user, UserService userService) =>
{
    return userService.GetUserLanguages(user);
})
.WithName("GetUserLanguages");

app.Run();