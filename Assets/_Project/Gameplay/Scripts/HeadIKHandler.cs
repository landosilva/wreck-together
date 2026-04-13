namespace WreckTogether.Gameplay
{
    using UnityEngine;

    public class HeadIKHandler : MonoBehaviour
    {
        public float LookPitch { get; set; }

        private Animator _animator;
        private Transform _headBone;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (_animator != null && _animator.avatar != null && _animator.avatar.isHuman)
            {
                _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0) return;
            if (_animator == null || _headBone == null) return;

            var bodyForward = transform.forward;
            var bodyRight = transform.right;

            var lookDirection = Quaternion.AngleAxis(LookPitch, bodyRight) * bodyForward;
            var targetPosition = _headBone.position + lookDirection * 2f;

            _animator.SetLookAtWeight(1f, 0f, 1f, 0f, 0.5f);
            _animator.SetLookAtPosition(targetPosition);
        }
    }
}
