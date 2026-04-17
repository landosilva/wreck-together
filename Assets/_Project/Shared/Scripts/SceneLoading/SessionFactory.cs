namespace WreckTogether.Shared
{
    using System;
    using Quantum;
    using WreckTogether.Tuning;

    /// <summary>
    /// Builds the Quantum objects a match needs, in one place, so the lobby flow
    /// and the offline bootstrap can't drift apart.
    /// </summary>
    public static class SessionFactory
    {
        public static RuntimeConfig BuildRuntimeConfig(SessionAssets assets)
        {
            var config = new RuntimeConfig
            {
                Seed = Guid.NewGuid().GetHashCode(),
                Map = assets.Map,
                WreckGameConfig = assets.WreckGameConfig,
                WreckPlayerPrototype = assets.PlayerPrototype,
            };

            var defaults = QuantumDefaultConfigs.Global;
            if (defaults != null) config.SimulationConfig = defaults.SimulationConfig;

            return config;
        }
    }
}
