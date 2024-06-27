using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiDetectZone : MonoBehaviour
{

    public bool EnnemiInZone = false;
    void OnTriggerStay(Collider other){
        if(other.CompareTag("Ennemi")){
            EnnemiInZone = true;
        }
    }
    void OnTriggerExit(Collider other){
        if(other.CompareTag("Ennemi")){
            EnnemiInZone = false;
        }
    }



}
