using UnityEngine;

namespace DeadAir.Raider
{
    // Настройки поведения рейдера. Лежат в отдельном ассете, чтобы несколько рейдеров
    // могли делить одни настройки, а геймдизайнер менял их, не трогая префаб и код.
    [CreateAssetMenu(fileName = "RaiderConfig", menuName = "Dead Air/Raider Config")]
    public class RaiderConfig : ScriptableObject
    {
        [Header("Патруль")]
        public float patrolSpeed = 2.2f;
        public float waitOnPoint = 0.8f;
        public float arriveDistance = 0.75f;

        [Header("Подозрение")]
        [Range(0f, 1f)] public float suspiciousLevel = 0.25f; // с какого значения шкалы рейдер настораживается
        public float suspiciousTime = 1.6f;                   // минимум столько стоит и смотрит

        [Header("Поиск")]
        public float searchSpeed = 3.4f;
        public float searchTime = 5f;         // сколько осматривается на месте
        public float searchTurnSpeed = 70f;

        [Header("Бой")]
        public float combatSpeed = 4.2f;
        public float combatDistance = 6f;     // ближе не подходит: стоит и стреляет
        [Range(0f, 1f)] public float loseTargetLevel = 0.15f; // шкала упала ниже - игрок потерян

        [Header("Сдача")]
        public bool canSurrender = true;
        [Range(0f, 1f)] public float surrenderHealth = 0.3f;  // 30% HP

        [Header("Цвет тела по состоянию (красный - угроза)")]
        public Color calmColor = new Color32(0x6B, 0x2E, 0x22, 0xFF);
        public Color suspiciousColor = new Color32(0xEB, 0xA1, 0x38, 0xFF);
        public Color searchColor = new Color32(0xB5, 0x56, 0x2E, 0xFF);
        public Color combatColor = new Color32(0xD6, 0x3D, 0x2B, 0xFF);
        public Color surrenderedColor = new Color32(0x8C, 0x85, 0x78, 0xFF);
        public Color deadColor = new Color32(0x2A, 0x27, 0x24, 0xFF);
    }
}
