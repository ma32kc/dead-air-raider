using UnityEngine;

namespace DeadAir.Raider
{
    // Подозрение: что-то заметил, останавливается и смотрит туда.
    // Шкала дошла до 1 - бой. Игрок пропал - идёт проверить место.
    public class SuspiciousState : RaiderStateBase
    {
        public SuspiciousState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.Suspicious;
        public override Color Color => Config.suspiciousColor;

        public override void Enter()
        {
            base.Enter();
            brain.StopMoving();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            RaiderVision vision = brain.Vision;

            if (vision.Detection >= 1f)
            {
                brain.ChangeState(RaiderStateId.Combat);
                return;
            }

            bool lostPlayer = !vision.CanSeeTarget && vision.Detection <= 0.05f;
            if (lostPlayer && timer >= Config.suspiciousTime)
            {
                brain.ChangeState(RaiderStateId.Search);
                return;
            }

            brain.Combat.TurnTo(brain.LastKnownPosition);
        }
    }
}
