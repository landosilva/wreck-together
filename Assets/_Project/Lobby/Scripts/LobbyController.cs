namespace WreckTogether.Lobby
{
    using System.Collections.Generic;
    using Eflatun.SceneReference;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private LobbyConnectionHandler _connectionHandler;
        [SerializeField] private SceneReference _menuScene;
        [SerializeField] private float _playerListRefreshInterval = 0.5f;

        private TextField _nicknameField;
        private TextField _roomNameField;
        private Button _createButton;
        private Button _joinButton;
        private Button _startButton;
        private Button _backButton;
        private Label _statusLabel;
        private VisualElement _connectPanel;
        private VisualElement _roomPanel;
        private VisualElement _playerList;

        private float _nextRefreshTime;
        private int _lastPlayerCount;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            _nicknameField = root.Q<TextField>("nickname-field");
            _roomNameField = root.Q<TextField>("room-name-field");

            // Restore saved nickname
            _nicknameField.value = PlayerPrefs.GetString("PlayerNickname", "");
            _createButton = root.Q<Button>("create-button");
            _joinButton = root.Q<Button>("join-button");
            _startButton = root.Q<Button>("start-button");
            _backButton = root.Q<Button>("back-button");
            _statusLabel = root.Q<Label>("status-label");
            _connectPanel = root.Q<VisualElement>("connect-panel");
            _roomPanel = root.Q<VisualElement>("room-panel");
            _playerList = root.Q<VisualElement>("player-list");

            _createButton.clicked += OnCreateClicked;
            _joinButton.clicked += OnJoinClicked;
            _startButton.clicked += OnStartClicked;
            _backButton.clicked += OnBackClicked;

            ShowConnectPanel();
        }

        private void OnDisable()
        {
            _createButton.clicked -= OnCreateClicked;
            _joinButton.clicked -= OnJoinClicked;
            _startButton.clicked -= OnStartClicked;
            _backButton.clicked -= OnBackClicked;
        }

        private void Update()
        {
            if (!_connectionHandler.IsInRoom) return;
            if (Time.time < _nextRefreshTime) return;

            _nextRefreshTime = Time.time + _playerListRefreshInterval;
            RefreshPlayerList();
        }

        private void ShowConnectPanel()
        {
            _connectPanel.style.display = DisplayStyle.Flex;
            _roomPanel.style.display = DisplayStyle.None;
            _statusLabel.text = "Enter a room name to create or join.";
            _lastPlayerCount = 0;
        }

        private void ShowRoomPanel()
        {
            _connectPanel.style.display = DisplayStyle.None;
            _roomPanel.style.display = DisplayStyle.Flex;
            _statusLabel.text = $"Room: {_connectionHandler.RoomName}";

            _gameSessionData.IsConnected = true;
            _gameSessionData.RoomName = _connectionHandler.RoomName;

            _lastPlayerCount = 0;
            RefreshPlayerList();
        }

        private void RefreshPlayerList()
        {
            var client = _connectionHandler.Client;
            var room = client?.CurrentRoom;
            if (room == null) return;

            var players = room.Players;
            if (players.Count == _lastPlayerCount) return;

            _lastPlayerCount = players.Count;
            _playerList.Clear();

            foreach (KeyValuePair<int, Photon.Realtime.Player> kvp in players)
            {
                var player = kvp.Value;
                var displayName = !string.IsNullOrEmpty(player.NickName) ? player.NickName : $"Player {kvp.Key}";
                var label = new Label(displayName);
                label.AddToClassList("wt-player-item");
                _playerList.Add(label);
            }

            _statusLabel.text = $"Room: {_connectionHandler.RoomName} ({players.Count} player{(players.Count != 1 ? "s" : "")})";
            _gameSessionData.ConnectedPlayers.Clear();
            foreach (KeyValuePair<int, Photon.Realtime.Player> entry in players)
            {
                _gameSessionData.ConnectedPlayers.Add(!string.IsNullOrEmpty(entry.Value.NickName) ? entry.Value.NickName : $"Player {entry.Key}");
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _createButton.SetEnabled(enabled);
            _joinButton.SetEnabled(enabled);
            _startButton.SetEnabled(enabled);
            _backButton.SetEnabled(enabled);
        }

        private string GetValidatedNickname()
        {
            var nickname = _nicknameField.value?.Trim();
            if (string.IsNullOrWhiteSpace(nickname))
            {
                _statusLabel.text = "Please enter a nickname.";
                return null;
            }

            PlayerPrefs.SetString("PlayerNickname", nickname);
            return nickname;
        }

        private async void OnCreateClicked()
        {
            var nickname = GetValidatedNickname();
            if (nickname == null) return;

            var roomName = _roomNameField.value;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                _statusLabel.text = "Please enter a room name.";
                return;
            }

            SetButtonsEnabled(false);
            _statusLabel.text = "Creating room...";

            var success = await _connectionHandler.ConnectToRoomAsync(roomName, creating: true, nickname);
            if (success)
            {
                _gameSessionData.IsHost = true;
                ShowRoomPanel();
            }
            else
            {
                _statusLabel.text = "Failed to create room.";
            }

            SetButtonsEnabled(true);
        }

        private async void OnJoinClicked()
        {
            var nickname = GetValidatedNickname();
            if (nickname == null) return;

            var roomName = _roomNameField.value;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                _statusLabel.text = "Please enter a room name.";
                return;
            }

            SetButtonsEnabled(false);
            _statusLabel.text = "Joining room...";

            var success = await _connectionHandler.ConnectToRoomAsync(roomName, creating: false, nickname);
            if (success)
            {
                _gameSessionData.IsHost = false;
                ShowRoomPanel();
            }
            else
            {
                _statusLabel.text = "Failed to join room.";
            }

            SetButtonsEnabled(true);
        }

        private void OnStartClicked()
        {
            SetButtonsEnabled(false);
            _statusLabel.text = "Starting game...";

            if (!_connectionHandler.SignalGameStart())
            {
                _statusLabel.text = "Failed to start game.";
                SetButtonsEnabled(true);
            }
            // All clients (including host) will detect the room property
            // and start via LobbyConnectionHandler.CheckForGameStart()
        }

        private async void OnBackClicked()
        {
            SetButtonsEnabled(false);
            _statusLabel.text = "Disconnecting...";

            await _connectionHandler.DisconnectAsync();
            _gameSessionData.Clear();

            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }
    }
}
