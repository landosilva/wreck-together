namespace WreckTogether.Lobby
{
    using System.Collections.Generic;
    using Eflatun.SceneReference;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;
    using WreckTogether.Tuning;

    public class LobbyController : MonoBehaviour
    {
        [Tooltip("UI Document containing lobby panels and buttons.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Manages Photon connection, room creation, and Quantum session startup.")]
        [SerializeField] private LobbyConnectionHandler _connectionHandler;

        [Tooltip("Scene loaded when the player presses Back.")]
        [SerializeField] private SceneReference _menuScene;

        [Tooltip("Max players and player-list refresh interval.")]
        [SerializeField] private NetworkTuning _network;

        [Tooltip("Database of selectable characters shown in the lobby.")]
        [SerializeField] private CharacterDatabase _characterDatabase;

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
        private VisualElement _characterSelector;
        private List<Button> _charButtons = new();

        private float _nextRefreshTime;
        private int _lastPlayerCount;
        private int _selectedCharacterIndex;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            _nicknameField = root.Q<TextField>("nickname-field");
            _roomNameField = root.Q<TextField>("room-name-field");
            _nicknameField.value = PlayerPrefs.GetString("PlayerNickname", "");
            _createButton = root.Q<Button>("create-button");
            _joinButton = root.Q<Button>("join-button");
            _startButton = root.Q<Button>("start-button");
            _backButton = root.Q<Button>("back-button");
            _statusLabel = root.Q<Label>("status-label");
            _connectPanel = root.Q<VisualElement>("connect-panel");
            _roomPanel = root.Q<VisualElement>("room-panel");
            _playerList = root.Q<VisualElement>("player-list");
            _characterSelector = root.Q<VisualElement>("character-selector");

            BuildCharacterSelector();

            _createButton.clicked += OnCreateClicked;
            _joinButton.clicked += OnJoinClicked;
            _startButton.clicked += OnStartClicked;
            _backButton.clicked += OnBackClicked;

            SelectCharacter(0);
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

            _nextRefreshTime = Time.time + _network.PlayerListRefreshIntervalSeconds;
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

            GameSession.IsConnected = true;
            GameSession.RoomName = _connectionHandler.RoomName;

            _lastPlayerCount = 0;
            RefreshPlayerList();
        }

        private void RefreshPlayerList()
        {
            var room = _connectionHandler.Client?.CurrentRoom;
            if (room == null) return;

            var players = room.Players;
            if (players.Count == _lastPlayerCount) return;

            _lastPlayerCount = players.Count;
            _playerList.Clear();
            GameSession.ConnectedPlayers.Clear();

            foreach (KeyValuePair<int, Photon.Realtime.Player> kvp in players)
            {
                var displayName = DisplayNameOf(kvp);
                _playerList.Add(CreatePlayerLabel(displayName));
                GameSession.ConnectedPlayers.Add(displayName);
            }

            _statusLabel.text = $"Room: {_connectionHandler.RoomName} ({players.Count} player{(players.Count != 1 ? "s" : "")})";
        }

        private static string DisplayNameOf(KeyValuePair<int, Photon.Realtime.Player> kvp)
        {
            return !string.IsNullOrEmpty(kvp.Value.NickName) ? kvp.Value.NickName : $"Player {kvp.Key}";
        }

        private static Label CreatePlayerLabel(string displayName)
        {
            var label = new Label(displayName);
            label.AddToClassList("wt-player-item");
            return label;
        }

        private void BuildCharacterSelector()
        {
            _characterSelector.Clear();
            _charButtons.Clear();

            var characters = _characterDatabase.GetAll();
            for (int i = 0; i < characters.Length; i++)
            {
                int index = i;
                var character = characters[i];

                var button = new Button(() => SelectCharacter(index));
                button.text = null;
                button.AddToClassList("wt-button");
                button.AddToClassList("wt-button--secondary");
                button.AddToClassList("wt-char-button");

                if (character.Portrait != null)
                {
                    var portrait = new Image();
                    portrait.sprite = character.Portrait;
                    portrait.AddToClassList("wt-char-portrait");
                    button.Add(portrait);
                }

                var label = new Label(character.DisplayName);
                label.AddToClassList("wt-char-label");
                button.Add(label);

                _characterSelector.Add(button);
                _charButtons.Add(button);
            }
        }

        private void SelectCharacter(int index)
        {
            _selectedCharacterIndex = index;
            _connectionHandler.CharacterIndex = index;

            for (int i = 0; i < _charButtons.Count; i++)
            {
                if (i == index)
                    _charButtons[i].AddToClassList("wt-char-button--selected");
                else
                    _charButtons[i].RemoveFromClassList("wt-char-button--selected");
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
                GameSession.IsHost = true;
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
                GameSession.IsHost = false;
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
        }

        private async void OnBackClicked()
        {
            SetButtonsEnabled(false);
            _statusLabel.text = "Disconnecting...";

            await _connectionHandler.DisconnectAsync();
            GameSession.Clear();

            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }
    }
}
