using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class RigidBodyMovement : NetworkBehaviour
{
    [Header("objects")]
    public Camera cam;
    public Rigidbody rb;
    public GameObject bullet;
    public Transform shootpoint;
    [Header("Player Stats")]
    public float moveSpeed = 1;
    public float dashMulti = 2; // 
    public float dashCooldown = 3; // seconds
    private float dashCounter;
    public float cDrag = 2;
    public float dDrag = 1;
    private Vector3 realRotation;
    private Vector3 move = new Vector3(0,0,0);
    private bool dashed = false;
    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        if(!IsOwner){
            Destroy(cam.gameObject);
            cam.enabled = false;
        }else{
            Debug.Log("I am the owner");
            rb = GetComponent<Rigidbody>();
            FPSGameManager.Instance.GiveSpawnPoint(transform); // When playerprefab is made, designate a spawn point
            // Reason you need to move this into the OnClientStarted event is because of latency?
            // Something about ownership
            NetworkManager.OnClientStarted += delegate {//https://docs-multiplayer.unity3d.com/netcode/current/troubleshooting/errormessages/
            //https://forum.unity.com/threads/unity-netcode-spawnpoints.1446415/
                FPSGameManager.Instance.TestVariabledServerRpc(1);
                FPSGameManager.Instance.AddScoreboardServerRpc(); 
                };
        }
    }
    public void OetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
 
    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        dashCounter = dashCooldown;
        rb.drag = dDrag;
    }
    void Update(){
        if(!IsOwner) return;
        
        // Make sure you only control your player character.
        // Note: Need ClientNetworkTransform to perform translation. Typically Only done server-side.
        // Also needs Network Rigidbody
        //Camera Work
        float xMovement = Input.GetAxisRaw("Mouse X") ;
        float yMovement = -Input.GetAxisRaw("Mouse Y") ;

        realRotation   = new Vector3(Mathf.Clamp(realRotation.x + yMovement, -90, 90), realRotation.y + xMovement, realRotation.z);
        realRotation.z = Mathf.Lerp(realRotation.z, 0f, Time.deltaTime * 3f);

        transform.eulerAngles = Vector3.Scale(realRotation, new Vector3(0f, 1f, 0f));

        cam.transform.eulerAngles =  realRotation;

        if(Input.GetKey(KeyCode.W)){move.z = 1;}
        else if(Input.GetKey(KeyCode.S)){move.z = -1;}
        else{move.z = 0;}
        if(Input.GetKey(KeyCode.A)){move.x = -1;}
        else if(Input.GetKey(KeyCode.D)){move.x = 1;}
        else{move.x = 0;}
        if(Input.GetKey(KeyCode.LeftShift) & dashCounter >= dashCooldown){dashed = true; dashCounter = 0;}
        else if(dashCounter < dashCooldown){dashCounter+= Time.deltaTime;}

        if(Input.GetKey(KeyCode.C)){rb.drag = cDrag;} else {rb.drag = dDrag;}
        // Pew Pew
        if(Input.GetKeyDown(KeyCode.R)){
            // can't instanitate + Spwan from Client Side, need server help 2069864811
            FPSGameManager.Instance.SpawnGameObjectServerRpc(bullet.name.GetHashCode(), shootpoint.position, cam.ScreenPointToRay(Input.mousePosition).GetPoint(50) - shootpoint.position);
        }
    }
    // Physics Logic
    void FixedUpdate()
    {
        if(!IsOwner) return;
        //print("Running");   
        if(dashed){
            move *= dashMulti;
            dashed = false;
        }
        rb.AddRelativeForce(moveSpeed * Time.deltaTime *move);
        //transform.Translate(moveSpeed * Time.deltaTime *move, Space.Self);
        
        
        

    }   
}
