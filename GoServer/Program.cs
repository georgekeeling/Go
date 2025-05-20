using SignalRChat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR(); // Add SignalR services
builder.Services.AddControllers(); // Add controllers if needed

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// next 2 added by CoPilot
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  // swagger was added by CoPilot
  //app.UseSwagger();
  //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Map SignalR hubs
app.MapHub<ChatHub>("/ChatHub");

// Map controllers if needed
app.MapControllers();

app.Run();
