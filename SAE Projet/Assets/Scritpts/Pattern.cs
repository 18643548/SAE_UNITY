using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern : MonoBehaviour
{
    public float ThrottleIncrement = 1f;
    public float MaxThrust = 3000f;
    public float responsivness = 5f;
    public float lift = 600f;
    public float RotateSpeed = 0f;
    private float ThrottleTarget = 0; //throttle visée

    public Vector3 CurrentEulerAngles;
    public Vector3 Player_Direction;

    private float roll;     //Tilting left to right.
    private float pitch;    //Tilting front to right.
    private float yaw;      //Pivoting left to right.

    private float timer1 = 0;// Pour conter le temps pour l'accélération
    private float airDensity = 0; // densitée de l'air
    private float airDrag; //trainée en fonction de la densité de l'air

    private float Inertie;
    private float equilibrium = 0; // valeur actuelle de la force rencetrant l'avion
    public float equilibre = 200; // force qui permet à l'avion de se recentrer tout seul

    private bool patrol_flag = false;

    public NewBehaviourScript Player;
    public ColliderHP HP;
    public Fire_Zone Fire;

    public CheckForPlayer_Back CheckBack;
    public CheckForPlayer_Front CheckFront;
    public CheckForPlayer_Right CheckRight;
    public CheckForPlayer_Left CheckLeft;

    public bool PlayerOnFront = false;
    public bool PlayerOnBack = false;
    public bool PlayerOnLeft = false;
    public bool PlayerOnRight = false;

    public bool ActionFlag = false;
    public bool Live = true;
    public int HitPoints = 20;


    public Vector3 Player_Position;

    private Quaternion Target_Position;

    public Quaternion TargetRotation;

    bool Decollage_flag = false;

    [SerializeField] Transform Target_OBJ;

    enum Etat_joueur {Vol_Idle, Decollage, Patrol, Go_to, Locked, Chase, HighTurn, Dead};

    public static bool is_fire;

    float X_max, X_Min, Y_Max, Y_Min, Z_Max, Z_Min; 
    bool X_ok = false;
    bool Y_ok = false;
    bool Z_ok = false;

    public double Vitesse;

    public Vector3 Patrol_Point;

    public bool Target_Acquire = false;

    Etat_joueur state;

    //Variables de la gestion de la poussee
    private float throttle = 0;       //Percentage of maximum engine thrust currently being used
    private float throttleTarget = 0; //Acceleration visee

    private void Awake(){
        state = Etat_joueur.Decollage;
        rb = GetComponent<Rigidbody>(); 
        Transform transAvion = rb.transform;// used to get the roll angle of the plane  
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
        if (acc) {
            throttleTarget += ThrottleIncrement;
        } else if (dec) {
            throttleTarget -= ThrottleIncrement; 
        }
        
        throttleTarget = Mathf.Clamp(throttleTarget, 0f, 100f);//Limitation de la valeur de l'acc�l�ration de 0 % � 100%

        //Variation de l'acc�l�ration
        if(throttle < throttleTarget){
            throttle += ThrottleIncrement;
        } else if(throttle > throttleTarget){
            throttle -= ThrottleIncrement;
        }

        
        Transform transAvion = rb.transform;// used to get the roll angle of the plane  
        //handle variations of the roll of the plane to auto center it (that's the purpose of the tail)
        if (transAvion.eulerAngles.z < 180){ //check the roll angle using eulerAngles function
            equilibrium = transAvion.eulerAngles.z * -equilibre; 
        } else { 
            equilibrium = (360 -  transAvion.eulerAngles.z) * equilibre;
        }
    }

    private void FixedUpdate()
    {
        Transform transAvion = rb.transform;// used to get the roll angle of the plane  

        rb.AddTorque(responseModifier * yaw * transform.up * rb.velocity.magnitude * airDensity);   // Yaw
        rb.AddTorque(responseModifier * pitch * transform.right * rb.velocity.magnitude * airDensity);  // Pitch
        rb.AddTorque(responseModifier * 1.2f * roll * transform.forward * rb.velocity.magnitude * airDensity);  //Roll

        rb.AddForce( (Inertie + MaxThrust * throttle )* transform.forward ) ; // Poussée
        //rb.AddForce(-transform.forward * rb.velocity.magnitude * airDrag); //trainée

        Inertie = rb.velocity.magnitude;

        rb.AddForce( lift * airDensity * rb.velocity.magnitude * Vector3.up); // Portance

        rb.AddTorque(equilibrium * transform.forward * airDensity);  // l'avion se centre tout seul horizontalement (counter roll)


        Player_Position = NewBehaviourScript.player_position;

        Vitesse = Inertie * 3.6;
    }

    void Lever(float _roll, float _pitch, float _yaw)
    {
        roll = _roll;
        pitch = _pitch;
        yaw = _yaw;

    }

    private IEnumerator WAITSECOND(float attente)
    {
        yield return new WaitForSeconds(attente);
    }

    void State_switch(){
        switch(state){
            case Etat_joueur.Decollage:
                print("Décollage");
                StartCoroutine(State_decollage());
                //StartCoroutine(State_HighTurn(true));
                
                break;
            case Etat_joueur.Vol_Idle:
                print("Idle");
                StartCoroutine(State_vol_idle());
                break;
            case Etat_joueur.Patrol:
                print("Patrol");
                StartCoroutine(State_Patrol());
                break;
            case Etat_joueur.Go_to:
                print("Goto");
                StartCoroutine(State_Go_To());
                break;
            case Etat_joueur.Locked:
                print("Locked");
                StartCoroutine(State_AutoLock());
                break;
            case Etat_joueur.Dead:
                break;
        }
    }

    IEnumerator State_decollage(){
        HandleInputs(true,false);
        print("début décollage");
        yield return new WaitForSeconds(5f);
        print("Fin décollage");
        Decollage_flag = true;
        state = Etat_joueur.Patrol;
    }

    IEnumerator State_vol_idle(){
        HandleInputs(true, false);
        yield return null;
    }

    IEnumerator State_Patrol(){
        if(!patrol_flag){
            patrol_flag = true;
            Patrol_Point = new Vector3(Random.Range(-1500,1500),Random.Range(0,2000),Random.Range(-1500,1500));
        
            X_max = Patrol_Point.x + 100; 
            X_Min = Patrol_Point.x - 100;

            Y_Max = Patrol_Point.y + 100;
            Y_Min = Patrol_Point.y - 100;

            Z_Max = Patrol_Point.z + 100;
            Z_Min = Patrol_Point.z - 100;
        }
        Target_Position = Quaternion.LookRotation(Patrol_Point - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, Target_Position, (RotateSpeed) * Time.deltaTime);
        StartCoroutine(WAITSECOND(1f));
        
        if(transform.position.x < X_max && transform.position.x >= X_Min){
            X_ok = true;
        } else {
            X_ok = false;
        }

        if(transform.position.y < Y_Max && transform.position.y >= Y_Min){
            Y_ok = true;
        } else {
            Y_ok = false;
        }

        if(transform.position.z < Z_Max && transform.position.z >= Z_Min){
            Z_ok = true;
        } else {
            Z_ok = false;
        }

        if(X_ok || Y_ok || Z_ok){
            patrol_flag = false;
            yield return null;
        }
    }

    IEnumerator State_Go_To(){
        TargetRotation = Quaternion.LookRotation(Player_Direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, (RotateSpeed) * Time.deltaTime);
        HandleInputs(true,false);
        yield return null;
    }

    IEnumerator State_AutoLock(){
        Vector3 Accuracy = new Vector3(Random.Range(-5,5),Random.Range(-5,5),Random.Range(-5,5));
        Accuracy += Player_Direction;

        TargetRotation = Quaternion.LookRotation(Accuracy);

        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, (RotateSpeed) * Time.deltaTime);
        if(Fire_Zone.isInBox && Target_Acquire){
            is_fire = true;
        } else {
            is_fire = false;
        }
        yield return null;
    }

    IEnumerator State_HighTurn(bool Droite){
        if(Droite){
            //transform.Rotate(90,0,0);
            if(!ActionFlag){
                Patrol_Point = new Vector3(transform.position.x + 100, transform.position.y, transform.position.z + 100);
                ActionFlag = true;
            } else {}
            
            Target_Position = Quaternion.LookRotation(Patrol_Point);
            transform.rotation = Quaternion.Slerp(transform.rotation, Target_Position, (RotateSpeed) * Time.deltaTime);
        } else {
            //transform.Rotate(-90,0,0);
        }
        print("HIGHTURN");
        yield return null;
    }


    //HighTurn, ImmelmannTurn, SplitS
    private void ThrottleManagement(){
         //////////////////////Gestion accélération////////////////////////
        if (throttle < ThrottleTarget) // acceleration
        {
            if (timer1 >= (Mathf.Clamp(throttle *throttle * 0.0001f, 0,100)  + 0.05f))
            { //lancement du timer
                throttle += ThrottleIncrement;
                timer1 = 0;
            }
            else timer1 += Time.deltaTime; // incrementation du timer1
        }

        else if (throttle > ThrottleTarget)// deceleration
        {
            if (timer1 >= 0.075f)
            { //lancement du timer
                throttle -= ThrottleIncrement;
                timer1 = 0;
            }
            else timer1 += Time.deltaTime; // incrementation du timer1
        };

        ////////////////////Gestion densité de l'air ///////////////////////
        airDensity = 1 - (1 / 3 * rb.transform.position.y * 0.00001f);  /// Gestion densité de l'air

        airDrag = rb.drag * airDensity; ///Gestion trainée en fonction de l'altitude
    }

    private void Update()
    {
        State_switch();
        ThrottleManagement();
        Live = HP.Alive;
        //print(Player_Direction.magnitude);
        if(Player_Position != null){
            if(Live){
                Player_Direction = Player_Position - rb.position;
                if(Decollage_flag){
                    if(Player_Direction.magnitude > 2000){ //En attente de trouver l'ennemi
                    state = Etat_joueur.Patrol;
                    HandleInputs(true,false);
                    print("> 2000");
                }
                if(Player_Direction.magnitude <= 2000 && Player_Direction.magnitude > 1000 && Decollage_flag){ //Ennemi repéré, rapprochement en cours 
                    print("< 2000");
                    HandleInputs(true,false);
                    if(Target_Acquire){
                            state = Etat_joueur.Go_to;
                        } else {
                            Target_Acquire = true;
                        }
                    }

                
                if(Player_Direction.magnitude <= 1000 && Player_Direction.magnitude > 500 && Target_Acquire){ //Ennemi proche
                    print("Chase");
                    HandleInputs(true,false);
                    state = Etat_joueur.Chase;
                }
                if(Player_Direction.magnitude <= 500 && Target_Acquire){ //Ennemi locked
                    print("auto_lock");
                    state = Etat_joueur.Locked;
                    if(Player_Direction.magnitude <= 200){
                        ThrottleTarget -= ThrottleIncrement;
                    } else {
                        ThrottleTarget += ThrottleIncrement;
                    }
                    float Chance_to_unlock = Random.Range(0,1);
                    if(Chance_to_unlock > 0.5f){
                            Target_Acquire = false;
                            print("Target Unlock");
                            state = Etat_joueur.Patrol;
                            //StartCoroutine(WAITSECOND(5f));
                    }
                }
            }
            
        } else {
            state = Etat_joueur.Dead;
            HandleInputs(false,true);
            Lever(0,-1000,0);
        }
        }  
            
        double Rotate_calculus = (101 - throttle)/100 + 0.5;
        RotateSpeed = (float) Rotate_calculus;
        PropelletSpin.Rotate(Vector3.up * (throttle)*100);
    }


    void OnCollisionEnter(Collision collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "ball(Clone)")
        {
            HitPoints -= 1;
            print("Ca touche ennemi");
        }

        if(HitPoints <= 0)
        {
            print("ENNEMI MORT");
            //Object.Destroy(this.gameObject);
        }
    }

}
