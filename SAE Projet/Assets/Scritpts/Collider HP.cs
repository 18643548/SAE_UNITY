using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ColliderHP : MonoBehaviour
{
    public float HitPoint = 10f;

    public bool Alive = true;
    //Detect collisions between the GameObjects with Colliders attached
    void OnCollisionEnter(Collision collision){
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "ball(Clone)"){
            HitPoint -= 1;
            print("Ca touche");
        }

        if(HitPoint <= 0){
            //Object.Destroy(this.gameObject);
            print("PERDU");
            HitPoint = 0;
            Alive = false;
        }


    }
}
