using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForPlayer_Back : MonoBehaviour
{
    public bool isInBox = false;

    void OnTriggerStay(Collider other){
        if(other.CompareTag("Player")){
            isInBox = true;
        }
    }
    void OnTriggerExit(Collider other){
        if(other.CompareTag("Player")){
            isInBox = false;
        }
    }
}
