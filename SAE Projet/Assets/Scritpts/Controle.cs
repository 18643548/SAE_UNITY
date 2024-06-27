using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{

    public AudioSource NightWitches;
    public AudioSource NoBullet; 
    public AudioSource RedBaron;
    public AudioSource HellAndBack;
    public AudioSource Spitfire;
    public AudioSource Wind;

    public Pattern Ennemi;

    public AudioSource Engine;
    public AudioSource Proppeler;

    public int Current_Audio = 0;
    public int Next_Audio = 0;

    private bool Pause = false;

    public double Vitesse = 0;



    

    [Header("Plane Stats")]
    
    [Tooltip("How much the throttle can variate.")]
    public float ThrottleIncrement = 1f;
    [Tooltip("Maximum speed when at 100% throttle.")]
    public float Vmax = 166.7f;
    [Tooltip("How responsive the plane is when rolling, pitching...")]
    public float responsivness = 5;
    [Tooltip("The lift coefficient")]
    public float lift = 600;
    [Tooltip("La valeur de la force qui recentre l'avion")]
    public float equilibre = 200; // force qui permet à l'avion de se recentrer tout seul
    [Tooltip("L'accélération actuelle de l'avion, mettre à la valuer désiré lors d'un spawn en l'air")]
    public float throttle = 0; //Percentage of maximum engine thrust currently being used

    private float roll; //Tilting left to right.
    private float pitch; //Tilting front to right.
    private float yaw; //Pivoting left to right.
    private float equilibrium = 0; // valeur actuelle de la force rencetrant l'avion
    private float ThrottleTarget = 0; //throttle visée
    private float timer1 = 0;// Pour conter le temps pour l'accélération

    bool ButtonFlag = false;

    public float Speed;//vitesse de l'avion en km/h 
    private bool thrFlag = false;

    public static Vector3 player_position; //position de l'avion du joueur 
    public static Vector3 Player_rotation;
    public int HitPoints = 20;
    private float responseModifier
    {//Value used to tweak responsivness to the plane mass.
        get
        {
            return (rb.mass / 10f) * responsivness;
        }
    }

    Rigidbody rb;
    [SerializeField] TextMeshProUGUI hud;
    [SerializeField] TextMeshProUGUI WinLoseHud;
    [SerializeField] Transform PropelletSpin;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        NightWitches.Play();
    }

        private void HandleInputs()
    {   

        //Set rotational values from our axis input.
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        //hande throttle value being sure to clamp it between 0 and 100

        if (throttle >49) thrFlag = true;//mise en effet de la limitation de controle au dessus de 79%

        //handle special throttle controle once in the air.
        if (thrFlag )//une fois au dessus de 50% de l'accélération, on ne peut plus revenir en dessous.
        {
            ThrottleIncrement = 0.5f;
            if (Input.GetKey(KeyCode.LeftShift)) ThrottleTarget += ThrottleIncrement;
            else if (Input.GetKey(KeyCode.LeftControl)) ThrottleTarget -= ThrottleIncrement;
            ThrottleTarget = Mathf.Clamp(ThrottleTarget, 50f, 100f);
        }
        else //pour un éventuel décolage.
        { 
            if (Input.GetKey(KeyCode.LeftShift)) ThrottleTarget += ThrottleIncrement;
            else if (Input.GetKey(KeyCode.LeftControl)) ThrottleTarget -= ThrottleIncrement;
            ThrottleTarget = Mathf.Clamp(ThrottleTarget, 0f, 100f);
        }
        //handle variations of the roll of the plane to auto center it  

        Transform transAvion = rb.transform;
        if ( (transAvion.eulerAngles.z < 180) ) 
             { equilibrium = (transAvion.eulerAngles.z * -equilibre); }
        else {  equilibrium = ((360 -  transAvion.eulerAngles.z) * equilibre); }

        //physiques
        Speed = Vmax * throttle/100 - Mathf.Sin(transAvion.eulerAngles.y) * 9.81f;
        
    }

    private void Update()
    {
        HandleInputs();
        UpdateHUD();
        AudioInput();
        engine_sound();

        PropelletSpin.Rotate(Vector3.up * throttle * 0.5f);
        player_position = rb.position;

        //Player_rotation = rb.rotation;

        //////////////////////Gestion accélération////////////////////////
        if (throttle < ThrottleTarget) // acceleration
        {
            if (thrFlag)
            {
                if (timer1 >= 0.25f)
                { //lancement du timer
                    throttle += ThrottleIncrement;
                    timer1 = 0;
                }
                else timer1 += Time.deltaTime; // incrementation du timer1
            }
            else
            {
                if (timer1 >= 0.125f)
                { //lancement du timer
                    throttle += ThrottleIncrement;
                    timer1 = 0;
                }
                else timer1 += Time.deltaTime; // incrementation du timer1
            }
            
        }

        else if (throttle > ThrottleTarget)// deceleration
        {
            if (timer1 >= 0.075f)
            { //lancement du timer
                throttle -= ThrottleIncrement;
                timer1 = 0;
            }
            else timer1 += Time.deltaTime; // incrementation du timer1
        }
    }

    private void FixedUpdate()
    {
        if(!Ennemi.Live){ 
            WinLoseHud.text = "Gagné";
        } else {
            //Apply forces to the plane
            Transform transAvion = rb.transform;// used to get the roll angle of the plane

            rb.AddTorque(responseModifier * yaw * transform.up * rb.velocity.magnitude);
            rb.AddTorque(responseModifier * pitch * transform.right * rb.velocity.magnitude * 0.5f);
            rb.AddTorque(responseModifier * roll * transform.forward * rb.velocity.magnitude);

            transform.Translate(0,0,(Speed * Time.deltaTime));

            rb.AddForce(lift * rb.velocity.magnitude * Vector3.up);

            rb.AddTorque(equilibrium * transform.forward);//l'avion se centre tout seul horizontalemen
        }
        
    }
    
    private void UpdateHUD()
    {
        hud.text = "Throttle " + throttle.ToString("F0") + "%\n";
        hud.text += "Air speed " + (Speed * 3.6f).ToString("F0") + "km/h\n";
        hud.text += "Altitude " + transform.position.y.ToString("F0") + "m";

    }

    void OnCollisionEnter(Collision collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "ball(Clone)")
        {
            HitPoints -= 1;
            print("Ca touche");
        }

        if(HitPoints == 0)
        {
            print("Joueur Perdu");
            WinLoseHud.text = "Perdu";
            //Object.Destroy(this.gameObject);
        }
    }

    void AudioInput(){
         if(!ButtonFlag){
            if(Input.GetAxis("PlayMusic") == 1){
                Pause = !Pause;
                Audio_Pause(Current_Audio,Pause);
                ButtonFlag = true;
            }
            if(Input.GetAxis("GoNextMusic") == 1){
                Next_Audio += 1;
                if(Next_Audio > 4){
                    Next_Audio = 0;
                }
                ButtonFlag = true;
            }
            if(Input.GetAxis("GoBackMusic") == 1){
                Next_Audio -= 1;
                if(Next_Audio < 0){
                    Next_Audio = 4;
                }
                ButtonFlag = true;
            }
        }
        
        if(Input.GetAxis("GoBackMusic") == 0 && Input.GetAxis("GoNextMusic") == 0 && Input.GetAxis("PlayMusic") == 0){
            ButtonFlag = false;
        }

        if(!NightWitches.isPlaying && !RedBaron.isPlaying && !NoBullet.isPlaying && !HellAndBack.isPlaying && !Pause && !Spitfire.isPlaying){
            Next_Audio++;
        }

        if(Next_Audio != Current_Audio){
            AudioPlay(Next_Audio);
            Current_Audio = Next_Audio;
        } 


    }

    void AudioPlay(int index){
        NightWitches.Stop();
        RedBaron.Stop();
        HellAndBack.Stop();
        NoBullet.Stop();
        Spitfire.Stop();

        if(index == 0){
            NightWitches.Play();
        } else if(index == 1){
            RedBaron.Play();
        } else if(index == 2){
            HellAndBack.Play();
        } else if(index == 3){
            NoBullet.Play();
        } else {
            Spitfire.Play();
        }
    }

    void Audio_Pause(int index, bool Pause){
        if(index == 0){
            if(Pause){
                NightWitches.Pause();
            } else {
                NightWitches.UnPause();
            }
        }
        if(index == 1){
            if(Pause){
                RedBaron.Pause();
            } else {
                RedBaron.UnPause();
            }
        }
        if(index == 2){
            if(Pause){
                HellAndBack.Pause();
            } else {
                HellAndBack.UnPause();
            }
        }
        if(index == 3){
            if(Pause){
                NoBullet.Pause();
            } else {
                NoBullet.UnPause();
            }
        }
        if(index == 4){
            if(Pause){
                Spitfire.Pause();
            } else {
                Spitfire.UnPause();
            }
        }
    }

    void engine_sound(){
        Proppeler.pitch = (float) Speed/600 + throttle/10000;
        Proppeler.volume = 0.1f;
    }
}
