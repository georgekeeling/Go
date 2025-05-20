using GoCarta;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
ModScore.Board[0, 0] = 1;
app.Run();
