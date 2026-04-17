namespace WreckTogether.Gameplay
{
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using WreckTogether.Shared;
    using WreckTogether.Tuning;

    public class GameplayInput : MonoBehaviour
    {
        [Tooltip("Look-related tuning (pitch clamp, default sensitivities, base FOV).")]
        [SerializeField] private InputTuning _input;

        private PlayerInputActions _inputActions;
        private float _yaw;
        private float _pitch;
        private bool _lookEnabled = true;

        public float Yaw => _yaw;
        public float Pitch => _pitch;
        public float CurrentLookFOV { get; set; }

        public bool LookEnabled
        {
            get => _lookEnabled;
            set { _lookEnabled = value; ApplyCursorState(); }
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            CurrentLookFOV = _input.BaseFOV;
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _yaw = 0f;
            _pitch = 0f;
            ApplyCursorState();
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            QuantumCallback.Subscribe(this, (CallbackPollInput cb) => OnPollInput(cb));
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy() => _inputActions?.Dispose();

        private void Update()
        {
            if (!_lookEnabled) return;

            var delta = _inputActions.Player.Look.ReadValue<Vector2>();
            if (IsGamepadActive())
                AccumulateGamepadLook(delta);
            else
                AccumulateMouseLook(delta);

            _pitch = Mathf.Clamp(_pitch, -_input.MaxPitchDegrees, _input.MaxPitchDegrees);
        }

        private void AccumulateMouseLook(Vector2 delta)
        {
            // Mouse.delta is accumulated per-poll — no Time.deltaTime needed
            var fovScale = PlayerSettings.ScaleSensitivityWithFOV
                ? LookSensitivity.FovScale(CurrentLookFOV, _input.BaseFOV)
                : 1f;
            var sens = PlayerSettings.MouseDegreesPerCount * LookSensitivity.DpiScale() * fovScale;

            _yaw   += delta.x * sens;
            _pitch += delta.y * sens * PitchSign();
        }

        private void AccumulateGamepadLook(Vector2 delta)
        {
            var sens = PlayerSettings.GamepadLookSensitivity * Time.deltaTime;
            _yaw   += delta.x * sens;
            _pitch += delta.y * sens * PitchSign();
        }

        private static float PitchSign() => PlayerSettings.InvertMouseY ? 1f : -1f;

        private bool IsGamepadActive()
        {
            var control = _inputActions.Player.Look.activeControl;
            return control != null && control.device is Gamepad;
        }

        private void OnPollInput(CallbackPollInput callback)
        {
            var move = _inputActions.Player.Move.ReadValue<Vector2>();
            var input = new Quantum.Input
            {
                Movement = new FPVector2(FP.FromFloat_UNSAFE(move.x), FP.FromFloat_UNSAFE(move.y)),
                Yaw      = FP.FromFloat_UNSAFE(_yaw),
                Pitch    = FP.FromFloat_UNSAFE(_pitch * Mathf.Deg2Rad),
            };
            callback.SetInput(input, DeterministicInputFlags.Repeatable);
        }

        private void ApplyCursorState()
        {
            Cursor.lockState = _lookEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !_lookEnabled;
        }
    }
}
