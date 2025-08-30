namespace GoServer
{
  public static class Gls     // Globals, but a shorter name
  {
    // locking https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
    // looks like you can do multiple locks too: https://stackoverflow.com/questions/5975664/how-to-lock-several-objects
    public static object usersLock = new object();    // Locking users list
    public static List<User> users = new();
  }
  // A game is underway when a player has an opponent and the oppnent's opponent is the player
  // when setting up, the challenged player's opponent is the challenger, the challenger's opponent is blank
  public class User
  {
    public string Name { get; set; }
    public string ConnectionId { get; set; }
    public string Opponent { get; set; }    // opponent name or blank
    public User(string name, string connectionId)
    {
      Name = name;
      ConnectionId = connectionId;
      Opponent = "";
    }
  }
}
