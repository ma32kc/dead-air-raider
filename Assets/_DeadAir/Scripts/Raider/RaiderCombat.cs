using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Raider
{
    // Как рейдер стреляет: разворот к цели, темп стрельбы и разброс. Сам выстрел делает Gun.
    [RequireComponent(typeof(Gun))]
    public class RaiderCombat : MonoBehaviour
    {
        [SerializeField] private float fireInterval = 0.7f;
        [SerializeField] private float turnSpeed = 8f;
        [SerializeField] private float maxAimAngle = 20f;     // стреляет, только когда почти развернулся к цели
        [SerializeField, Range(0f, 1f)] private float hitChanceNear = 0.85f;
        [SerializeField, Range(0f, 1f)] private float hitChanceFar = 0.35f;

        private Gun gun;
        private float nextShotTime;

        private void Awake()
        {
            gun = GetComponent<Gun>();
        }

        // Плавно поворачивает рейдера к точке (только по горизонтали)
        public void TurnTo(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.magnitude < 0.05f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        public void TryShoot(Transform target)
        {
            if (Time.time < nextShotTime || !IsFacing(target.position))
                return;

            nextShotTime = Time.time + fireInterval;

            Vector3 from = gun.Muzzle.position;
            gun.Fire(from, GetAimPoint(target, from) - from);
        }

        private bool IsFacing(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            return Vector3.Angle(transform.forward, direction) <= maxAimAngle;
        }

        // Чем дальше цель, тем чаще промахи. При промахе целимся чуть мимо.
        private Vector3 GetAimPoint(Transform target, Vector3 from)
        {
            Vector3 aimPoint = target.position + Vector3.up * 1.4f;
            float hitChance = Mathf.Lerp(hitChanceNear, hitChanceFar, Vector3.Distance(from, aimPoint) / gun.Range);

            if (Random.value <= hitChance)
                return aimPoint;

            Vector3 side = Vector3.Cross(Vector3.up, aimPoint - from).normalized;
            if (Random.value < 0.5f)
                side = -side;
            return aimPoint + side * 0.9f + Vector3.up * Random.Range(-0.3f, 0.5f);
        }
    }
}
