using DeadAir.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadAir.Raider
{
    // Отладка прямо в Game view: конус зрения, состояние над головой и маркер последней известной позиции.
    // Все объекты (линия конуса, маркер, табличка) лежат в префабе рейдера, здесь только обновляем их.
    [RequireComponent(typeof(RaiderBrain))]
    public class RaiderDebugDraw : MonoBehaviour
    {
        [SerializeField] private LineRenderer cone;
        [SerializeField] private int coneSegments = 18;
        [SerializeField] private Transform lastKnownMarker;

        [Header("Табличка над головой")]
        [SerializeField] private Transform label;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image detectionFill;

        [Header("Цвета")]
        [SerializeField] private Color normalColor = new Color32(0xE5, 0xE0, 0xD4, 0xFF);
        [SerializeField] private Color mutedColor = new Color32(0x8C, 0x85, 0x78, 0x99);
        [SerializeField] private Color alertColor = new Color32(0xEB, 0xA1, 0x38, 0xFF);
        [SerializeField] private Color dangerColor = new Color32(0xD6, 0x3D, 0x2B, 0xFF);

        private RaiderBrain brain;
        private Camera viewCamera;

        private void Awake()
        {
            brain = GetComponent<RaiderBrain>();
            cone.positionCount = coneSegments + 1;
            cone.loop = true;
        }

        private void Start()
        {
            viewCamera = Camera.main;
        }

        private void LateUpdate()
        {
            bool visible = DebugOverlay.Visible;
            bool alive = !brain.Health.IsDead;
            RaiderStateId state = brain.CurrentStateId;

            bool showCone = visible && alive && state != RaiderStateId.Surrendered;
            cone.enabled = showCone;
            if (showCone)
                DrawCone();

            bool alerted = state == RaiderStateId.Suspicious
                           || state == RaiderStateId.Search
                           || state == RaiderStateId.Combat;
            lastKnownMarker.gameObject.SetActive(visible && alive && alerted);
            lastKnownMarker.position = brain.LastKnownPosition + Vector3.up * 0.2f;

            label.gameObject.SetActive(visible);
            if (visible)
                UpdateLabel(alive, state);
        }

        private void UpdateLabel(bool alive, RaiderStateId state)
        {
            // Табличка всегда смотрит в камеру
            if (viewCamera != null)
                label.rotation = viewCamera.transform.rotation;

            stateText.text = alive ? state.ToString() : "Dead";
            stateText.color = StateColor(state);
            healthText.text = "HP " + Mathf.RoundToInt(brain.Health.Current);

            float detection = brain.Vision.Detection;
            detectionFill.fillAmount = detection;
            detectionFill.color = detection >= 1f ? dangerColor : alertColor;
        }

        private Color StateColor(RaiderStateId state)
        {
            switch (state)
            {
                case RaiderStateId.Suspicious: return alertColor;
                case RaiderStateId.Search: return alertColor;
                case RaiderStateId.Combat: return dangerColor;
                default: return normalColor;
            }
        }

        // Конус - замкнутая линия: глаз -> дуга -> обратно к глазу
        private void DrawCone()
        {
            RaiderVision vision = brain.Vision;
            Vector3 origin = vision.Eye.position;
            float halfAngle = vision.ViewAngle / 2f;

            cone.SetPosition(0, origin);
            for (int i = 0; i < coneSegments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(coneSegments - 1));
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * transform.forward;
                cone.SetPosition(i + 1, origin + direction * vision.ViewDistance);
            }

            Color color = vision.CanSeeTarget ? alertColor : mutedColor;
            cone.startColor = color;
            cone.endColor = color;
        }
    }
}
