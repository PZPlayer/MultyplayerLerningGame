using UnityEngine;

namespace Thanks.Player
{
    public class CameraEffectsController : MonoBehaviour, ICameraEffects
    {
        [SerializeField] private Transform effectsPivot; // дочерний объект к CameraPivot

        // Punch
        private bool isPunching;
        private Vector2 punchTarget;     // целевое смещение (x = yaw, y = pitch)
        private Vector2 punchStart;
        private float punchSpeed;
        private float punchProgress;

        // Shake
        private bool isShaking;
        private float shakeIntensity;
        private float shakeRemainingTime;
        private float shakeInitialDuration;
        private Vector2 shakeOffset;

        // “екущее базовое смещение (результат Punch'а)
        private Vector2 currentBaseOffset;

        private void Start()
        {
            if (effectsPivot == null)
                Debug.LogError("CameraEffectsController: effectsPivot не назначен!");
            else
                effectsPivot.localRotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (effectsPivot == null ) return;

            // --- ќбработка Punch (смещение без возврата) ---
            if (isPunching)
            {
                punchProgress += punchSpeed * Time.deltaTime;
                if (punchProgress >= 1f)
                {
                    currentBaseOffset = punchTarget;
                    isPunching = false;
                }
                else
                {
                    currentBaseOffset = Vector2.Lerp(punchStart, punchTarget, punchProgress);
                }
            }

            // --- ќбработка Shake (тр€ска поверх базового смещени€) ---
            if (isShaking)
            {
                shakeRemainingTime -= Time.deltaTime;
                if (shakeRemainingTime <= 0f)
                {
                    isShaking = false;
                    shakeOffset = Vector2.zero;
                }
                else
                {
                    float t = shakeRemainingTime / shakeInitialDuration; // от 1 до 0
                    float currentIntensity = shakeIntensity * t;
                    shakeOffset.x = Random.Range(-currentIntensity, currentIntensity);
                    shakeOffset.y = Random.Range(-currentIntensity, currentIntensity);
                }
            }
            else
            {
                //  огда тр€ска закончена, смещение обнул€етс€ (уже обнулено выше)
                // Ќичего дополнительно не делаем
            }

            // ѕримен€ем итоговый поворот: базовое смещение + тр€ска
            Vector2 total = currentBaseOffset + shakeOffset;
            effectsPivot.localRotation = Quaternion.Euler(total.y + effectsPivot.localRotation.y, total.x + effectsPivot.localRotation.x, 0f);
        }

        public void Punch(Vector2 delta, float speed)
        {
            if (effectsPivot == null) return;
            punchStart = currentBaseOffset;           // стартуем от текущей позиции
            punchTarget = currentBaseOffset + delta;  // цель = текуща€ + дельта
            punchSpeed = Mathf.Abs(speed);
            punchProgress = 0f;
            isPunching = true;
        }

        public void Shake(float intensity, float duration, float decay = 0.9f)
        {
            if (effectsPivot == null) return;
            shakeIntensity = intensity;
            shakeRemainingTime = duration;
            shakeInitialDuration = duration;
            isShaking = true;
            // decay можно использовать дл€ нелинейного затухани€, но дл€ простоты оставлен линейный спад
        }

        public void StopAll()
        {
            isPunching = false;
            isShaking = false;
            currentBaseOffset = Vector2.zero;
            shakeOffset = Vector2.zero;
            effectsPivot.localRotation = Quaternion.identity;
        }
    }
}