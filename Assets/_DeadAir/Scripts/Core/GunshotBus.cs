using System;
using UnityEngine;

namespace DeadAir.Core
{
    // Глобальное событие "кто-то выстрелил". Глушителей в мире нет, поэтому слышно каждый выстрел.
    // Оружие не знает о рейдерах, а рейдеры - об оружии: они просто подписываются на событие.
    public static class GunshotBus
    {
        public static event Action<Vector3, GameObject> ShotFired; // где стреляли и кто

        public static void Raise(Vector3 position, GameObject shooter)
        {
            ShotFired?.Invoke(position, shooter);
        }
    }
}
