namespace WreckTogether.Tuning
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Tuning/Network", fileName = "NetworkTuning")]
    public class NetworkTuning : ScriptableObject
    {
        [Tooltip("Maximum players allowed per Photon room.")]
        public int MaxPlayers = 4;

        [Tooltip("Seconds between player-list refreshes while in the lobby.")]
        public float PlayerListRefreshIntervalSeconds = 0.5f;
    }
}
