using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// This script implements the Player mechanics for shooting and health

public class Gun : MonoBehaviour {

    public bool gameState = true;

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public float bulletTravel = 100.0f;
    public bool isDead;
 

    public Text magBullets;
    public Text remainingBullets;
    public Text healthDisplay;

    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    public GameObject bulletHole;
    public GameObject bulletHoleFlesh;
    public GameObject muzzleFlash;
    public GameObject bulletSound;

    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {

        if (gameState)
        {
            // Cool down times
            if (gunShotTime >= 0.0f)
            {
                gunShotTime -= Time.deltaTime;
            }
            if (gunReloadTime >= 0.0f)
            {
                gunReloadTime -= Time.deltaTime;
            }


            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
            {
                Tuple<bool, RaycastHit> shot = shotDetection(); // Done (Gets a Tuple consisting of 1) Boolean to check if the raycast collided, 2) The RayCastHit object.)

                addEffects(shot.Item1, shot.Item2); // Done (Adds 1)BulletHole prefab at bullet collision location, 2) Muzzle Flash prefab at gun end posiiton, 3) Adds bullet firing sound to the gun end position.)

                animator.SetBool("fire", true);
                gunShotTime = 0.5f;

                // Instantiating the muzzle prefab and shot sound

                magBulletsVal = magBulletsVal - 1;
                if (magBulletsVal <= 0 && remainingBulletsVal > 0)
                {
                    animator.SetBool("reloadAfterFire", true);
                    gunReloadTime = 2.5f;
                    Invoke("reloaded", 2.5f);
                }
            }
            else
            {
                animator.SetBool("fire", false);
            }

            if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead)
            {
                animator.SetBool("reload", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.0f);
            }
            else
            {
                animator.SetBool("reload", false);
            }
            updateText();
        }
       
    }

    public void GameOver()
    {
        Debug.Log("Game Restarting...");
        gameState = false;
        Invoke("Restart", 10.0f);
    }

    public void Restart()
    {
        gameObject.GetComponent<CharacterController>().enabled = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
  

    public void Being_shot(float damage) // getting hit from enemy
    {
        if (!isDead)
        {
            health -= damage;
            if (health < 0) health = 0;

            healthDisplay.text = health.ToString();

            if (health <= 0)
            {
                isDead = true;
                gameObject.GetComponent<CharacterController>().enabled = false;
                animator.SetBool("dead", true);
                GameOver();
            }
        }
        
    }

    public void RefillAmmo()
    {
        remainingBulletsVal = 90;
        magBulletsVal = magSize;
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (eventNumber == 1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        if (eventNumber == 2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();
    }

    Tuple <bool, RaycastHit> shotDetection() // Detecting the object which player shot 
    {
        RaycastHit rayHit;
        int layerMask = 1 << 9;
        // Debug.Log(layerMask);
        layerMask = ~layerMask;
        // Debug.Log(layerMask);

        bool hitCheck = Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, bulletTravel);
        //RaycastHit[] items = Physics.RaycastAll(end.transform.position, (end.transform.position - start.transform.position).normalized, bulletTravel);

        //foreach(RaycastHit item in items)
        //{
        //    Debug.Log(item.transform.tag);
        //}

        return Tuple.Create(hitCheck, rayHit);
    }

    void addEffects(bool hitCheck, RaycastHit rayHit) // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // Add a bullet hole if collision with a wall-like layers
        if (hitCheck)
        {
            //Debug.Log(rayHit.collider.gameObject.tag);
            if (rayHit.collider.tag == "environment")
            {
                GameObject bulletHoleObj = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
            }
            else if (rayHit.collider.tag == "enemy_head")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to enemy
                rayHit.collider.GetComponentInParent<EnemyController>().Being_shot(rayHit.collider.GetComponentInParent<EnemyController>().maxHealth);
                rayHit.collider.GetComponentInParent<EnemyController>().force_detect = true;
            }
            else if (rayHit.collider.tag == "enemy_chest")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to enemy
                rayHit.collider.GetComponentInParent<EnemyController>().Being_shot(rayHit.collider.GetComponentInParent<EnemyController>().maxHealth * 0.3f);
                rayHit.collider.GetComponentInParent<EnemyController>().force_detect = true;
            }
            else if (rayHit.collider.tag == "enemy_hands")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to enemy
                rayHit.collider.GetComponentInParent<EnemyController>().Being_shot(rayHit.collider.GetComponentInParent<EnemyController>().maxHealth * 0.1f);
                rayHit.collider.GetComponentInParent<EnemyController>().force_detect = true;
            }
            else if (rayHit.collider.tag == "enemy_legs")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to enemy
                rayHit.collider.GetComponentInParent<EnemyController>().Being_shot(rayHit.collider.GetComponentInParent<EnemyController>().maxHealth * 0.2f);
                rayHit.collider.GetComponentInParent<EnemyController>().force_detect = true;
            }
            else
            {
                GameObject bulletHoleObj = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
            }

        }

        // Muzzle Flash Particle System
        GameObject muzzleFlashObj = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        muzzleFlashObj.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleFlashObj, 1.0f);

        // Bullet shooting sound from gun
        GameObject bulletSoundObj = Instantiate(bulletSound, end.transform.position, end.transform.rotation);
        Destroy(bulletSoundObj, 1.0f);
    }

}
