using System.Collections.Generic;
using DeadAir.Core;
using UnityEngine;
using UnityEngine.AI;

namespace DeadAir.Raider
{
    // "Мозг" рейдера - конечный автомат на паттерне "Состояние".
    // Сам мозг почти ничего не решает: он хранит ссылки на компоненты, переключает состояния
    // и даёт им общие методы (идти к точке, повернуться по ходу движения и т.д.).
    // Вся логика поведения - в классах из папки States.
    //
    // Patrol / ReturnToPatrol -> шкала >= suspiciousLevel -> Suspicious -> шкала = 1 -> Combat
    // Suspicious или Combat: игрок пропал -> Search -> никого -> ReturnToPatrol -> Patrol
    // Выстрел в радиусе слуха -> сразу Search (в бою только обновляет позицию игрока)
    // Мало HP и рейдер остался один -> Surrendered
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(RaiderVision))]
    [RequireComponent(typeof(RaiderHearing))]
    [RequireComponent(typeof(RaiderCombat))]
    [RequireComponent(typeof(Health))]
    public class RaiderBrain : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        // Все активные рейдеры на сцене - чтобы понять, остался ли рейдер один
        private static readonly List<RaiderBrain> activeRaiders = new List<RaiderBrain>();

        [SerializeField] private RaiderConfig config;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Renderer body;   // перекрашивается по состоянию

        private Dictionary<RaiderStateId, RaiderStateBase> states;
        private RaiderStateBase currentState;
        private RaiderHearing hearing;
        private MaterialPropertyBlock bodyTint;
        private int waypointIndex;

        public RaiderConfig Config => config;
        public NavMeshAgent Agent { get; private set; }
        public RaiderVision Vision { get; private set; }
        public RaiderCombat Combat { get; private set; }
        public Health Health { get; private set; }

        public RaiderStateId CurrentStateId => currentState != null ? currentState.Id : RaiderStateId.Patrol;
        public Vector3 CurrentWaypoint => waypoints[waypointIndex].position;

        // Последняя известная позиция игрока (или место, откуда стреляли)
        public Vector3 LastKnownPosition { get; set; }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Vision = GetComponent<RaiderVision>();
            Combat = GetComponent<RaiderCombat>();
            Health = GetComponent<Health>();
            hearing = GetComponent<RaiderHearing>();
            bodyTint = new MaterialPropertyBlock();

            Agent.updateRotation = false; // поворачиваем рейдера сами

            states = new Dictionary<RaiderStateId, RaiderStateBase>
            {
                { RaiderStateId.Patrol, new PatrolState(this) },
                { RaiderStateId.Suspicious, new SuspiciousState(this) },
                { RaiderStateId.Search, new SearchState(this) },
                { RaiderStateId.Combat, new CombatState(this) },
                { RaiderStateId.ReturnToPatrol, new ReturnToPatrolState(this) },
                { RaiderStateId.Surrendered, new SurrenderedState(this) }
            };
        }

        private void OnEnable()
        {
            activeRaiders.Add(this);
            hearing.ShotHeard += OnShotHeard;
            Health.Died += OnDied;
        }

        private void OnDisable()
        {
            activeRaiders.Remove(this);
            hearing.ShotHeard -= OnShotHeard;
            Health.Died -= OnDied;
        }

        private void Start()
        {
            ChangeState(RaiderStateId.Patrol);
        }

        private void Update()
        {
            Vision.Tick(Time.deltaTime);
            if (Vision.CanSeeTarget)
                LastKnownPosition = Vision.LastSeenPosition;

            if (ShouldSurrender())
                ChangeState(RaiderStateId.Surrendered);

            currentState.Tick(Time.deltaTime);
        }

        public void ChangeState(RaiderStateId id)
        {
            if (currentState != null)
                currentState.Exit();

            currentState = states[id];
            currentState.Enter();
            SetBodyColor(currentState.Color);
        }

        private void OnShotHeard(Vector3 position)
        {
            // Каждое состояние само решает, как реагировать на выстрел
            if (currentState != null)
                currentState.OnShotHeard(position);
        }

        private bool ShouldSurrender()
        {
            if (!config.canSurrender || CurrentStateId == RaiderStateId.Surrendered)
                return false;
            if (Health.Percent > config.surrenderHealth)
                return false;

            // Сдаётся, только если других боеспособных рейдеров нет
            int fighters = 0;
            foreach (RaiderBrain raider in activeRaiders)
            {
                if (raider.CurrentStateId != RaiderStateId.Surrendered)
                    fighters++;
            }
            return fighters <= 1;
        }

        private void OnDied()
        {
            Agent.enabled = false;
            SetBodyColor(config.deadColor);
            enabled = false; // мёртвый рейдер больше ничего не делает
        }

        // MaterialPropertyBlock меняет цвет только у этого рендерера, не создавая копию материала
        private void SetBodyColor(Color color)
        {
            body.GetPropertyBlock(bodyTint);
            bodyTint.SetColor(BaseColorId, color);
            body.SetPropertyBlock(bodyTint);
        }

        // ---------- Общие методы для состояний ----------

        public void MoveTo(Vector3 point)
        {
            // Точку (например, игрок стоит на ящике) переносим на ближайшее место NavMesh
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                Agent.SetDestination(hit.position);
        }

        public void StopMoving()
        {
            Agent.ResetPath();
        }

        public bool HasArrived()
        {
            return !Agent.pathPending && Agent.remainingDistance <= config.arriveDistance;
        }

        // Поворачивается туда, куда идёт
        public void FaceMoveDirection()
        {
            Vector3 velocity = Agent.velocity;
            velocity.y = 0f;
            if (velocity.magnitude < 0.2f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        public void SelectNextWaypoint()
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        public void SelectNearestWaypoint()
        {
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, waypoints[i].position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    waypointIndex = i;
                }
            }
        }

        // Маршрут патруля в окне Scene
        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Gizmos.color = new Color(0.9f, 0.88f, 0.83f);
            for (int i = 0; i < waypoints.Length; i++)
            {
                Transform from = waypoints[i];
                Transform to = waypoints[(i + 1) % waypoints.Length];
                if (from == null || to == null)
                    continue;

                Gizmos.DrawWireSphere(from.position, 0.3f);
                Gizmos.DrawLine(from.position, to.position);
            }
        }
    }
}
