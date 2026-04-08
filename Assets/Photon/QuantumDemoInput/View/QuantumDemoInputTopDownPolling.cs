namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
  using UnityEngine.InputSystem;
#endif

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDemoInputTopDownPolling : MonoBehaviour {
#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
    private InputAction _move;
    private InputAction _aim;
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
      _aim = playerInput.actions["Aim"];
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
      QuantumDemoInputTopDown tInput = default;

#if ENABLE_INPUT_SYSTEM && QUANTUM_ENABLE_INPUTSYSTEM
      // no worries with clamping, normalization (all implicit)
      // Left, Right, Up, Down are set during casting based on MoveDirection
      tInput.MoveDirection = _move.ReadValue<Vector2>().ToFPVector2();

      // normally uses second thumb stick or mouse based input
      tInput.AimDirection = _aim.ReadValue<Vector2>().ToFPVector2();

      tInput.Jump = UnityEngine.Input.GetButton("Jump");
      tInput.Dash = UnityEngine.Input.GetButton("Fire2");
      tInput.Fire = UnityEngine.Input.GetButton("Fire1");
      tInput.Use = UnityEngine.Input.GetButton("Fire3");
#elif ENABLE_LEGACY_INPUT_MANAGER
      var x = UnityEngine.Input.GetAxis("Horizontal");
      var y = UnityEngine.Input.GetAxis("Vertical");

      tInput.Left = x < 0;
      tInput.Right = x > 0;
      tInput.Down = y < 0;
      tInput.Up = y > 0;

      // no worries with clamping, normalization (all implicit)
      tInput.MoveDirection = new FPVector2(x.ToFP(), y.ToFP());

      // normally uses second thumb stick or mouse based input
      tInput.AimDirection = new FPVector2(x.ToFP(), y.ToFP());

      tInput.Jump = UnityEngine.Input.GetButton("Jump");
      tInput.Dash = UnityEngine.Input.GetButton("Fire2");
      tInput.Fire = UnityEngine.Input.GetButton("Fire1");
      tInput.Use = UnityEngine.Input.GetButton("Fire3");
#endif

      // implicitly casts to base input
      callback.SetInput(tInput, DeterministicInputFlags.Repeatable);
    }
  }
}