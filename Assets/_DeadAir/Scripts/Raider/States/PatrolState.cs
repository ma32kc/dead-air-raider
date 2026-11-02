using UnityEngine;

namespace DeadAir.Raider
{
    // Патруль: ходит по точкам по кругу и ненадолго останавливается на каждой.
    public class PatrolState : RaiderStateBase
    {
        private bool waiting;

        public PatrolState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.Patrol;
        public override Color Color => Config.calmColor;

        public override void Enter()
        {
            base.Enter();
            waiting = false;
            brain.Agent.speed = Config.patrolSpeed;
            brain.MoveTo(brain.CurrentWaypoint);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (TryNoticePlayer())
                return;

            brain.FaceMoveDirection();

            if (!waiting)
            {
                if (brain.HasArrived())
                {
                    waiting = true;
                    timer = 0f;
                }
            }
            else if (timer >= Config.waitOnPoint)
            {
                // Постояли - идём к следующей точке
                waiting = false;
                brain.SelectNextWaypoint();
                brain.MoveTo(brain.CurrentWaypoint);
            }
        }
    }
}
