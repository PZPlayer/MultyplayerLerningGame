using System.Collections;
using System;
using UnityEngine;

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

    private Coroutine lineDissapear;
    private Coroutine reloadTime;
    private Coroutine holdingShootButton;

    private bool holdingShtButton;

    private Camera cam;
    private ICameraEffects effects;

    private float lastTimeShooted;
    private int magazine;

    void IWeaponInitiliazble.Initiliaze(ICameraEffects eff, Camera camera)
    {
        effects = eff;
        cam = camera;

        magazine = _data.Ammo;
    }

    void IAimable.Aim()
    {
        Debug.Log("Aiming!");
    }

    void IReloadble.Reload() 
    {
        if (reloadTime == null)
            reloadTime = StartCoroutine(ReloadMagazine());

        Debug.Log("Reloading!");
    }

    void IShootable.Shoot()
    {
        holdingShtButton = true;

        if (holdingShootButton == null)
            holdingShootButton = StartCoroutine(HoldShootButton());
    }

    private void ExecuteShooting()
    {
        Vector2 punchDir = new Vector2(UnityEngine.Random.Range(-maxKnockOffValue, maxKnockOffValue), UnityEngine.Random.Range(-maxKnockOffValue, maxKnockOffValue));
        effects.Punch(punchDir, powerKnockOffValue);

        RaycastHit hit;
        if (Physics.Raycast(_shootPoint.transform.position, cam.transform.forward, out hit, maxHitDistance, hitLayer))
        {
        }

        if (lineDissapear == null)
            lineDissapear = StartCoroutine(CastALine(cam.transform.forward));

        OnSuccsesfulShot?.Invoke(); 
    }

    private IEnumerator HoldShootButton()
    {
        while (holdingShtButton)
        {
            if (magazine <= 0 || Time.time - lastTimeShooted <= _data.ShootBetweenTime || reloadTime != null)
            {
                if (magazine <= 0)
                {
                    this.GetComponent<IReloadble>().Reload();
                }

                yield return null;

                continue;
            }

            ExecuteShooting();
            magazine--;
            lastTimeShooted = Time.time;

            yield return null;
        }
    }

    private IEnumerator ReloadMagazine()
    {
        yield return new WaitForSeconds(_data.ReloadTime);

        magazine = _data.Ammo;
        reloadTime = null;
    }

    private IEnumerator CastALine(Vector3 dir)
    {
        if (lineDissapear != null) yield break;

        LineRenderer lineRenderer = Instantiate(lineBullet).GetComponent<LineRenderer>();
        Destroy(lineRenderer.gameObject, lineLifeTime * 1.5f);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, _shootPoint.gameObject.transform.position);
        lineRenderer.SetPosition(1, dir * maxHitDistance);


        float timer = 0;

        while (timer < lineLifeTime)
        {
            timer += Time.deltaTime;

            lineRenderer.startWidth = startLineWidth * (1 - timer / lineLifeTime);
            lineRenderer.endWidth = startLineWidth * (1 - timer / lineLifeTime);

            yield return null;
        }

        lineDissapear = null;
        lineRenderer.enabled = false;
    }

    void IAimable.StopAiming()
    {
    }

    void IShootable.StopShooting()
    {
        holdingShootButton = null;
        holdingShtButton = false;
    }
}
