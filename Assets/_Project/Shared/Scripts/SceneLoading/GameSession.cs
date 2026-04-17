namespace WreckTogether.Shared
{
    using System.Collections.Generic;

    /// <summary>
    /// Process-wide runtime state that survives scene loads. Not an asset — plain
    /// static so Editor and Build behave identically and there's no SO mutation
    /// anti-pattern. Cleared at boot by <see cref="WreckTogether.Shared.GameBootstrap"/>.
    /// </summary>
    public static class GameSession
    {
        public static bool IsConnected;
        public static bool IsHost;
        public static string RoomName;
        public static readonly List<string> ConnectedPlayers = new();
        public static int FinalScore;
        public static float MatchDuration;

        public static void Clear()
        {
            IsConnected = false;
            IsHost = false;
            RoomName = null;
            ConnectedPlayers.Clear();
            FinalScore = 0;
            MatchDuration = 0f;
        }
    }
}
