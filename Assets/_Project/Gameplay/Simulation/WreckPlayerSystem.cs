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
            public PhysicsBody3D* Body;
            public WreckPlayerLink* PlayerLink;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (f.Global->MatchOver)
            {
                return;
            }

            var config = f.FindAsset(f.RuntimeConfig.WreckGameConfig);
            Input* input = f.GetPlayerInput(filter.PlayerLink->PlayerRef);

            var movement = input->Movement;
            if (movement.SqrMagnitude > FP._1)
            {
                movement = movement.Normalized;
            }

            var force = new FPVector3(movement.X, 0, movement.Y) * config.PlayerMoveSpeed;
            filter.Body->AddForce(force);
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

            // Spread players along X axis
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
