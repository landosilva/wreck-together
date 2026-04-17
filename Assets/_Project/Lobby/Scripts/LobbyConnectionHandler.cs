namespace WreckTogether.Lobby
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Eflatun.SceneReference;
    using Photon.Deterministic;
    using Photon.Realtime;
    using Quantum;
    using UnityEngine;
    using WreckTogether.Shared;

    public class LobbyConnectionHandler : MonoBehaviour
    {
        [Tooltip("Scene loaded when all clients detect the game-start signal.")]
        [SerializeField] private SceneReference _gameplayScene;

        [Tooltip("Max players allowed per room and player-list refresh interval.")]
        [SerializeField] private WreckTogether.Tuning.NetworkTuning _network;

        [Tooltip("Map, game config, and player prototype driving the match.")]
        [SerializeField] private WreckTogether.Tuning.SessionAssets _sessionAssets;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private WreckTogether.Shared.SceneLoader _sceneLoader;

        [Tooltip("Room custom-property key used to signal game start across all clients.")]
        [SerializeField] private string _gameStartedKey = "started";

        private RealtimeClient _client;
        private QuantumRunner _runner;
        private CancellationTokenSource _cancellation;
        private bool _gameStarting;
        private string _nickname;
        private int _characterIndex;

        private void Awake() => DontDestroyOnLoad(gameObject);

        public int CharacterIndex { get => _characterIndex; set => _characterIndex = value; }
        public bool IsConnected => _client is { IsConnected: true };
        public bool IsInRoom => _client?.CurrentRoom != null;
        public bool IsGameStarting => _gameStarting;
        public string RoomName => _client?.CurrentRoom?.Name;
        public RealtimeClient Client => _client;
        public QuantumRunner Runner => _runner;

        public async Task<bool> ConnectToRoomAsync(string roomName, bool creating, string nickname)
        {
            if (_client is { IsConnected: true })
            {
                Debug.LogWarning("[LobbyConnection] Already connected.");
                return false;
            }

            _cancellation = new CancellationTokenSource();
            _gameStarting = false;
            _nickname = nickname;

            var appSettings = PhotonServerSettings.Global.AppSettings;
            var arguments = new MatchmakingArguments
            {
                PhotonSettings = new AppSettings(appSettings),
                MaxPlayers = _network.MaxPlayers,
                RoomName = roomName,
                CanOnlyJoin = !creating,
                PluginName = "QuantumPlugin",
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
                _client.NickName = _nickname;
                Debug.Log($"[LobbyConnection] Connected to room: {_client.CurrentRoom.Name} as {_nickname}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyConnection] Connection failed: {e.Message}");
                await CleanupAsync();
                return false;
            }
        }

        public bool SignalGameStart()
        {
            if (_client?.CurrentRoom == null)
            {
                Debug.LogError("[LobbyConnection] Not in a room.");
                return false;
            }

            var props = new Photon.Client.PhotonHashtable { { _gameStartedKey, true } };
            _client.CurrentRoom.SetCustomProperties(props);
            Debug.Log("[LobbyConnection] Host signaled game start.");
            return true;
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
                await _sceneLoader.LoadSceneAsync(_gameplayScene.Name);

                var sessionArgs = new SessionRunner.Arguments
                {
                    RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                    GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                    ClientId = _client.UserId ?? Guid.NewGuid().ToString(),
                    RuntimeConfig = SessionFactory.BuildRuntimeConfig(_sessionAssets),
                    SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                    GameMode = DeterministicGameMode.Multiplayer,
                    PlayerCount = _network.MaxPlayers,
                    Communicator = new QuantumNetworkCommunicator(_client),
                    CancellationToken = _cancellation?.Token ?? CancellationToken.None,
                };

                _runner = (QuantumRunner)await SessionRunner.StartAsync(sessionArgs);

                var runtimePlayer = new RuntimePlayer
                {
                    PlayerNickname = _nickname,
                    CharacterIndex = _characterIndex,
                };
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

        public async Task DisconnectAsync() => await CleanupAsync();

        private async Task CleanupAsync()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            _gameStarting = false;

            if (_runner != null)
            {
                try { await _runner.ShutdownAsync(); }
                catch (Exception e) { Debug.LogException(e); }
                _runner = null;
            }

            if (_client != null)
            {
                try { if (_client.IsConnected) await _client.DisconnectAsync(); }
                catch (Exception e) { Debug.LogException(e); }
                _client = null;
            }
        }

        private void Update()
        {
            _client?.Service();
            CheckForGameStart();
        }

        private void CheckForGameStart()
        {
            if (_gameStarting) return;
            if (_client?.CurrentRoom == null) return;

            var props = _client.CurrentRoom.CustomProperties;
            if (props == null || !props.ContainsKey(_gameStartedKey)) return;
            if (!(bool)props[_gameStartedKey]) return;

            _gameStarting = true;
            Debug.Log("[LobbyConnection] Detected game start signal.");
            _ = StartGameAsync();
        }

        private void OnDestroy()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _runner?.Shutdown();
            if (_client is { IsConnected: true })
                _client.Disconnect();
        }
    }
}
