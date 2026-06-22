using System.Collections;
using System;
using UnityEngine;
using Mirror;

// ВАЖНО: WeaponBase должен наследоваться от Mirror.NetworkBehaviour,
// иначе [Command]/[ClientRpc]/[SyncVar]/hasAuthority/isServer не скомпилируются.
public class Pistol : WeaponBase, IBaseWeapon, IWeaponInitiliazble
{
    public event Action OnSuccsesfulShot;

    [SerializeField] private float maxKnockOffValue;
    [SerializeField] private float powerKnockOffValue;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float lineLifeTime;
    [SerializeField] private float startLineWidth;
    [SerializeField] private float maxHitDistance;
    [SerializeField] private GameObject lineBullet;

    private Coroutine holdingShootButton;
    private bool holdingShtButton;

    private Camera cam;
    private ICameraEffects effects;
    private Vector2 punchDir;
    // lastTimeShooted и reloadCoroutine НЕ синкаются специально:
    // у сервера своя копия (источник правды), у владельца клиента — своя
    // (используется только для локального предсказания/анти-спама команд).
    private float lastTimeShooted;
    private Coroutine reloadCoroutine;

    // SyncVar — патроны должны быть видны (хотя бы) владельцу для UI.
    [SyncVar(hook = nameof(OnMagazineChanged))]
    private int magazine;

    void IWeaponInitiliazble.Initiliaze(ICameraEffects eff, Camera camera)
    {
        effects = eff;
        cam = camera;

        if (effects == null || cam == null)
            Debug.LogWarning("Pistol: ICameraEffects не передан в Initiliaze. Эффекты камеры работать не будут.");

        // Магазин выставляет только сервер — это синкается клиентам через SyncVar.
        if (isServer)
            magazine = _data.Ammo;
    }

    void IAimable.Aim(){}
    void IAimable.StopAiming() { }

    void IReloadble.Reload()
    {
        if (!isOwned) return;
        CmdReload();
    }

    void IShootable.Shoot()
    {

        if (!isOwned) return; // стрелять может только владелец этого оружия

        holdingShtButton = true;
        if (holdingShootButton == null)
            holdingShootButton = StartCoroutine(HoldShootButton());
    }

    void IShootable.StopShooting()
    {
        holdingShootButton = null;
        holdingShtButton = false;
    }

    private IEnumerator HoldShootButton()
    {
        while (holdingShtButton)
        {
            if (magazine <= 0)
            {
                this.GetComponent<IReloadble>().Reload();
                yield return null;
                continue;
            }

            // Локальная "грубая" проверка кулдауна — просто чтобы не спамить Command'ы.
            // Реальная проверка всё равно на сервере.
            if (Time.time - lastTimeShooted < _data.ShootBetweenTime)
            {
                yield return null;
                continue;
            }


            CmdShoot(_shootPoint.position, cam.transform.forward);

            yield return null;
        }
    }

    [Command]
    private void CmdShoot(Vector3 origin, Vector3 direction)
    {
        // ---- ВСЁ НИЖЕ ВЫПОЛНЯЕТСЯ ТОЛЬКО НА СЕРВЕРЕ ----

        if (magazine <= 0) return;


        if (Time.time - lastTimeShooted < _data.ShootBetweenTime) return;


        // Простейшая защита от подмены origin (телепорт/читы):
        // если клиент прислал точку далеко от того, где сервер реально видит игрока — игнорируем её.
        if (Vector3.Distance(origin, _shootPoint.position) > 1f)
            origin = _shootPoint.position;

        direction = direction.normalized;
        Vector3 endPoint = origin + direction * maxHitDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxHitDistance, hitLayer))
        {
            endPoint = hit.point;

            // Замени IDamageable на свой реальный интерфейс/класс урона, если он называется иначе.
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(_data.Damage);
        }

        magazine--;
        lastTimeShooted = Time.time;

        RpcShowTracer(origin, endPoint);
        OnSuccsesfulShot?.Invoke();
    }

    // Рассылается всем клиентам (включая стрелка) — рисуем трассер одинаково у всех.
    [ClientRpc]
    private void RpcShowTracer(Vector3 start, Vector3 end)
    {
        if (isOwned)
        {
            punchDir = new Vector2(
                UnityEngine.Random.Range(-maxKnockOffValue, maxKnockOffValue),
                UnityEngine.Random.Range(-maxKnockOffValue, maxKnockOffValue));

            effects?.Punch(punchDir, powerKnockOffValue);
            Debug.Log("Pistol: RpcShowTracer вызван на владельце, вызываем эффекты камеры.");
        }
            

        StartCoroutine(CastALine(start, end));
    }

    [Command]
    private void CmdReload()
    {
        if (reloadCoroutine != null) return;
        reloadCoroutine = StartCoroutine(ReloadMagazine());
    }

    [Server]
    private IEnumerator ReloadMagazine()
    {
        yield return new WaitForSeconds(_data.ReloadTime);
        magazine = _data.Ammo;
        reloadCoroutine = null;
    }

    // Хук вызывается на каждом клиенте, когда magazine меняется на сервере.
    private void OnMagazineChanged(int oldValue, int newValue)
    {
        if (!isOwned) return;
        // Сюда повесь обновление UI патронов, например:
        // UIManager.Instance.UpdateAmmo(newValue, _data.Ammo);
    }

    // Теперь каждый выстрел создаёт свой независимый объект линии,
    // быстрая стрельба больше не "съедает" новые трассеры.
    private IEnumerator CastALine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.red, lineLifeTime);

        LineRenderer lineRenderer = Instantiate(lineBullet).GetComponent<LineRenderer>();
        Destroy(lineRenderer.gameObject, lineLifeTime * 1.5f);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        float timer = 0f;
        while (timer < lineLifeTime)
        {
            timer += Time.deltaTime;
            float t = 1f - timer / lineLifeTime;
            lineRenderer.startWidth = startLineWidth * t;
            lineRenderer.endWidth = startLineWidth * t;
            yield return null;
        }

        lineRenderer.enabled = false;
    }
}