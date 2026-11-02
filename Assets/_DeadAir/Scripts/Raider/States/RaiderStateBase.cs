using UnityEngine;

namespace DeadAir.Raider
{
    // Базовый класс состояния (паттерн "Состояние" / State).
    // Каждое состояние - отдельный класс со своей логикой: Enter при входе, Tick каждый кадр, Exit при выходе.
    // Так RaiderBrain не превращается в огромный switch, а новое состояние добавляется новым классом.
    public abstract class RaiderStateBase
    {
        protected readonly RaiderBrain brain;
        protected float timer; // сколько секунд прошло с входа в состояние

        protected RaiderStateBase(RaiderBrain brain)
        {
            this.brain = brain;
        }

        protected RaiderConfig Config => brain.Config;

        public abstract RaiderStateId Id { get; }
        public abstract Color Color { get; } // цвет рейдера в этом состоянии

        public virtual void Enter()
        {
            timer = 0f;
        }

        public virtual void Tick(float deltaTime)
        {
            timer += deltaTime;
        }

        public virtual void Exit()
        {
        }

        // Услышал выстрел. По умолчанию - сразу идёт искать к месту выстрела.
        // Бой и сдача переопределяют это поведение.
        public virtual void OnShotHeard(Vector3 position)
        {
            brain.LastKnownPosition = position;
            brain.ChangeState(RaiderStateId.Search);
        }

        // Общая проверка для спокойных состояний (патруль, возврат):
        // шкала заполнилась полностью - бой, частично - подозрение.
        protected bool TryNoticePlayer()
        {
            float detection = brain.Vision.Detection;

            if (detection >= 1f)
            {
                brain.ChangeState(RaiderStateId.Combat);
                return true;
            }

            if (detection >= Config.suspiciousLevel)
            {
                brain.ChangeState(RaiderStateId.Suspicious);
                return true;
            }

            return false;
        }
    }
}
