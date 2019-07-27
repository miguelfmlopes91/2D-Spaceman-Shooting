﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float fireRate   = 0f;
    public int Damage     = 10;
    public LayerMask whatToHit;

    public Transform BulletTrailPrefab;
    public Transform MuzzleFlashPrefab;
    public Transform HitPrefab;

    float timeToSpawnEffect = 0f;
    public float timeToSpawnRate = 10f;

    //handle camera shake
    public float camShakeAmount = 0.1f;
    public float camShakeLength = 0.02f;
    CameraShake camShake;

    public string weaponShootSound = "DefaultShot";

    Bardo.AudioManager audioManager;

    float timeToFire = 0f;
    Transform firePoint;
     
    // Start is called before the first frame update
    void Awake(){
        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.Log("No firepoint! WHAT!?");
        }

    }

    private void Start()
    {
        camShake = GameMaster.gm.GetComponent<CameraShake>();
        if (camShake == null)
        {
            Debug.LogError("No camera shake script found on GM object");
        }
        audioManager = Bardo.AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found in scene");
        }
    }
    // Update is called once per frame
    void Update(){
        if (fireRate == 0)//check if it's single burst
        {
            if (Input.GetMouseButton(0))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetMouseButton(0) && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);

        RaycastHit2D hit = Physics2D.Raycast(firePointPosition, mousePosition - firePointPosition, 100, whatToHit);


        Debug.DrawLine(firePointPosition, (mousePosition - firePointPosition) * 100, Color.cyan);

        if (hit.collider != null){
            Debug.DrawLine(firePointPosition, hit.point, Color.red);
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            //Debug.Log("we hit " + hit.collider.name + "and did damage " + Damage);
            if (enemy!= null)
            {
                enemy.DamageEnemy(Damage);
            }
        }

        if (Time.time > timeToSpawnEffect)
        {
            Vector3 hitpos;
            Vector3 hitNormal;

            if (hit.collider == null)
            {
                hitpos = (mousePosition - firePointPosition) * 30;
                hitNormal = new Vector3(9999, 9999, 9999);
            }
            else
            {
                hitpos = hit.point;
                hitNormal = hit.normal;
            }

            Effect(hitpos, hitNormal);
            timeToSpawnEffect = Time.time + 1 / timeToSpawnRate;
        }
    }

    private void Effect(Vector3 hitposition, Vector3 normal)
    {
        Transform trail = Instantiate(BulletTrailPrefab, firePoint.position, firePoint.rotation) as Transform;
        LineRenderer lr = trail.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, hitposition);
        }

        Destroy(trail.gameObject, 0.04f);

        if (normal != new Vector3(9999,9999,9999))
        {
            Transform hitParticle = Instantiate(HitPrefab, hitposition, Quaternion.FromToRotation(Vector3.right, normal)) as Transform;
            Destroy(hitParticle.gameObject, 1f);
        }

        Transform clone = Instantiate(MuzzleFlashPrefab, firePoint.position, firePoint.rotation) as Transform;
        clone.parent = firePoint;
        float size = UnityEngine.Random.Range(0.6f, 0.9f);
        clone.localScale = new Vector3(size, size, size);
        Destroy(clone.gameObject,0.02f);

        camShake.Shake(camShakeAmount, camShakeLength);

        audioManager.PlaySound(weaponShootSound);
    }
}
