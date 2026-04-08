namespace WreckTogether.Gameplay
{
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;

    public class GameplayInput : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => OnPollInput(callback));
        }

        private void OnPollInput(CallbackPollInput callback)
        {
            var input = new Quantum.Input();

            var h = UnityEngine.Input.GetAxisRaw("Horizontal");
            var v = UnityEngine.Input.GetAxisRaw("Vertical");

            input.Movement = new FPVector2(
                FP.FromFloat_UNSAFE(h),
                FP.FromFloat_UNSAFE(v)
            );

            callback.SetInput(input, DeterministicInputFlags.Repeatable);
        }
    }
}
