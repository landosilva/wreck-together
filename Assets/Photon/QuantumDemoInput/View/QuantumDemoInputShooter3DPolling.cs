namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
  using UnityEngine.InputSystem;
#endif

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// Is build on the new input system and does not support the legacy input manager.
  /// </summary>
  public class QuantumDemoInputShooter3DPolling : MonoBehaviour {
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
      QuantumDemoInputShooter3D sInput = default;

#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM

      // no worries with clamping, normalization (all implicit)
      sInput.MoveDirection = _move.ReadValue<Vector2>().ToFPVector2();

      sInput.Jump = _jump.IsPressed();
      sInput.Fire = _fire1.IsPressed();
      sInput.Dash = _fire2.IsPressed();
      sInput.Use = _fire3.IsPressed();

      // grab this using local mouse, etc
      sInput.Yaw = default;
      sInput.Pitch = default;
#elif ENABLE_LEGACY_INPUT_MANAGER
      var x = UnityEngine.Input.GetAxis("Horizontal");
      var y = UnityEngine.Input.GetAxis("Vertical");

      // no worries with clamping, normalization (all implicit)
      sInput.MoveDirection = new FPVector2(x.ToFP(), y.ToFP());

      sInput.Jump = UnityEngine.Input.GetButton("Jump");
      sInput.Dash = UnityEngine.Input.GetButton("Fire2");
      sInput.Fire = UnityEngine.Input.GetButton("Fire1");
      sInput.Use = UnityEngine.Input.GetButton("Fire3");

      // grab this using local mouse, etc
      sInput.Yaw = default;
      sInput.Pitch = default;
#endif

      // implicitly casts to base input
      callback.SetInput(sInput, DeterministicInputFlags.Repeatable);
    }
  }
}