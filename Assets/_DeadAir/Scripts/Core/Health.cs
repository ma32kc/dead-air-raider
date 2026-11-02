using System;
using UnityEngine;

namespace DeadAir.Core
{
    // Здоровье. Висит и на игроке, и на рейдере.
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;

        public float Current => currentHealth;
        public float Percent => currentHealth / maxHealth; // от 0 до 1
        public bool IsDead => currentHealth <= 0f;

        // События (паттерн Наблюдатель)
        public event Action<float> Damaged; // получил урон (сколько)
        public event Action Died;           // здоровье закончилось - вызывается один раз

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            Damaged?.Invoke(damage);

            if (IsDead)
                Died?.Invoke();
        }
    }
}
