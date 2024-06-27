using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_parameter : MonoBehaviour
{
    public Rigidbody MyRb;

    void OnTriggerEnter(Collider others)
    {
        if(others.gameObject.tag == "Avion"){
            Object.Destroy(this.gameObject);
        }
        if(others.gameObject.tag == "Player"){
            Object.Destroy(this.gameObject);
        }
    }

    private void Awake()
    {
        StartCoroutine(waiter());
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(10);
        Object.Destroy(this.gameObject);
    }
}
