using UnityEngine;

namespace DeadAir.Core
{
    // Оружие: луч из точки в направлении, урон по IDamageable, трассер, вспышка и звук.
    // Одно и то же оружие у игрока и у рейдера - отличаются только настройки в префабе.
    // Кто и когда стреляет, решает владелец (PlayerShooter, RaiderCombat).
    public class Gun : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 60f;
        [SerializeField] private float bulletRadius;  // больше 0 - пуля "толстая" и прощает неточный прицел

        [Header("Эффекты")]
        [SerializeField] private Tracer tracerPrefab;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private Color tracerColor = Color.white;

        [Header("Звук")]
        [SerializeField] private AudioClip shotSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField, Range(0f, 1f)] private float shotSpatialBlend = 0.2f;

        public Transform Muzzle => muzzle;
        public float Range => range;

        // origin - откуда летит пуля. Это не всегда ствол: игрок стреляет "из груди",
        // чтобы ствол, высунутый за край укрытия, не простреливал стену.
        public void Fire(Vector3 origin, Vector3 direction)
        {
            direction.Normalize();
            Vector3 hitPoint = origin + direction * range;

            if (CastBullet(origin, direction, out RaycastHit hit))
            {
                hitPoint = hit.point;

                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null && !target.IsDead)
                {
                    target.TakeDamage(damage);
                    SoundPlayer.PlayAt(hitSound, hit.point, 0.55f, 0.8f);
                }
            }

            PlayEffects(hitPoint);
            GunshotBus.Raise(muzzle.position, gameObject);
        }

        private bool CastBullet(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            if (bulletRadius > 0f)
                return Physics.SphereCast(origin, bulletRadius, direction, out hit, range);

            return Physics.Raycast(origin, direction, out hit, range);
        }

        private void PlayEffects(Vector3 hitPoint)
        {
            if (tracerPrefab != null)
                Instantiate(tracerPrefab).Show(muzzle.position, hitPoint, tracerColor);

            if (muzzleFlashPrefab != null)
                Instantiate(muzzleFlashPrefab, muzzle.position, Quaternion.identity);

            SoundPlayer.PlayAt(shotSound, muzzle.position, 1f, shotSpatialBlend);
        }
    }
}
