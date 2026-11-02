using System;
using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Raider
{
    // Слух: рейдер слышит любой чужой выстрел в радиусе hearRadius
    // и сообщает об этом через событие ShotHeard.
    public class RaiderHearing : MonoBehaviour
    {
        [SerializeField] private float hearRadius = 40f;

        public event Action<Vector3> ShotHeard;

        private void OnEnable()
        {
            GunshotBus.ShotFired += HandleShot;
        }

        private void OnDisable()
        {
            GunshotBus.ShotFired -= HandleShot;
        }

        private void HandleShot(Vector3 position, GameObject shooter)
        {
            if (shooter == gameObject)
                return; // свои выстрелы не считаем

            if (Vector3.Distance(transform.position, position) > hearRadius)
                return;

            ShotHeard?.Invoke(position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.92f, 0.63f, 0.22f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, hearRadius);
        }
    }
}
