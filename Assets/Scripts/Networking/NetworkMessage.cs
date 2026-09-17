using UnityEngine;

public static class NetworkMessage
{
    public static string Fire(int x, int y) => $"FIRE:{x},{y}";
    public static string Result(bool hit, int x, int y) => $"RESULT:{(hit ? "HIT" : "MISS")}:{x},{y}";
    public static string Turn(bool yourTurn) => yourTurn ? "TURN:YOU" : "TURN:OPPONENT";
    public static string GameOver(bool youWin) => youWin ? "GAMEOVER:YOU_WIN" : "GAMEOVER:YOU_LOSE";

    public static (string type, string[] args) Parse(string message)
    {
        string[] parts = message.Split(':');
        string type = parts[0];
        string[] args = parts.Length > 1 ? parts[1..] : new string[0];
        return (type, args);
    }
}
