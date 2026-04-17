namespace WreckTogether.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Math for consistent mouse look across monitors (DPI) and camera views (FOV).
    /// </summary>
    public static class LookSensitivity
    {
        private const float ReferenceDpi = 96f;

        public static float DpiScale()
        {
            var dpi = Screen.dpi;
            return dpi <= 0f ? 1f : ReferenceDpi / dpi;
        }

        /// <summary>
        /// Quake/Source "0% match" FOV scaling — flick distance stays
        /// constant when the camera zooms.
        /// </summary>
        public static float FovScale(float currentFov, float baseFov)
        {
            baseFov    = Mathf.Max(1f, baseFov);
            currentFov = Mathf.Clamp(currentFov, 1f, 179f);
            return Mathf.Tan(currentFov * 0.5f * Mathf.Deg2Rad)
                 / Mathf.Tan(baseFov    * 0.5f * Mathf.Deg2Rad);
        }
    }
}
