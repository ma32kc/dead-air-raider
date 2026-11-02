using UnityEngine;

namespace DeadAir.Raider
{
    // Поиск: идёт к последней известной позиции (или месту выстрела),
    // там крутится и осматривается. Никого не нашёл - возвращается к патрулю.
    public class SearchState : RaiderStateBase
    {
        private bool lookingAround;

        public SearchState(RaiderBrain brain) : base(brain) { }

        public override RaiderStateId Id => RaiderStateId.Search;
        public override Color Color => Config.searchColor;

        public override void Enter()
        {
            base.Enter();
            lookingAround = false;
            brain.Agent.speed = Config.searchSpeed;
            brain.MoveTo(brain.LastKnownPosition);
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

            if (vision.CanSeeTarget && vision.Detection >= Config.suspiciousLevel)
            {
                brain.ChangeState(RaiderStateId.Suspicious);
                return;
            }

            if (!lookingAround)
            {
                brain.FaceMoveDirection();
                if (brain.HasArrived())
                {
                    lookingAround = true;
                    timer = 0f;
                }
                return;
            }

            brain.transform.Rotate(0f, Config.searchTurnSpeed * deltaTime, 0f);

            if (timer >= Config.searchTime)
                brain.ChangeState(RaiderStateId.ReturnToPatrol);
        }
    }
}
