namespace WreckTogether.Lobby
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private LobbyConnectionHandler _connectionHandler;
        [SerializeField] private string _menuSceneName = "Menu";

        private TextField _roomNameField;
        private Button _createButton;
        private Button _joinButton;
        private Button _startButton;
        private Button _backButton;
        private Label _statusLabel;
        private VisualElement _connectPanel;
        private VisualElement _roomPanel;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            _roomNameField = root.Q<TextField>("room-name-field");
            _createButton = root.Q<Button>("create-button");
            _joinButton = root.Q<Button>("join-button");
            _startButton = root.Q<Button>("start-button");
            _backButton = root.Q<Button>("back-button");
            _statusLabel = root.Q<Label>("status-label");
            _connectPanel = root.Q<VisualElement>("connect-panel");
            _roomPanel = root.Q<VisualElement>("room-panel");

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

        private void ShowConnectPanel()
        {
            _connectPanel.style.display = DisplayStyle.Flex;
            _roomPanel.style.display = DisplayStyle.None;
            _statusLabel.text = "Enter a room name to create or join.";
        }

        private void ShowRoomPanel()
        {
            _connectPanel.style.display = DisplayStyle.None;
            _roomPanel.style.display = DisplayStyle.Flex;
            _statusLabel.text = $"Room: {_connectionHandler.RoomName}";

            _gameSessionData.IsConnected = true;
            _gameSessionData.RoomName = _connectionHandler.RoomName;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _createButton.SetEnabled(enabled);
            _joinButton.SetEnabled(enabled);
            _startButton.SetEnabled(enabled);
            _backButton.SetEnabled(enabled);
        }

        private async void OnCreateClicked()
        {
            var roomName = _roomNameField.value;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                _statusLabel.text = "Please enter a room name.";
                return;
            }

            SetButtonsEnabled(false);
            _statusLabel.text = "Creating room...";

            var success = await _connectionHandler.ConnectToRoomAsync(roomName, creating: true);
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
            var roomName = _roomNameField.value;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                _statusLabel.text = "Please enter a room name.";
                return;
            }

            SetButtonsEnabled(false);
            _statusLabel.text = "Joining room...";

            var success = await _connectionHandler.ConnectToRoomAsync(roomName, creating: false);
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

        private async void OnStartClicked()
        {
            SetButtonsEnabled(false);
            _statusLabel.text = "Starting game...";

            var success = await _connectionHandler.StartGameAsync();
            if (success)
            {
                // Unload lobby scene — gameplay is already loaded additively
                await _sceneLoader.UnloadSceneAsync("Lobby");
            }
            else
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
            _gameSessionData.Clear();

            await _sceneLoader.LoadSceneAsync(_menuSceneName);
        }
    }
}
