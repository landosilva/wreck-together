namespace WreckTogether.Gameplay
{
    using System;
    using System.Threading.Tasks;
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;
    using WreckTogether.Shared;
    using WreckTogether.Tuning;

    /// <summary>
    /// Guarantees a Quantum session exists when the Gameplay scene starts. If one
    /// is already running (we came from the lobby flow) this is a no-op. If not
    /// (designer hit Play on Gameplay.unity directly) we spin up an offline local
    /// session with default character and nickname so every scene is independently
    /// runnable.
    /// </summary>
    public class SessionBootstrap : MonoBehaviour
    {
        [Tooltip("Quantum assets used to build the offline session's RuntimeConfig.")]
        [SerializeField] private SessionAssets _sessionAssets;

        [Tooltip("Character index selected for the offline local player.")]
        [SerializeField] private int _offlineCharacterIndex = 0;

        [Tooltip("Nickname for the offline local player.")]
        [SerializeField] private string _offlineNickname = "Player";

        private async void Start()
        {
            if (QuantumRunner.Default != null) return;
            await StartOfflineSession();
        }

        private async Task StartOfflineSession()
        {
            var args = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = $"OfflinePlayer_{Guid.NewGuid().ToString()[..8]}",
                RuntimeConfig = SessionFactory.BuildRuntimeConfig(_sessionAssets),
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Local,
                PlayerCount = 1,
            };

            var runner = (QuantumRunner)await SessionRunner.StartAsync(args);

            var runtimePlayer = new RuntimePlayer
            {
                PlayerNickname = _offlineNickname,
                CharacterIndex = _offlineCharacterIndex,
            };
            runner.Game.AddPlayer(0, runtimePlayer);

            Debug.Log("[SessionBootstrap] Offline Quantum session started.");
        }
    }
}
