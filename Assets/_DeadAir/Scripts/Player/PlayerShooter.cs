using DeadAir.Controls;
using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Player
{
    // Стрельба игрока: куда смотрит прицел в центре экрана, туда и летит пуля.
    [RequireComponent(typeof(Gun))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Health))]
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float fireCooldown = 0.35f;
        [SerializeField] private float chestHeight = 1.4f;

        private Gun gun;
        private PlayerInputHandler input;
        private Health health;
        private SuitOxygen suit;
        private float nextShotTime;

        private void Awake()
        {
            gun = GetComponent<Gun>();
            input = GetComponent<PlayerInputHandler>();
            health = GetComponent<Health>();
            suit = GetComponent<SuitOxygen>();
        }

        private void OnEnable()
        {
            input.FirePressed += TryShoot;
        }

        private void OnDisable()
        {
            input.FirePressed -= TryShoot;
        }

        private void TryShoot()
        {
            if (health.IsDead || Time.time < nextShotTime)
                return;

            // Руки заняты изолентой - стрелять нельзя
            if (suit != null && suit.IsTaping)
                return;

            nextShotTime = Time.time + fireCooldown;

            // Пуля летит из груди в точку под прицелом. Укрытие по пути её остановит.
            Vector3 chest = transform.position + Vector3.up * chestHeight;
            gun.Fire(chest, FindAimPoint(chest) - chest);
        }

        // Точка, на которую смотрит прицел в центре экрана.
        // Луч начинаем на уровне игрока, чтобы не попасть в то, что у него за спиной.
        private Vector3 FindAimPoint(Vector3 chest)
        {
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float skip = Mathf.Max(0f, Vector3.Dot(chest - ray.origin, ray.direction));
            Vector3 start = ray.origin + ray.direction * skip;

            if (Physics.Raycast(start, ray.direction, out RaycastHit hit, gun.Range))
                return hit.point;

            return start + ray.direction * gun.Range;
        }
    }
}
