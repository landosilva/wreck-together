namespace WreckTogether.Shared
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Game Session Data")]
    public class GameSessionData : ScriptableObject
    {
        public bool IsConnected;
        public bool IsHost;
        public string RoomName;
        public List<string> ConnectedPlayers = new();
        public int FinalScore;
        public float MatchDuration;

        public void Clear()
        {
            IsConnected = false;
            IsHost = false;
            RoomName = null;
            ConnectedPlayers.Clear();
            FinalScore = 0;
            MatchDuration = 0f;
        }

        private void OnEnable()
        {
            Clear();
        }
    }
}
