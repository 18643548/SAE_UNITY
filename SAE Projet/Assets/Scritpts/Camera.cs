using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    //[Header("Camera setup")]
    [Tooltip("An array of transform representing camera positions")]
    [SerializeField] Transform[] povs;
    [Tooltip("the speed at which the camera follows the plane")]
    [SerializeField] float camSpeed;

    private int index = 1;
    private Vector3 target;
    public Vector3 Player_Rotation;

    public NewBehaviourScript Player;




    private void Update()
    {
        // Numbers 0-2 represent different povs
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;

        //Set the camera to the relevant pov.
        target = povs[index].position;

        Player_Rotation = NewBehaviourScript.Player_rotation;
    }

    private void FixedUpdate()
    {
        // Move camera to the disered position; Must be in Fixedupdate to avoid camera jitter.
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * camSpeed);
        Player_Rotation = NewBehaviourScript.Player_rotation;
        //Quaternion CamRotation = Quaternion.Slerp(Player_Rotation, Quaternion.LookRotation(), Time.deltaTime * camSpeed);
        //transform.AddTorque = new Vector3(Player_Rotation);
        transform.forward = povs[index].forward;
    }

}