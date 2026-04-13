namespace WreckTogether.Gameplay
{
    using Quantum;
    using UnityEngine;

    public class FPSCameraController : MonoBehaviour
    {
        [SerializeField] private GameplayInput _input;
        [SerializeField] private float _eyeOffsetAboveHead = 0.2f;

        private Transform _target;
        private Transform _headBone;

        private void LateUpdate()
        {
            if (_target == null)
            {
                FindLocalPlayer();
                if (_target == null) return;
            }

            Vector3 eyePosition;
            if (_headBone != null)
            {
                eyePosition = _headBone.position + Vector3.up * _eyeOffsetAboveHead;
            }
            else
            {
                eyePosition = _target.position + Vector3.up * 1.4f;
            }

            transform.SetPositionAndRotation(
                eyePosition,
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
                            FindHeadBone();
                            return;
                        }
                    }
                }
            }
        }

        private void FindHeadBone()
        {
            var animator = _target.GetComponentInChildren<Animator>();
            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                _headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            }
        }
    }
}
