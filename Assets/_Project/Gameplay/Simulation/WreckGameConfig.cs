namespace Quantum.WreckTogether
{
    using Photon.Deterministic;

    public class WreckGameConfig : AssetObject
    {
        public FP MatchDuration = 300;
        public FP PlayerMoveSpeed = 5;
        public AssetRef<EntityPrototype> PlayerPrototype;
    }
}
