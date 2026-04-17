namespace WreckTogether.Tuning
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Tuning/Input", fileName = "InputTuning")]
    public class InputTuning : ScriptableObject
    {
        [Tooltip("Maximum vertical look angle in degrees (shared by camera and head IK).")]
        public float MaxPitchDegrees = 89f;

        [Tooltip("Default mouse sensitivity: degrees of yaw per unit of raw mouse delta.")]
        public float DefaultMouseDegreesPerCount = 0.15f;

        [Tooltip("Default gamepad look sensitivity in degrees per second at full stick.")]
        public float DefaultGamepadLookSensitivity = 120f;

        [Tooltip("Reference FOV at which mouse sensitivity equals the base value.")]
        public float BaseFOV = 90f;
    }
}
