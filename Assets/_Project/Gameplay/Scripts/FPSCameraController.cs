namespace WreckTogether.Gameplay
{
    using Quantum;
    using UnityEngine;

    public class FPSCameraController : MonoBehaviour
    {
        [SerializeField] private GameplayInput _input;
        [SerializeField] private float _eyeHeight = 0.8f;

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                FindLocalPlayer();
                if (_target == null) return;
            }

            var position = _target.position + Vector3.up * _eyeHeight;
            transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(_input.Pitch, _input.Yaw, 0f)
            );
        }

        private void FindLocalPlayer()
        {
            var game = QuantumRunner.Default?.Game;
            if (game == null) return;

            var localPlayers = game.GetLocalPlayers();

            var frame = game.Frames.Verified;
            if (frame == null) return;

            foreach (var localPlayer in localPlayers)
            {
                foreach (var pair in frame.GetComponentIterator<WreckPlayerLink>())
                {
                    if (pair.Component.PlayerRef == localPlayer)
                    {
                        var viewUpdater = FindAnyObjectByType<QuantumEntityViewUpdater>();
                        var entityView = viewUpdater?.GetView(pair.Entity);
                        if (entityView != null)
                        {
                            _target = entityView.transform;
                            return;
                        }
                    }
                }
            }
        }
    }
}
