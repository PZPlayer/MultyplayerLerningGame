using UnityEngine;
using System.Collections;
using Mirror;

public class PlayerLook : NetworkBehaviour, IPlayerLookFOV, ICameraEffects
{
    [Header("Ссылки на трансформы")]
    [SerializeField] private Transform playerYawRoot;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Настройки ввода")]
    [SerializeField] private IPlayerLookInput lookInput;

    [Header("Настройки поворота камеры")]
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;

    [Header("Настройки FOV")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float aimFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 90f;

    private float currentPitch = 0f;
    private Coroutine fovCoroutine;
    private Coroutine punchCoroutine;   // корутина для Punch

    // Эффекты камеры
    private Vector2 activePunchOffset;  // текущее смещение от Punch (плавно меняется)
    private Vector2 shakeOffset;        // смещение от тряски
    private Coroutine shakeCoroutine;

    private void OnEnable()
    {
        lookInput?.Enable(); lookInput = GetComponent<IPlayerLookInput>();
    }
    private void OnDisable() => lookInput?.Disable();

    private void Update()
    {
        if (lookInput == null) return;

        Vector2 lookDelta = lookInput.LookDelta * Time.deltaTime;

        // Суммируем мышь, активный Punch и тряску
        float totalPitchDelta = lookDelta.y + activePunchOffset.y * Time.deltaTime + shakeOffset.y * Time.deltaTime;
        float totalYawDelta = lookDelta.x + activePunchOffset.x * Time.deltaTime + shakeOffset.x * Time.deltaTime;

        HandlePitch(totalPitchDelta);
        HandleYaw(totalYawDelta);
    }

    public override void OnStartLocalPlayer()
    {
        print("IAMLOCAL " + isLocalPlayer);

        if (!isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(false);
            this.enabled = false; 
        }

    }

    private void HandlePitch(float deltaPitch)
    {
        currentPitch += deltaPitch;
        currentPitch = Mathf.Clamp(currentPitch, pitchMin, pitchMax);
        cameraPivot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    private void HandleYaw(float deltaYaw)
    {
        playerYawRoot.Rotate(Vector3.up, deltaYaw);
    }

    // ==================== ICameraEffects ====================
    public void Punch(Vector2 delta, float speed)
    {
        if (punchCoroutine != null)
            StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchCoroutine(delta, speed));
    }

    private IEnumerator PunchCoroutine(Vector2 delta, float speed)
    {
        Vector2 start = activePunchOffset;
        Vector2 target = start + delta;
        float elapsed = 0f;

        while (elapsed < speed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / speed;           // 0..1
            activePunchOffset = Vector2.Lerp(start, target, t);
            yield return null;
        }
        activePunchOffset = Vector2.zero;                 // фиксируем точное конечное значение
        punchCoroutine = null;
    }

    public void Shake(float intensity, float duration, float decay = 0.9f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration, decay));
    }

    public void StopAll()
    {
        // Останавливаем Punch
        if (punchCoroutine != null)
            StopCoroutine(punchCoroutine);
        activePunchOffset = Vector2.zero;
        // Останавливаем тряску
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeOffset = Vector2.zero;
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration, float decay)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentIntensity = intensity * Mathf.Pow(1f - t, 1f / Mathf.Max(0.01f, decay));
            shakeOffset.x = Random.Range(-currentIntensity, currentIntensity);
            shakeOffset.y = Random.Range(-currentIntensity, currentIntensity);
            yield return null;
        }
        shakeOffset = Vector2.zero;
        shakeCoroutine = null;
    }

    // ==================== IPlayerLookFOV ====================
    public void ChangeFOV(float targetFOV, float speed)
    {
        if (fovCoroutine != null)
            StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(ChangeFOVCoroutine(targetFOV, speed));
    }

    private IEnumerator ChangeFOVCoroutine(float target, float speed)
    {
        float startFOV = playerCamera.fieldOfView;
        float duration = Mathf.Abs(target - startFOV) / speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, target, t);
            yield return null;
        }
        playerCamera.fieldOfView = target;
        fovCoroutine = null;
    }

    public void SetAimFOV() => ChangeFOV(aimFOV, fovChangeSpeed);
    public void ResetFOV() => ChangeFOV(defaultFOV, fovChangeSpeed);
}