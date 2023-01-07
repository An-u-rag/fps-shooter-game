using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script implements the Ammo Refill Trigger to restock player's ammo. (Bonus)

public class AmmoRefillTrigger : MonoBehaviour
{
    private bool check = false;
    // Start is called before the first frame update
    void Start()
    {
        check = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (check == false && other.gameObject.name == "player")
        {
            Gun gun = other.gameObject.GetComponent<Gun>();
            if(gun != null)
            {
                gun.RefillAmmo();
                Debug.Log("Ammo Refilled");
                check = true;
                // Refill will be available after 10 seconds again
                Invoke("SetCheck", 10.0f);
            }
        }
    }

    void SetCheck()
    {
        check = false;
    }
}