namespace WreckTogether.Gameplay
{
    using Quantum;
    using UnityEngine;

    /// <summary>
    /// Drives the transform of the first-person camera rig (typically a
    /// CinemachineCamera) to sit at the local player's <c>EyeAnchor</c> child
    /// transform, rotated by the locally-predicted yaw/pitch from
    /// <see cref="GameplayInput"/>.
    ///
    /// The eye anchor lives on the Player prefab so its position can be tuned
    /// per-character in the inspector instead of being derived at runtime.
    /// </summary>
    public class FPSCameraController : MonoBehaviour
    {
        [Tooltip("Name of the child transform on the player prefab that marks the eye position.")]
        [SerializeField] private string _eyeAnchorName = "EyeAnchor";

        [SerializeField] private GameplayInput _input;

        private Transform _eyeAnchor;

        private void LateUpdate()
        {
            if (_eyeAnchor == null)
            {
                FindLocalEyeAnchor();
                if (_eyeAnchor == null) return;
            }

            transform.SetPositionAndRotation(
                _eyeAnchor.position,
                Quaternion.Euler(_input.Pitch, _input.Yaw, 0f)
            );
        }

        private void FindLocalEyeAnchor()
        {
            var game = QuantumRunner.Default?.Game;
            if (game == null) return;

            var frame = game.Frames.Verified;
            if (frame == null) return;

            foreach (var localPlayer in game.GetLocalPlayers())
            {
                foreach (var pair in frame.GetComponentIterator<WreckPlayerLink>())
                {
                    if (pair.Component.PlayerRef != localPlayer) continue;

                    var viewUpdater = FindAnyObjectByType<QuantumEntityViewUpdater>();
                    var entityView = viewUpdater?.GetView(pair.Entity);
                    if (entityView == null) continue;

                    var anchor = FindChildRecursive(entityView.transform, _eyeAnchorName);
                    if (anchor == null)
                    {
                        // Not an error on the first few frames: CharacterModelView
                        // re-parents the anchor onto the head bone during its own
                        // OnActivate, which may not have run yet.
                        return;
                    }

                    _eyeAnchor = anchor;
                    return;
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
