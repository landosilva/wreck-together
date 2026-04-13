namespace WreckTogether.Gameplay
{
    using Quantum;
    using UnityEngine;
    using WreckTogether.Shared;

    public class CharacterModelView : QuantumEntityViewComponent
    {
        [SerializeField] private CharacterDatabase _characterDatabase;
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private float _maxSpeed = 5f;

        [Tooltip("Name of the child transform that marks the eye position. Re-parented to the head bone at spawn so it follows head animation and IK.")]
        [SerializeField] private string _eyeAnchorName = "EyeAnchor";

        private GameObject _modelInstance;
        private Animator _animator;
        private HeadIKHandler _headIK;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

        public override void OnActivate(Frame frame)
        {
            if (!frame.TryGet<WreckPlayerLink>(EntityRef, out var playerLink))
                return;

            var characterIndex = playerLink.CharacterIndex;
            var character = _characterDatabase.Get(characterIndex);

            if (character.ModelPrefab == null)
            {
                Debug.LogWarning($"[CharacterModelView] Character '{character.DisplayName}' has no model prefab.");
                return;
            }

            _modelInstance = Instantiate(character.ModelPrefab, transform);
            _modelInstance.transform.localPosition = Vector3.zero;
            _modelInstance.transform.localRotation = Quaternion.identity;

            // Set up Animator
            if (_animatorController != null)
            {
                _animator = _modelInstance.GetComponent<Animator>();
                if (_animator == null)
                {
                    _animator = _modelInstance.AddComponent<Animator>();
                }

                _animator.runtimeAnimatorController = _animatorController;
            }

            // Set up Head IK for all players (pitch is networked)
            _headIK = _modelInstance.AddComponent<HeadIKHandler>();

            // Re-parent the EyeAnchor to the head bone so it follows animation
            // and head-IK rotation. Local offset is taken from the prefab so
            // designers can tune it without editing code.
            ReparentEyeAnchorToHead();
        }

        private void ReparentEyeAnchorToHead()
        {
            var anchor = transform.Find(_eyeAnchorName);
            if (anchor == null) return;

            if (_animator == null || _animator.avatar == null || !_animator.avatar.isHuman) return;

            var head = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;

            anchor.SetParent(head, worldPositionStays: true);
        }

        public override void OnUpdateView()
        {
            var frame = PredictedFrame;
            if (frame == null) return;

            // Drive animator parameters from KCC
            if (_animator != null && frame.TryGet<KCC>(EntityRef, out var kcc))
            {
                var realSpeed = kcc.Data.RealSpeed.AsFloat;
                var normalizedSpeed = Mathf.Clamp01(realSpeed / _maxSpeed);

                _animator.SetFloat(SpeedHash, normalizedSpeed, 0.1f, Time.deltaTime);
                _animator.SetBool(IsGroundedHash, kcc.Data.IsGrounded);
                _animator.SetFloat(VerticalSpeedHash, kcc.Data.RealVelocity.Y.AsFloat);
            }

            // Drive head IK from networked pitch
            if (_headIK != null && frame.TryGet<WreckPlayerLink>(EntityRef, out var link))
            {
                var pitchDegrees = link.Pitch.AsFloat * Mathf.Rad2Deg;
                _headIK.LookPitch = Mathf.Clamp(pitchDegrees, -60f, 60f);
            }
        }

        public override void OnDeactivate()
        {
            _animator = null;
            _headIK = null;

            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
                _modelInstance = null;
            }
        }
    }
}
