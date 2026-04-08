namespace WreckTogether.Lobby
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Photon.Deterministic;
    using Photon.Realtime;
    using Quantum;
    using Quantum.Menu;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class LobbyConnectionHandler : MonoBehaviour
    {
        [SerializeField] private string _gameplaySceneName = "Gameplay";
        [SerializeField] private int _maxPlayers = 4;

        private RealtimeClient _client;
        private QuantumRunner _runner;
        private CancellationTokenSource _cancellation;
        private string _loadedScene;

        public bool IsConnected => _client is { IsConnected: true };
        public bool IsInRoom => _client?.CurrentRoom != null;
        public string RoomName => _client?.CurrentRoom?.Name;
        public RealtimeClient Client => _client;
        public QuantumRunner Runner => _runner;

        public async Task<bool> ConnectToRoomAsync(string roomName, bool creating)
        {
            if (_client is { IsConnected: true })
            {
                Debug.LogWarning("[LobbyConnection] Already connected.");
                return false;
            }

            _cancellation = new CancellationTokenSource();

            var appSettings = PhotonServerSettings.Global.AppSettings;
            var arguments = new MatchmakingArguments
            {
                PhotonSettings = new AppSettings(appSettings),
                MaxPlayers = _maxPlayers,
                RoomName = roomName,
                CanOnlyJoin = !creating,
                AsyncConfig = new AsyncConfig
                {
                    TaskFactory = AsyncConfig.CreateUnityTaskFactory(),
                    CancellationToken = _cancellation.Token
                },
                AuthValues = new AuthenticationValues
                {
                    UserId = $"Player_{Guid.NewGuid().ToString()[..8]}"
                }
            };

            try
            {
                _client = await MatchmakingExtensions.ConnectToRoomAsync(arguments);
                Debug.Log($"[LobbyConnection] Connected to room: {_client.CurrentRoom.Name}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyConnection] Connection failed: {e.Message}");
                await CleanupAsync();
                return false;
            }
        }

        public async Task<bool> StartGameAsync()
        {
            if (_client?.CurrentRoom == null)
            {
                Debug.LogError("[LobbyConnection] Not in a room.");
                return false;
            }

            try
            {
                // Load gameplay scene additively
                await SceneManager.LoadSceneAsync(_gameplaySceneName, LoadSceneMode.Additive);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_gameplaySceneName));
                _loadedScene = _gameplaySceneName;

                // Build RuntimeConfig
                var runtimeConfig = new RuntimeConfig();
                runtimeConfig.Seed = Guid.NewGuid().GetHashCode();

                var defaultConfigs = QuantumDefaultConfigs.Global;
                if (defaultConfigs != null)
                {
                    runtimeConfig.SimulationConfig = defaultConfigs.SimulationConfig;
                }

                // Start Quantum session
                var sessionArgs = new SessionRunner.Arguments
                {
                    RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                    GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                    ClientId = _client.UserId ?? Guid.NewGuid().ToString(),
                    RuntimeConfig = runtimeConfig,
                    SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                    GameMode = DeterministicGameMode.Multiplayer,
                    PlayerCount = _maxPlayers,
                    Communicator = new QuantumNetworkCommunicator(_client),
                    CancellationToken = _cancellation?.Token ?? CancellationToken.None,
                };

                _runner = (QuantumRunner)await SessionRunner.StartAsync(sessionArgs);

                // Add local player
                var runtimePlayer = new RuntimePlayer();
                _runner.Game.AddPlayer(0, runtimePlayer);

                Debug.Log("[LobbyConnection] Game started.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyConnection] Start game failed: {e.Message}");
                await CleanupAsync();
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            await CleanupAsync();
        }

        private async Task CleanupAsync()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;

            if (_runner != null)
            {
                try
                {
                    await _runner.ShutdownAsync();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                _runner = null;
            }

            if (_client != null)
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        await _client.DisconnectAsync();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                _client = null;
            }

            if (!string.IsNullOrEmpty(_loadedScene))
            {
                try
                {
                    var scene = SceneManager.GetSceneByName(_loadedScene);
                    if (scene.isLoaded)
                    {
                        await SceneManager.UnloadSceneAsync(_loadedScene);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                _loadedScene = null;
            }
        }

        private void OnDestroy()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _runner?.Shutdown();
            if (_client is { IsConnected: true })
            {
                _client.Disconnect();
            }
        }
    }
}
