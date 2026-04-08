namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
  using UnityEngine.InputSystem;
#endif
  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDemoInputPlatformer2DPolling : MonoBehaviour {
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
    private InputAction _move;
    private InputAction _fire1;
    private InputAction _fire2;
    private InputAction _fire3;
    private InputAction _jump;
#endif

    /// <summary>
    /// Get or create the PlayerInput component. Uses the Quantum default input action asset.
    /// Caches the individual input actions to query inside the Quantum input polling callback.
    /// </summary>
    private void Awake() {
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
      if (TryGetComponent<PlayerInput>(out var playerInput) == false) {
        playerInput = gameObject.AddComponent<PlayerInput>();
        playerInput.actions = QuantumDefaultConfigs.Global.InputActionAsset;
        playerInput.actions.Enable();
      }

      _move = playerInput.actions["Move"];
      _fire1 = playerInput.actions["Fire1"];
      _fire2 = playerInput.actions["Fire2"];
      _fire3 = playerInput.actions["Fire3"];
      _jump = playerInput.actions["Jump"];
#endif
    }

    /// <summary>
    /// Register Quantum input polling callback.
    /// </summary>
    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    /// <summary>
    /// Set an empty input when polled by the simulation.
    /// </summary>
    /// <param name="callback"></param>
    public void PollInput(CallbackPollInput callback) {
      QuantumDemoInputPlatformer2D pInput = default;

#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
      var move = _move.ReadValue<Vector2>().ToFPVector2();
      pInput.Left = move.X < 0;
      pInput.Right = move.Y > 0;
      pInput.Down = move.Y < 0;
      pInput.Up = move.Y > 0;

      pInput.Jump = _jump.IsPressed();
      pInput.Fire = _fire1.IsPressed();
      pInput.Dash = _fire2.IsPressed();
      pInput.Use = _fire3.IsPressed();

      // grab this using mouse, etc
      pInput.AimDirection = default;
#elif ENABLE_LEGACY_INPUT_MANAGER
      var x = UnityEngine.Input.GetAxis("Horizontal");
      pInput.Left = x < 0;
      pInput.Right = x > 0;
      var y = UnityEngine.Input.GetAxis("Vertical");
      pInput.Down = y < 0;
      pInput.Up = y > 0;

      pInput.Jump = UnityEngine.Input.GetButton("Jump");
      pInput.Dash = UnityEngine.Input.GetButton("Fire2");
      pInput.Fire = UnityEngine.Input.GetButton("Fire1");
      pInput.Use = UnityEngine.Input.GetButton("Fire3");

      // grab this using mouse, etc
      pInput.AimDirection = default;
#endif

      // implicitly casts to base input
      callback.SetInput(pInput, DeterministicInputFlags.Repeatable);
    }
  }
}