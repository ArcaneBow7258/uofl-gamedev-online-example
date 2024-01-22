using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerMovement : NetworkBehaviour
{
    public Camera cam;
    public float moveSpeed = 1;
    public int bullet;
    private Vector3 realRotation;
    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        if(!IsOwner){
            cam.enabled = false;
        }else{
            Debug.Log("I am the owner");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure you only control your player character.
        // Note: Need ClientNetworkTransform to perform translation. Typically Only done server-side.
        if(!IsOwner) return;
        //print("Running");   
        Vector3 move = new Vector3(0,0,0);
        if(Input.GetKey(KeyCode.W)){move.z += 1;}
        if(Input.GetKey(KeyCode.S)){move.z -= 1;}
        if(Input.GetKey(KeyCode.A)){move.x -= 1;}
        if(Input.GetKey(KeyCode.D)){move.x += 1;}
        transform.Translate(moveSpeed * Time.deltaTime *move, Space.Self);
        float xMovement = Input.GetAxisRaw("Mouse X") ;
        float yMovement = -Input.GetAxisRaw("Mouse Y") ;

        realRotation   = new Vector3(Mathf.Clamp(realRotation.x + yMovement, -90, 90), realRotation.y + xMovement, realRotation.z);
        realRotation.z = Mathf.Lerp(realRotation.z, 0f, Time.deltaTime * 3f);

        transform.eulerAngles = Vector3.Scale(realRotation, new Vector3(0f, 1f, 0f));

        cam.transform.eulerAngles =  realRotation;

        if(Input.GetKeyDown(KeyCode.R)){
            print("pew pew");
            // can't instanitate / Spwan from Client Side, need server help
            GameManager.instance.SpawnGameObjectServerRpc(bullet, transform.position, transform.rotation);
        }

    }   
}
