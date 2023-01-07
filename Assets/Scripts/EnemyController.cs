using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the AI enemy's movement and shooting and health mechanics.

public class EnemyController : MonoBehaviour
{
    Animator animator;
    public GameObject spine;

    // Target locations for movement
    public GameObject[] targetLocations;
    private Vector3 currentTargetLocation;
    private int locCount;
    private int total;

    public float animation_speed;
    public float fieldOfView;
    public float sight;
    public float range;
    public float reactionTime;
    public float enemy_run_speed_factor;
    public float enemy_aim_speed;

    public bool isDead;
    public float health = 100.0f;
    public float maxHealth = 100.0f;

    private Vector3 move_direction;
    private Vector3 player_direction;

    private bool isDetect;
    private GameObject player;

    // Enemy variables
    private Vector3 currentPosition;
    private Vector3 forwardDirection;
    float distance_to_player;
    float enemy_angle;

    // Enemy Gun Variables
    public GameObject end, start; // The gun start and end point
    public GameObject gun;

    float gunShotTime = 0.1f;
    public float bulletTravel = 100.0f;

    public GameObject bulletHole;
    public GameObject bulletHoleFlesh;
    public GameObject muzzleFlash;
    public GameObject bulletSound;

    public Vector3 randomSpreadRange;
    public float enemy_accuracy_percentage;

    public bool force_detect;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        // Initializing animator values
        isDetect = false;
        animation_speed = 1.0f;

        total = targetLocations.Length;
        if (total > 0)
        {
            locCount = 0;
            currentTargetLocation = targetLocations[locCount].transform.position;
        }

        player = GameObject.Find("player");

        force_detect = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Updates to variables every frame
        // Get direction to player from enemy
        player_direction = (player.transform.position - transform.position).normalized;
        currentPosition = transform.position;
        // Get field of view of enemy w.r.t forward direction.
        forwardDirection = transform.forward;

