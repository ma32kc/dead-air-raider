using DeadAir.Controls;
using DeadAir.Core;
using UnityEngine;

namespace DeadAir.Player
{
    // Движение игрока относительно камеры, бег и прыжок.
    // Что доступно (бег, прыжок) и как высоко прыгаем - зависит от гравитации планеты.
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlanetGravity planetGravity = PlanetGravity.Base;
        [SerializeField] private float walkSpeed = 4f;     // скафандр громоздкий - ходим не быстро
        [SerializeField] private float runSpeed = 6.5f;
        [SerializeField] private float turnSpeed = 10f;

        // Камера поворачивает этот объект, отсюда берём направление "вперёд"
        [SerializeField] private Transform cameraPivot;

        [Header("Шаги")]
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private float stepInterval = 0.48f;
        [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.25f;

        private CharacterController controller;
        private PlayerInputHandler input;
        private Health health;
        private float verticalSpeed;
        private float stepTimer;
        private bool jumpRequested;

        // Эти значения выставляет ApplyGravity по типу планеты
        private float gravity;
        private float jumpHeight;
        private bool canRun;
        private bool canJump;

        public PlanetGravity Gravity => planetGravity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputHandler>();
            health = GetComponent<Health>();
            if (footstepSource == null)
                footstepSource = GetComponent<AudioSource>();

            // Игрок всегда рядом с камерой - 2D, без Create/Destroy на каждый шаг
            footstepSource.playOnAwake = false;
            footstepSource.loop = false;
            footstepSource.spatialBlend = 0f;
            ApplyGravity();
        }

        private void OnEnable()
        {
            input.JumpPressed += OnJumpPressed;
        }

        private void OnDisable()
        {
            input.JumpPressed -= OnJumpPressed;
        }

        // Вызывается и при изменении поля в инспекторе - можно переключать гравитацию прямо в Play
        private void OnValidate()
        {
            ApplyGravity();
        }

        private void ApplyGravity()
        {
            switch (planetGravity)
            {
                case PlanetGravity.Low:
                    gravity = -6f;
                    jumpHeight = 3f;
                    canRun = false;
                    canJump = true;
                    break;

                case PlanetGravity.Base:
                    gravity = -15f;
                    jumpHeight = 1.5f;
                    canRun = true;
                    canJump = true;
                    break;

                case PlanetGravity.High:
                    gravity = -30f;
                    jumpHeight = 0f;
                    canRun = true;
                    canJump = false;
                    break;
            }
        }

        private void Update()
        {
            if (health.IsDead)
                return;

            Vector3 move = GetMoveDirection();
            bool running = canRun && input.SprintHeld;
            float speed = running ? runSpeed : walkSpeed;

            RotateTowards(move);
            UpdateGravity();

            // Move вызываем один раз за кадр, иначе капсула может застрять в стене
            Vector3 velocity = move * speed;
            velocity.y = verticalSpeed;
            controller.Move(velocity * Time.deltaTime);

            UpdateFootsteps(move, running);
        }

        private void OnJumpPressed()
        {
            jumpRequested = true;
        }

        // Ввод (x, y) -> направление в мире относительно камеры (без наклона вверх/вниз)
        private Vector3 GetMoveDirection()
        {
            Vector2 moveInput = Vector2.ClampMagnitude(input.Move, 1f);

            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;
            forward.y = 0f;
            right.y = 0f;

            return forward.normalized * moveInput.y + right.normalized * moveInput.x;
        }

        private void RotateTowards(Vector3 move)
        {
            if (move.magnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        private void UpdateGravity()
        {
            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f; // немного прижимаем к земле

            if (jumpRequested && canJump && controller.isGrounded)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;

            verticalSpeed += gravity * Time.deltaTime;
        }

        private void UpdateFootsteps(Vector3 move, bool running)
        {
            if (!controller.isGrounded || move.magnitude < 0.1f)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = running ? stepInterval * 0.7f : stepInterval;
            }
        }

        private void PlayFootstep()
        {
            if (footstepSound == null)
                return;

            footstepSource.pitch = Random.Range(0.95f, 1.05f);
            footstepSource.PlayOneShot(footstepSound, footstepVolume);
        }
    }
}
