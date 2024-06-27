using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail_Player_script : MonoBehaviour
{
    public NewBehaviourScript Joueur;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        double PlayerVitesse = Joueur.Speed;
        
        if(Joueur != null){
            if(PlayerVitesse *3.6f > 100){
                GetComponent<TrailRenderer>().enabled = true;
            } else {
                GetComponent<TrailRenderer>().enabled = false;
            }
        }
        
        
    }
}
