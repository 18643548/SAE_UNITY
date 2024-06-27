using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launchscript : MonoBehaviour
{
    public GameObject projectile;
    public AudioSource Fireshot;
    public float BulletVel = 150f;

    bool flag = false;

    int time = 0;
    int delay = 20;
    bool IsFire = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            print("Fire");
            if(time == delay){
                time = 0;
                GameObject Lancement = Instantiate(projectile, transform.position, transform.rotation);
                Lancement.GetComponent<Rigidbody>().AddRelativeForce(new Vector3 (0,BulletVel,0));
                
                //Fireshot.Play();
                
            } else {
                time++;
            } 

            if(Fireshot.time > 0.38f || !Fireshot.isPlaying){
                Fireshot.Play();
                
            }
        }
   
    }
}
