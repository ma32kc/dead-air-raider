using DeadAir.Controls;
using DeadAir.Core;
using DeadAir.Game;
using DeadAir.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadAir.UI
{
    // HUD на стекле шлема: здоровье, кислород, пробой скафандра, гравитация, экран смерти.
    // Вёрстка - в префабе HUD, здесь только данные.
    public class PlayerHud : MonoBehaviour
    {
        [Header("Игрок")]
        [SerializeField] private Health health;
        [SerializeField] private SuitOxygen suit;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private GameSession session;

        [Header("Показатели")]
        [SerializeField] private GameObject crosshair;
        [SerializeField] private GameObject vitalsPanel;
        [SerializeField] private StatBar healthBar;
        [SerializeField] private StatBar oxygenBar;
        [SerializeField] private TMP_Text gravityText;
        [SerializeField] private TMP_Text tapeText;

        [Header("Пробой скафандра")]
        [SerializeField] private GameObject breachPanel;
        [SerializeField] private TMP_Text breachTitle;
        [SerializeField] private TMP_Text breachHint;
        [SerializeField] private GameObject tapeBar;
        [SerializeField] private Image tapeFill;
        [SerializeField] private GameObject patchedLabel;
        [SerializeField] private float blinkPeriod = 0.8f;

        [Header("Смерть")]
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private TMP_Text restartHint;

        [Header("Цвета")]
        [SerializeField] private Color normalColor = new Color32(0xE5, 0xE0, 0xD4, 0xFF);
        [SerializeField] private Color warningColor = new Color32(0xEB, 0xA1, 0x38, 0xFF);
        [SerializeField] private Color dangerColor = new Color32(0xD6, 0x3D, 0x2B, 0xFF);
        [SerializeField, Range(0f, 1f)] private float lowHealth = 0.3f;
        [SerializeField, Range(0f, 1f)] private float lowOxygen = 0.25f;

        private void OnEnable()
        {
            health.Died += ShowDeathScreen;
        }

        private void OnDisable()
        {
            health.Died -= ShowDeathScreen;
        }

        private void Start()
        {
            deathScreen.SetActive(false);
            gravityText.text = "ГРАВИТАЦИЯ  " + GravityName(motor.Gravity);
        }

        private void Update()
        {
            if (health.IsDead)
                return;

            UpdateVitals();
            UpdateBreach();
        }

        private void UpdateVitals()
        {
            // Мало здоровья - красный (угроза)
            healthBar.Show(health.Percent, health.Percent < lowHealth ? dangerColor : normalColor);

            // Утечка - янтарный, почти кончился - красный
            Color oxygenColor = suit.IsLeaking ? warningColor : normalColor;
            if (suit.Percent < lowOxygen)
                oxygenColor = dangerColor;
            oxygenBar.Show(suit.Percent, oxygenColor);

            tapeText.text = "ИЗОЛЕНТА  ×" + suit.TapeRolls;
        }

        // Над прицелом: пробой скафандра и процесс заклейки
        private void UpdateBreach()
        {
            breachPanel.SetActive(suit.IsLeaking);
            patchedLabel.SetActive(suit.IsPatched);
            if (!suit.IsLeaking)
                return;

            // Мигает, пока не заклеили
            bool blink = Mathf.Repeat(Time.time, blinkPeriod) < blinkPeriod * 0.6f;
            breachTitle.color = blink ? dangerColor : warningColor;

            tapeBar.SetActive(suit.IsTaping);
            breachHint.gameObject.SetActive(!suit.IsTaping);
            tapeFill.fillAmount = suit.TapeProgress;

            breachHint.text = suit.TapeRolls > 0
                ? "Зажми " + input.TapeKeyName + " — заклеить изолентой"
                : "Изолента закончилась";
        }

        private void ShowDeathScreen()
        {
            crosshair.SetActive(false);
            vitalsPanel.SetActive(false);
            breachPanel.SetActive(false);
            patchedLabel.SetActive(false);

            deathScreen.SetActive(true);
            restartHint.text = session.RestartKeyName + " — начать заново";
        }

        private static string GravityName(PlanetGravity gravity)
        {
            switch (gravity)
            {
                case PlanetGravity.Low: return "НИЗКАЯ";
                case PlanetGravity.High: return "ВЫСОКАЯ";
                default: return "БАЗОВАЯ";
            }
        }
    }
}
