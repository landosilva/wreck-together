namespace WreckTogether.Gameplay
{
    using Quantum;
    using Unity.Cinemachine;
    using UnityEngine;

    public class FPSCameraController : MonoBehaviour
    {
        [Tooltip("Input component that provides yaw/pitch and receives the current FOV.")]
        [SerializeField] private GameplayInput _input;

        [Tooltip("Virtual camera whose lens FOV is published to the input for sensitivity scaling.")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        [Tooltip("Name of the child transform on the player prefab that marks eye position.")]
        [SerializeField] private string _eyeAnchorName = "EyeAnchor";

        private Transform _eyeAnchor;

        private void Awake()
        {
            if (_cinemachineCamera == null)
                _cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void LateUpdate()
        {
            PublishCurrentFOV();

            if (_eyeAnchor == null)
            {
                _eyeAnchor = ResolveLocalEyeAnchor();
                if (_eyeAnchor == null) return;
            }

            transform.SetPositionAndRotation(
                _eyeAnchor.position,
                Quaternion.Euler(_input.Pitch, _input.Yaw, 0f));
        }

        private void PublishCurrentFOV()
        {
            if (_cinemachineCamera != null)
                _input.CurrentLookFOV = _cinemachineCamera.Lens.FieldOfView;
        }

        private Transform ResolveLocalEyeAnchor()
        {
            var game = QuantumRunner.Default?.Game;
            var frame = game?.Frames.Verified;
            if (frame == null) return null;

            var viewUpdater = FindAnyObjectByType<QuantumEntityViewUpdater>();
            if (viewUpdater == null) return null;

            foreach (var localPlayer in game.GetLocalPlayers())
            {
                foreach (var pair in frame.GetComponentIterator<WreckPlayerLink>())
                {
                    if (pair.Component.PlayerRef != localPlayer) continue;

                    var view = viewUpdater.GetView(pair.Entity);
                    if (view == null) continue;

                    // Null for the first few frames while CharacterModelView
                    // re-parents the anchor onto the head bone
                    return FindChildByName(view.transform, _eyeAnchorName);
                }
            }
            return null;
        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
                var found = FindChildByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
