namespace Quantum.WreckTogether
{
    using Photon.Deterministic;

    public unsafe class WreckPlayerSystem : SystemMainThreadFilter<WreckPlayerSystem.Filter>,
        ISignalOnPlayerAdded, ISignalOnPlayerDisconnected
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public KCC* KCC;
            public WreckPlayerLink* PlayerLink;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (f.Global->MatchOver)
            {
                return;
            }

            Input* input = f.GetPlayerInput(filter.PlayerLink->PlayerRef);

            var movement = input->Movement;
            if (movement.SqrMagnitude > FP._1)
            {
                movement = movement.Normalized;
            }

            // Rotate movement by yaw so WASD is relative to look direction
            var yawRotation = FPQuaternion.Euler(0, input->Yaw, 0);
            var moveDirection = yawRotation * new FPVector3(movement.X, 0, movement.Y);

            // Feed direction to KCC — EnvironmentProcessor handles acceleration/friction
            filter.KCC->SetInputDirection(moveDirection);

            // Set look rotation so entity faces the yaw direction
            filter.KCC->SetLookRotation(0, input->Yaw);
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var config = f.FindAsset(f.RuntimeConfig.WreckGameConfig);
            var prototypeRef = f.RuntimeConfig.WreckPlayerPrototype.IsValid
                ? f.RuntimeConfig.WreckPlayerPrototype
                : config.PlayerPrototype;

            var prototype = f.FindAsset(prototypeRef);
            var entity = f.Create(prototype);

            f.Set(entity, new WreckPlayerLink { PlayerRef = player });

            if (f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
            {
                var offset = player._index * 2;
                transform->Position = new FPVector3(offset - 3, 1, 0);
                transform->Teleport(f, transform);
            }
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            foreach (var pair in f.GetComponentIterator<WreckPlayerLink>())
            {
                if (pair.Component.PlayerRef == player)
                {
                    f.Destroy(pair.Entity);
                }
            }
        }
    }
}
