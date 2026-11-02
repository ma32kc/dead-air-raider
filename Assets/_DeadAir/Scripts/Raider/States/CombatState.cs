using UnityEngine;

namespace DeadAir.Raider
{
    // Бой: подходит на дистанцию и стреляет. Никогда не убегает.
    // Если игрок спрятался - идёт к месту, где видел его в последний раз (сквозь стены не подсматривает).
    public class CombatState : RaiderStateBase
    {
        public CombatState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.Combat;
        public override Color Color => Config.combatColor;

        public override void Enter()
        {
            base.Enter();
            brain.Agent.speed = Config.combatSpeed;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            RaiderVision vision = brain.Vision;

            if (!vision.CanSeeTarget && vision.Detection <= Config.loseTargetLevel)
            {
                brain.ChangeState(RaiderStateId.Search);
                return;
            }

            if (vision.CanSeeTarget)
                FightVisibleTarget(vision.Target);
            else
                ChaseLastKnownPosition();
        }

        // В бою выстрел ничего не переключает, только подсказывает, где игрок
        public override void OnShotHeard(Vector3 position)
        {
            if (!brain.Vision.CanSeeTarget)
                brain.LastKnownPosition = position;
        }

        private void FightVisibleTarget(Transform target)
        {
            brain.Combat.TurnTo(target.position);

            float distance = Vector3.Distance(brain.transform.position, target.position);
            if (distance > Config.combatDistance)
                brain.MoveTo(target.position);
            else
                brain.StopMoving();

            brain.Combat.TryShoot(target);
        }

        private void ChaseLastKnownPosition()
        {
            brain.Combat.TurnTo(brain.LastKnownPosition);
            brain.MoveTo(brain.LastKnownPosition);
        }
    }
}
