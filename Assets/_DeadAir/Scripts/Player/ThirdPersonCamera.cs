using DeadAir.Controls;
using UnityEngine;

namespace DeadAir.Player
{
    // Камера от третьего лица из-за правого плеча.
    // Плечо нужно, чтобы прицел в центре экрана не закрывал сам игрок.
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;    // игрок
        [SerializeField] private Transform yawPivot;  // объект в игроке, по нему PlayerMotor понимает, где "вперёд"
        [SerializeField] private PlayerInputHandler input;

        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float height = 1.7f;
        [SerializeField] private float shoulderOffset = 0.75f;
        [SerializeField] private float sensitivity = 0.12f;
        [SerializeField] private float minPitch = -40f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float wallPadding = 0.25f;

        private float yaw;
        private float pitch = 6f;

        private void Start()
        {
            yaw = target.eulerAngles.y;
        }

        private void LateUpdate()
        {
            // Пока курсор свободен (нажали Esc), мышь камеру не крутит
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = input.Look;
                yaw += look.x * sensitivity;
                pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);
            }

            yawPivot.rotation = Quaternion.Euler(0f, yaw, 0f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 head = target.position + Vector3.up * height;
            Vector3 shoulder = head + rotation * Vector3.right * shoulderOffset;
            Vector3 cameraPosition = shoulder - rotation * Vector3.forward * distance;

            transform.position = AvoidWalls(head, cameraPosition);
            transform.rotation = rotation;
        }

        // Если между головой и камерой стена - придвигаем камеру ближе
        private Vector3 AvoidWalls(Vector3 head, Vector3 cameraPosition)
        {
            Vector3 direction = cameraPosition - head;
            if (Physics.SphereCast(head, wallPadding, direction.normalized, out RaycastHit hit, direction.magnitude))
                return head + direction.normalized * Mathf.Max(0.3f, hit.distance - 0.05f);

            return cameraPosition;
        }
    }
}
