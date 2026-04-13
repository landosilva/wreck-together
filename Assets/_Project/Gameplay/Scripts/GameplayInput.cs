namespace WreckTogether.Gameplay
{
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GameplayInput : MonoBehaviour
    {
        [SerializeField] private float _mouseSensitivity = 0.15f;
        [SerializeField] private float _gamepadLookSensitivity = 120f;

        private PlayerInputActions _inputActions;
        private float _yaw;
        private float _pitch;

        public float Yaw => _yaw;
        public float Pitch => _pitch;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _yaw = 0f;
            _pitch = 0f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => OnPollInput(callback));
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            var lookDelta = _inputActions.Player.Look.ReadValue<Vector2>();


            var activeControl = _inputActions.Player.Look.activeControl;
            if (activeControl != null && activeControl.device is Gamepad)
            {
                _yaw += lookDelta.x * _gamepadLookSensitivity * Time.deltaTime;
                _pitch -= lookDelta.y * _gamepadLookSensitivity * Time.deltaTime;
            }
            else
            {
                _yaw += lookDelta.x * _mouseSensitivity;
                _pitch -= lookDelta.y * _mouseSensitivity;
            }

            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
        }

        private void OnPollInput(CallbackPollInput callback)
        {
            var input = new Quantum.Input();

            var move = _inputActions.Player.Move.ReadValue<Vector2>();
            input.Movement = new FPVector2(
                FP.FromFloat_UNSAFE(move.x),
                FP.FromFloat_UNSAFE(move.y)
            );

            input.Yaw = FP.FromFloat_UNSAFE(_yaw);
            input.Pitch = FP.FromFloat_UNSAFE(_pitch * Mathf.Deg2Rad);

            callback.SetInput(input, DeterministicInputFlags.Repeatable);
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }
    }
}
