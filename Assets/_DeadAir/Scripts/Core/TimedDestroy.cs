using UnityEngine;

namespace DeadAir.Core
{
    // Удаляет объект через заданное время (вспышка выстрела и т.п.)
    public class TimedDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.06f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
