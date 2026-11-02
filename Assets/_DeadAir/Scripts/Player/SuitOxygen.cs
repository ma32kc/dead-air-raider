using System;
using DeadAir.Controls;
using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Player
{
    // Кислород в скафандре.
    // Запас ограничен и тратится всегда. Попадание может пробить скафандр - начинается утечка.
    // Утечку можно временно закрыть изолентой (удерживать кнопку). Через время лента отходит и утечка возобновляется.
    // Кислород закончился - герой задыхается и теряет здоровье.
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class SuitOxygen : MonoBehaviour
    {
        [Header("Кислород")]
        [SerializeField] private float maxOxygen = 100f;
        [SerializeField] private float normalUsage = 0.35f;      // в секунду, без утечки хватит почти на 5 минут
        [SerializeField] private float leakUsage = 3f;           // дополнительный расход при пробое
        [SerializeField] private float suffocationDamage = 8f;   // урон в секунду, когда кислород кончился

        [Header("Пробой и изолента")]
        [SerializeField, Range(0f, 1f)] private float breachChance = 0.4f; // шанс пробоя при попадании
        [SerializeField] private float tapeTime = 1.2f;          // сколько секунд держать кнопку
        [SerializeField] private float tapeHoldTime = 40f;       // сколько держится лента
        [SerializeField] private int tapeRolls = 3;

        private Health health;
        private PlayerInputHandler input;
        private float oxygen;
        private float tapeProgress;   // сколько уже клеим
        private float patchTimer;     // сколько ещё продержится лента

        public float Percent => oxygen / maxOxygen;
        public bool IsLeaking { get; private set; }
        public bool IsPatched => patchTimer > 0f;
        public bool IsTaping => tapeProgress > 0f;
        public float TapeProgress => tapeProgress / tapeTime;
        public int TapeRolls => tapeRolls;

        public event Action Breached;

        private void Awake()
        {
            health = GetComponent<Health>();
            input = GetComponent<PlayerInputHandler>();
            oxygen = maxOxygen;
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
        }

        private void Update()
        {
            if (health.IsDead)
                return;

            UpdatePatch();
            UpdateTaping();
            UpdateOxygen();
        }

        private void OnDamaged(float damage)
        {
            // Удушье не пробивает скафандр - только пули
            if (oxygen <= 0f || IsLeaking)
                return;

            if (UnityEngine.Random.value < breachChance)
                StartLeak();
        }

        private void StartLeak()
        {
            IsLeaking = true;
            patchTimer = 0f;
            Breached?.Invoke();
        }

        private void UpdateOxygen()
        {
            float usage = normalUsage;
            if (IsLeaking)
                usage += leakUsage;

            oxygen = Mathf.Max(0f, oxygen - usage * Time.deltaTime);

            if (oxygen <= 0f)
                health.TakeDamage(suffocationDamage * Time.deltaTime);
        }

        // Держим кнопку - клеим. Отпустили раньше времени - начинаем заново.
        private void UpdateTaping()
        {
            bool canTape = IsLeaking && tapeRolls > 0;

            if (!input.TapeHeld || !canTape)
            {
                tapeProgress = 0f;
                return;
            }

            tapeProgress += Time.deltaTime;
            if (tapeProgress >= tapeTime)
            {
                tapeProgress = 0f;
                tapeRolls--;
                IsLeaking = false;
                patchTimer = tapeHoldTime;
            }
        }

        // Лента держится не вечно
        private void UpdatePatch()
        {
            if (patchTimer <= 0f)
                return;

            patchTimer -= Time.deltaTime;
            if (patchTimer <= 0f)
                StartLeak();
        }
    }
}
