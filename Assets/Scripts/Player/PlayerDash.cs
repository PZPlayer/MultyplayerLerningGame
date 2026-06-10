using UnityEngine;
using System.Collections;

namespace Thanks.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerDash : MonoBehaviour, IDashInput
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 20f;          // сила рывка
        [SerializeField] private float dashDuration = 0.2f;      // длительность неуязвимости/блокировки
        [SerializeField] private float dashCooldown = 1f;        // перезарядка

        private IDashable movement;
        private float lastDashTime;
        private bool isDashing;

        private void Awake()
        {
            movement = GetComponent<IDashable>();
            if (movement == null)
                Debug.LogError("PlayerDash требует компонент PlayerMovement!");
        }

        /// <summary>
        /// Выполнить рывок в заданном направлении (нормализованном)
        /// </summary>
        public void PerformDash(Vector3 direction)
        {
            if (isDashing) return;
            if (Time.time - lastDashTime < dashCooldown) return;

            lastDashTime = Time.time;
            StartCoroutine(DashCoroutine(direction.normalized));
        }

        private IEnumerator DashCoroutine(Vector3 dir)
        {
            isDashing = true;
            // Мгновенное изменение скорости

            float timer = 0;
            while (timer < dashDuration)
            {
                timer += Time.deltaTime;
                movement.ApplyDashImpulse(dir * dashForce);
                yield return null;
            }

            isDashing = false;
        }
    }
}