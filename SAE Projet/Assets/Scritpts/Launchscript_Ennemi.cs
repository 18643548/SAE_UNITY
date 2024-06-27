using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launchscript_Ennemi : MonoBehaviour
{
    public GameObject projectile;
    public Pattern Fire;

    public AudioSource Fireshot;
    public float BulletVel = 1f;

    int time = 0;
    int delay = 40;
    bool IsFire = false;

    // Update is called once per frame
    void Update()
    {
        if(Pattern.is_fire)
        {
            if(time == delay){
                time = 0;
                GameObject Lancement = Instantiate(projectile, transform.position, transform.rotation);
                Lancement.GetComponent<Rigidbody>().AddRelativeForce(new Vector3 (0,BulletVel,0));
                
            } else {
                time++;
            } 
            if(Fireshot.time > 0.38f || !Fireshot.isPlaying){
                Fireshot.Play();
            }
        } 
        
        
        
    }
}
