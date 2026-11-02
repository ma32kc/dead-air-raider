using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Raider
{
    // Зрение рейдера: конус + проверка стен лучом + шкала обнаружения от 0 до 1.
    // Пока игрок в конусе, шкала растёт: вблизи быстро, вдали медленно.
    public class RaiderVision : MonoBehaviour
    {
        [SerializeField] private Transform eye;
        [SerializeField] private Transform target;

        [Header("Конус")]
        [SerializeField] private float viewDistance = 18f;
        [SerializeField] private float viewAngle = 70f;    // ширина конуса в градусах
        [SerializeField] private float maxLookUp = 75f;    // насколько высоко видит (игрок может залезть на ящик)
        [SerializeField] private float maxLookDown = 35f;

        [Header("Шкала обнаружения")]
        [SerializeField] private float fillNear = 1.4f;    // скорость роста вблизи (в секунду)
        [SerializeField] private float fillFar = 0.35f;    // скорость роста на границе зрения
        [SerializeField] private float nearDistance = 5f;
        [SerializeField] private float decay = 0.55f;      // скорость падения, когда игрока не видно

        private IDamageable targetHealth;

        public float Detection { get; private set; }   // 0 - не замечен, 1 - точно замечен
        public bool CanSeeTarget { get; private set; }
        public Vector3 LastSeenPosition { get; private set; }
        public Transform Target => target;
        public Transform Eye => eye;
        public float ViewDistance => viewDistance;
        public float ViewAngle => viewAngle;

        private void Awake()
        {
            if (target != null)
                targetHealth = target.GetComponent<IDamageable>();
        }

        public void ResetDetection()
        {
            Detection = 0f;
        }

        // Вызывает RaiderBrain каждый кадр
        public void Tick(float deltaTime)
        {
            CanSeeTarget = IsTargetVisible();

            if (CanSeeTarget)
            {
                LastSeenPosition = target.position;

                float distance = Vector3.Distance(eye.position, target.position);
                float t = Mathf.InverseLerp(viewDistance, nearDistance, distance); // 1 вблизи, 0 на границе
                Detection += Mathf.Lerp(fillFar, fillNear, t) * deltaTime;
            }
            else
            {
                Detection -= decay * deltaTime;
            }

            Detection = Mathf.Clamp01(Detection);
        }

        private bool IsTargetVisible()
        {
            if (target == null || targetHealth == null || targetHealth.IsDead)
                return false;

            // Достаточно увидеть грудь или голову
            Vector3 chest = target.position + Vector3.up * 1.4f;
            Vector3 head = target.position + Vector3.up * 1.8f;
            return CanSeePoint(chest) || CanSeePoint(head);
        }

        private bool CanSeePoint(Vector3 point)
        {
            Vector3 toPoint = point - eye.position;
            float distance = toPoint.magnitude;
            if (distance > viewDistance)
                return false;

            // Угол по горизонтали: попадает ли в конус
            Vector3 flat = toPoint;
            flat.y = 0f;
            if (Vector3.Angle(transform.forward, flat) > viewAngle / 2f)
                return false;

            // Угол по вертикали
            float verticalAngle = Vector3.Angle(flat, toPoint);
            float limit = toPoint.y > 0f ? maxLookUp : maxLookDown;
            if (verticalAngle > limit)
                return false;

            // Луч до игрока: если первым попали не в игрока, значит мешает стена.
            // Глаз находится внутри своей капсулы, а луч, начатый внутри коллайдера, его не замечает.
            if (Physics.Raycast(eye.position, toPoint.normalized, out RaycastHit hit, distance + 0.5f))
                return hit.transform == target;

            return false;
        }
    }
}