        // Check if Enemy is alive
        if (!isDead)
        {
            // Detect player
            isDetect = DetectPlayer();

            // If player is detected initiate run to player till a distance of 10 meters and maintain distance
            if (isDetect || force_detect)
            {
                animator.SetBool("walk", false);
                // Check if the distance between player and enemy is less than 10 meters
                if (distance_to_player <= 10)
                {
                    // If it is less than or equal to 10 meters, then shoot
                    animator.SetBool("run", false);
                    Quaternion desiredRotation = Quaternion.LookRotation(player_direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * enemy_aim_speed);

                    // Shoot
                    ShootAtPlayer();
                }
                else if (distance_to_player > 10)
                {
                    // If distance is more than 10 meters then close the gap by running to 10 meters
                    animator.SetBool("fire", false);
                    animator.SetBool("run", true);
                    animator.SetFloat("run_animation_speed", enemy_run_speed_factor);
                    Quaternion desiredRotation = Quaternion.LookRotation(player_direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * 5.0f);
                }

            }
            // If player is not detected, continue on target location path.
            else
            {
                force_detect = false;
                animator.SetBool("fire", false);
                animator.SetBool("run", false);
                animator.SetBool("walk", true);
                UpdateTargetLocation();
                MoveEnemy();
 
            }
        }
        else
        {
            animator.SetBool("fire", false);
            animator.SetBool("run", false);
            animator.SetBool("walk", false);
        }
    }

    public void Being_shot(float damage) // getting hit from enemy
    {
        if (!isDead)
        {
            health -= damage;

            if (health <= 0)
            {
                isDead = true;
                gameObject.GetComponent<CharacterController>().enabled = false;
                animator.SetTrigger("dead");

                // Separate gun and add rigidbody plus collider to it.
                gun.AddComponent<Rigidbody>();
                CapsuleCollider gunCollider = gun.AddComponent<CapsuleCollider>();
                gunCollider.radius = 0.2f;
                gunCollider.height = 1;
                gunCollider.direction = 2;
                gun.transform.parent = null;
            }
        }
    }

   
    public bool DetectPlayer()
    {
        // Get distance to player from enemy
        distance_to_player = Vector3.Distance(transform.position, player.transform.position);
        //float distance_to_wall;
        //Tuple<bool, RaycastHit> check = isWall();
        
        // Get direction that enemy is facing
        enemy_angle = Vector3.Angle(transform.forward, player_direction);

        // Check if the distance is less that threshold = sight
        // AND also if enemy is looking in the direction of where player is
        if((distance_to_player <= sight && enemy_angle <= fieldOfView))
        {
            // If player in sight and facing player
            // check if enemy is looking at a wall and player_distance is more than wall_distance
            //if (check.Item1)
            //{
            //    // Calculate distance to wall
            //    distance_to_wall = Vector3.Distance(transform.position, check.Item2.transform.position);
            //    if (distance_to_player > distance_to_wall)
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //}
            return true;
        } else if (distance_to_player <= sight*0.2) {
            // If player is very close despite the angle return true
            // Enemy can hear player
            // Calculate distance to wall
            //distance_to_wall = Vector3.Distance(transform.position, check.Item2.transform.position);
            //if (distance_to_player > distance_to_wall)
            //{
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}
            return true;
        }
        else
            return false;
    }

    void UpdateTargetLocation()
    {
        if (!isDead)
        {
            float xDisplacement = Mathf.Abs(currentPosition.x - currentTargetLocation.x);
            float zDisplacement = Mathf.Abs(currentPosition.z - currentTargetLocation.z);
            if (xDisplacement < 0.5f && zDisplacement < 0.5f)
            {
                if (locCount < total-1)
                {
                    locCount += 1;
                }
                else
                {
                    locCount = 0;
                }
                currentTargetLocation = targetLocations[locCount].transform.position;
            }
        }
        
    }

    void MoveEnemy()
    {
        Quaternion desiredRotation = Quaternion.LookRotation(currentTargetLocation - currentPosition);

        animator.SetFloat("animation_speed", animation_speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime*1.2f);

    }

    void ShootAtPlayer()
    {
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }

        if (gunShotTime <= 0)
        {
            animator.SetBool("fire", true);

            Tuple<bool, RaycastHit> shot = shotDetection(); // Done (Gets a Tuple consisting of 1) Boolean to check if the raycast collided, 2) The RayCastHit object.)

            addEffects(shot.Item1, shot.Item2); // Done (Adds 1)BulletHole prefab at bullet collision location, 2) Muzzle Flash prefab at gun end posiiton, 3) Adds bullet firing sound to the gun end position.)

            gunShotTime = 0.5f;
        }
        
    }

    Tuple<bool, RaycastHit> shotDetection() // Detecting the object which enemy shot 
    {
        RaycastHit rayHit;

        // 20% chance to hit player - enemy_accuracy
        int rand = UnityEngine.Random.Range(0, 101);
        bool hitCheck;

        if (rand <= enemy_accuracy_percentage)
        {
            // sure impact to player
            // Check for headshot or not (50% probability)
            int headshot_chance = UnityEngine.Random.Range(0, 2);
            Vector3 shootDirection;
            if (headshot_chance == 1)
            {
                Vector3 playerheadpos = player.transform.Find("swat:Hips").Find("swat:Spine").Find("swat:Spine1").Find("swat:Spine2").Find("swat:Neck").position;
                shootDirection = (playerheadpos - end.transform.position).normalized;
            }
            else
            {
                Vector3 playerbodypos = player.transform.Find("swat:Hips").position;
                shootDirection = (playerbodypos - end.transform.position).normalized;
            }
            // add vertical(y-axis) spread only to player for headshot %
            // Random Offset
            hitCheck = Physics.Raycast(end.transform.position, shootDirection, out rayHit, bulletTravel);
        }
        else
        {
            // else add a bullet spread
            // Random Offset
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(0, randomSpreadRange.x), UnityEngine.Random.Range(0, randomSpreadRange.y), UnityEngine.Random.Range(0, randomSpreadRange.z));
            Vector3 playerbodypos = player.transform.Find("swat:Hips").position;
            Vector3 shootDirection = (playerbodypos - end.transform.position).normalized;
            hitCheck = Physics.Raycast(end.transform.position, (shootDirection + randomOffset).normalized, out rayHit, bulletTravel);
        }

        return Tuple.Create(hitCheck, rayHit);
    }

    void addEffects(bool hitCheck, RaycastHit rayHit) // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // Add a bullet hole if collision with a wall-like layers
        if (hitCheck)
        {
            if (rayHit.collider.tag == "environment")
            {
                GameObject bulletHoleObj = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
            }
            else if (rayHit.collider.tag == "player_head")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to player
                player.GetComponent<Gun>().Being_shot(player.GetComponent<Gun>().maxHealth);
            }
            else if (rayHit.collider.tag == "player_body")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to player
                player.GetComponent<Gun>().Being_shot(player.GetComponent<Gun>().maxHealth * 0.3f);
            }
            else if (rayHit.collider.tag == "player_legs")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to player
                player.GetComponent<Gun>().Being_shot(player.GetComponent<Gun>().maxHealth * 0.2f);
            }
            else if (rayHit.collider.tag == "player_hands")
            {
                GameObject bulletHoleObj = Instantiate(bulletHoleFlesh, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObj, 2.5f);
                // Do Damage to player
                player.GetComponent<Gun>().Being_shot(player.GetComponent<Gun>().maxHealth * 0.1f);
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
