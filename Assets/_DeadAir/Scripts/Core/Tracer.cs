using UnityEngine;

namespace DeadAir.Core
{
    // Трассер пули: линия от ствола до точки попадания, живёт доли секунды.
    [RequireComponent(typeof(LineRenderer))]
    public class Tracer : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.08f;

        public void Show(Vector3 from, Vector3 to, Color color)
        {
            LineRenderer line = GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.3f);

            Destroy(gameObject, lifetime);
        }
    }
}
