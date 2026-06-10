using UnityEngine;
using System.Collections;

public class PlayerLook : MonoBehaviour, IPlayerLookFOV
{
    [Header("Ссылки на трансформы")]
    [SerializeField] private Transform playerYawRoot;   // мгновенный поворот по Y (тело/направление игрока)
    [SerializeField] private Transform cameraPivot;     // объект для вертикального поворота камеры
    [SerializeField] private Camera playerCamera;       // компонент камеры для FOV

    [Header("Настройки ввода (должен быть назначен компонент с IPlayerLookInput)")]
    [SerializeField] private IPlayerLookInput lookInput; // назначается через инспектор или кодом

    [Header("Настройки поворота камеры")]
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;

    [Header("Настройки FOV")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float aimFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 90f; // градусов в секунду

    private float currentPitch = 0f;
    private Coroutine fovCoroutine;

    private void OnValidate()
    {
        // Проверка: если lookInput не назначен, ищем на этом же объекте
        if (lookInput == null)
            lookInput = GetComponent<IPlayerLookInput>();
    }

    private void OnEnable() => lookInput?.Enable();
    private void OnDisable() => lookInput?.Disable();

    private void Update()
    {
        if (lookInput == null) return;

        Vector2 lookDelta = lookInput.LookDelta * Time.deltaTime;

        HandlePitch(lookDelta.y);
        HandleYaw(lookDelta.x);
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

    // ===== Публичные методы для FOV =====
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
