using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to fire a trigger once the player reaches the door at the end of the game to restart the game.

public class LevelPass : MonoBehaviour
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
        if(check == false && other.gameObject.name == "player")
        {
            Gun gun = other.gameObject.GetComponent<Gun>();
            if (gun != null)
            {
                gun.GameOver();
                check = true;
            }
        }
    }
}
