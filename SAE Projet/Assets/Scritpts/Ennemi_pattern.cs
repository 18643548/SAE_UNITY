using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ennemi_pattern : MonoBehaviour
{
    public float ThrottleIncrement = 0.1f;
    public float MaxThrust = 5000f;
    public float responsivness = 25f;
    public float lift = 150f;

    private float roll;     //Tilting left to right.
    private float pitch;    //Tilting front to right.
    private float yaw;      //Pivoting left to right.

    private bool State_finish = false;

    enum Etat_joueur {Vol_Idle, Decollage, Rotation};

    Etat_joueur state;

    //Variables de la gestion de la poussee
    private float throttle = 0;       //Percentage of maximum engine thrust currently being used
    private float throttleTarget = 0; //Acceleration visee

    private void Awake(){
        state = Etat_joueur.Decollage;
        rb = GetComponent<Rigidbody>();
    }

    float responseTime(float throttle)   //Temps de r�ponse pour les acc�l�rations
    {
        float time1 = Mathf.Pow((throttle * 1f), 3); 
        return time1;
    }

    private float responseModifier
    {//Value used to tweak responsivness to the plane mass.
        get
        {
            return (rb.mass / 10f) * responsivness;
        }
    }

    Rigidbody rb;
    [SerializeField] Transform PropelletSpin;

    private void HandleInputs(bool acc, bool dec)
    {
        //hande throttle value being sure to clamp it between 0 and 100
        if (acc) // acc�l�ration
        {
            throttleTarget += ThrottleIncrement;
        }

        else if (dec) //d�c�l�ration
        {
            throttleTarget -= ThrottleIncrement; 
        };
        
        throttleTarget = Mathf.Clamp(throttleTarget, 0f, 100f);//Limitation de la valeur de l'acc�l�ration de 0 % � 100%

        //Variation de l'acc�l�ration
        if (throttle < throttleTarget)      //acc�l�ration
        {
            WAITSECOND(responseTime(throttle));
            throttle += ThrottleIncrement;
        }
        else if (throttle > throttleTarget) //d�c�l�ration
        {
            WAITSECOND(0.05f);
            throttle -= ThrottleIncrement;
        }
    }

    private void FixedUpdate()
    {
        //Apply forces to the plane
        rb.AddTorque(responseModifier * yaw * transform.up * rb.velocity.magnitude);      //Controle droite gauche
        rb.AddTorque(responseModifier * pitch * transform.right * rb.velocity.magnitude); //Controle haut bas
        rb.AddTorque(responseModifier * roll * transform.forward * rb.velocity.magnitude);//Controle roulis 

        //Force de pouss�e
        
        rb.AddForce(MaxThrust * throttle * transform.forward); //Avancer

        //Force de Port�e
        rb.AddForce(lift * rb.velocity.magnitude * Vector3.up);//Port�e

    }

    void Lever(float _roll, float _pitch, float _yaw)
    {
        roll = _roll;
        pitch = _pitch;
        yaw = _yaw;

    }

    private IEnumerator WAITSECOND(float attente)
    {
        yield return new WaitForSecondsRealtime(attente);
        Debug.Log("La pause de " + attente + " secondes est terminée.");
    }

    void State_switch(){
        switch(state){
            case Etat_joueur.Decollage:
                StartCoroutine(State_decollage());
                if(State_finish)
                {
                    state = Etat_joueur.Vol_Idle;
                    State_finish = false;
                }          
                break;
            case Etat_joueur.Vol_Idle:
                StartCoroutine(State_vol_idle());
                if(State_finish)
                {
                    state = Etat_joueur.Rotation;
                    State_finish = false;
                }
                break;
            case Etat_joueur.Rotation:
                StartCoroutine(State_Rotation());
                break;
        }
    }

    IEnumerator State_decollage(){
        print("enter State_decollage");
        HandleInputs(true,false);
        yield return StartCoroutine(WAITSECOND(3f));
        Lever(0,-0.5f,0);
        yield return StartCoroutine(WAITSECOND(0.0005f));
        Lever(0,0.3f,0);
        yield return StartCoroutine(WAITSECOND(0.0005f));
        Lever(0,0,0);
        State_finish = true;
        print("exit State_decollage");
    }

    IEnumerator State_vol_idle(){ //vol de croisiere
        print("enter State_vol_idle");
        HandleInputs(true,false);
        Lever(0,0,0);
        yield return StartCoroutine(WAITSECOND(5f));
        State_finish = true;
        print("exit State_vol_idle");
    }

    IEnumerator State_Rotation(){
        HandleInputs(false,true);
        yield return StartCoroutine(WAITSECOND(0.005f));
        HandleInputs(false,false);
        Lever(1,0,0);
        yield return StartCoroutine(WAITSECOND(2f));
        Lever(0,-1,0);
        yield return StartCoroutine(WAITSECOND(10f));
        state = Etat_joueur.Vol_Idle;
    }

    private void Update()
    {
        State_switch();

        PropelletSpin.Rotate(Vector3.up * (throttle * 0.01f));
    }


}