using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadAir.UI
{
    // Полоска показателя (здоровье, кислород): заполнение + процент справа
    public class StatBar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private TMP_Text valueText;

        private int shownPercent = -1;

        public void Show(float value, Color color)
        {
            fill.fillAmount = value;
            fill.color = color;
            valueText.color = color;

            // Текст пересобираем, только когда число поменялось
            int percent = Mathf.RoundToInt(value * 100f);
            if (percent != shownPercent)
            {
                shownPercent = percent;
                valueText.text = percent + "%";
            }
        }
    }
}
