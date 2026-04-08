namespace Quantum.WreckTogether
{
    public unsafe class WreckTimerSystem : SystemMainThread
    {
        public override void OnInit(Frame f)
        {
            var config = f.FindAsset(f.RuntimeConfig.WreckGameConfig);
            f.Global->MatchTimer = config.MatchDuration;
            f.Global->MatchOver = false;
        }

        public override void Update(Frame f)
        {
            if (f.Global->MatchOver)
            {
                return;
            }

            f.Global->MatchTimer -= f.DeltaTime;

            if (f.Global->MatchTimer <= 0)
            {
                f.Global->MatchTimer = 0;
                f.Global->MatchOver = true;
            }
        }
    }
}
