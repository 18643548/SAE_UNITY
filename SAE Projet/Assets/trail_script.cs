using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trail_script : MonoBehaviour
{
    public Pattern Ennemi;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Ennemi != null){
            if(Ennemi.Vitesse > 100){
                GetComponent<TrailRenderer>().enabled = true;
            } else {
                GetComponent<TrailRenderer>().enabled = false;
            }
        } 
    }
}
