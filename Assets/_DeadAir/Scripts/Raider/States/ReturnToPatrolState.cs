using UnityEngine;

namespace DeadAir.Raider
{
    // Возврат: идёт к ближайшей точке маршрута, потом снова патрулирует.
    public class ReturnToPatrolState : RaiderStateBase
    {
        public ReturnToPatrolState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.ReturnToPatrol;
        public override Color Color => Config.calmColor;

        public override void Enter()
        {
            base.Enter();
            brain.Vision.ResetDetection();
            brain.Agent.speed = Config.patrolSpeed;
            brain.SelectNearestWaypoint();
            brain.MoveTo(brain.CurrentWaypoint);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (TryNoticePlayer())
                return;

            brain.FaceMoveDirection();

            // Небольшая задержка, чтобы агент успел построить путь
            if (timer > 0.3f && brain.HasArrived())
                brain.ChangeState(RaiderStateId.Patrol);
        }
    }
}
