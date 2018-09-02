using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    public enum FireMode { Auto, Burst, Single}
    public FireMode fireMode;
    //pos of muzzle
    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float fireRate = 100; //ms
    public float muzzleVelocity = 35; // speed bullet leave the gun
    public int burstCount;
    public int projectilesPerMag;
    public float reloadTime = .3f;

    [Header("Recoil")]
    //RECOIL EFFECT
    public Vector2 kickMinMax = new Vector2(.05f,.2f), recoilAngleMinMax = new Vector2(3,5);
    public float recoilMovSettleTime = .1f, recoilRotSettleTime = .1f;
    Vector3 recoilSmoothDampVel;
    float recoilAngle, recoilRotSmoothDampVel;
    [Header("Effects")]
    public Transform shell, shellEjection;
    public AudioClip shootAudio, reloadAudio;

    int shotsRemainingInBurst;
    int projectilesRemainingInMag;
    float nextShotTime;
    Muzzleflash muzzleflash;
    bool triggerReleased;
    bool isReloading;

    private void Start()
    {
        muzzleflash = GetComponent<Muzzleflash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    //late update updated after all calculations
    private void LateUpdate()
    {
        //animate recoil, return to irginal pos after goin' back
        transform.localPosition = 
            Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVel, recoilMovSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVel, recoilRotSettleTime);
        //apply rotation
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
        //reloading
        if(!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if(!isReloading)
            transform.LookAt(aimPoint);
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleased)
                    return;
            }
            for (int i = 0; i < projectileSpawn.Length; i++) {
                if(projectilesRemainingInMag == 0)
                {
                    break;
                }
                projectilesRemainingInMag--;
                nextShotTime = Time.time + fireRate / 1000;
                Projectile newPr = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newPr.SetSpeed(muzzleVelocity);
            }
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
            //RECOIL
            //move gun back with random \power\
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            //recoil chg angle
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
            //min max value
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
            AudioManager.instance.PlaySound(shootAudio,transform.position);
        }
       
    }
    public void Reload()
    {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag) { 
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }
    //reload animate
    IEnumerator AnimateReload()
    {
        isReloading = true;
        //delay
        yield return new WaitForSeconds(.2f);
        //animation
        float reloadSpeed = 1 / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        while(percent < 1)
        {
            percent += Time.deltaTime* reloadSpeed;
            //zero - value - zero
            float interpolation = (-percent * percent + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            //rotation
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
            //skip
            yield return null;
        }
        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleased = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleased = true;
        shotsRemainingInBurst = burstCount;
    }
}
