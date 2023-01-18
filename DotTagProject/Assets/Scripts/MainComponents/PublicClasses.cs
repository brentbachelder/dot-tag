public class GamePlayerPositions
{
    public int xPos = 0;
    public int yPos = 0;
}

public class GamePlayerVariables
{
    public int It = 0;
    public int Lives = 0;
}

public class GamePlayerStatic
{
    public string ID;
    public int Color;
    public int Pause = 0;
}

public class GamePlayerManager
{
    public int Controller = 0;
    public int Owner = 0;
    public string GameMessage = "empty";
}

public class GameSettings
{
    public int PlayerSpeed = 0;
    public int CameraZoom = 1;
    public int GameType = 0;
    public int TimeLimit = 90;
    public int CurrentPlayers = 0;
    public int MaxPlayers = 6;
    public string Status = "open";
    public string Type = "public";
    public int NextInLine = 0;
    public string GameID;
}