using UnityEngine;

namespace DeadAir.Raider
{
    // Сдался: остановился, повернулся к игроку и больше ничего не делает.
    public class SurrenderedState : RaiderStateBase
    {
        public SurrenderedState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.Surrendered;
        public override Color Color => Config.surrenderedColor;

        public override void Enter()
        {
            base.Enter();
            brain.StopMoving();

            Vector3 toPlayer = brain.Vision.Target.position - brain.transform.position;
            toPlayer.y = 0f;
            if (toPlayer.magnitude > 0.1f)
                brain.transform.rotation = Quaternion.LookRotation(toPlayer);
        }

        // Сдавшийся на выстрелы не реагирует
        public override void OnShotHeard(Vector3 position)
        {
        }
    }
}
