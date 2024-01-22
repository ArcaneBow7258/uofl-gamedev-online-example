using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using Unity.VisualScripting;
public class GameManager : NetworkBehaviour
{
    // Network Variables are values that are updated on each side
    public static GameManager instance;
    public NetworkVariable<float> points = new NetworkVariable<float>();
    public float local_points = 500;
    public TMP_Text display;
    /* Some list of network prefabs to reference 
    / You can also explicitly define it such as
    / public GameObject gameObject 
    / public NetworkObject networkObject
    / public List<GameObject> gameObject
    / 
    / We use a list since we're using a manager, but if you had for example a turret you are handling on Server-side, you can just save the reference to a single bullet there.
    */
    public NetworkPrefabsList networkPrefabsList;
    void Awake(){
        if(instance != null){
            Debug.Log("You have two Game managers");
        }
        else{
            instance = this;
        }
       
    }
    // Start is called before the first frame update
    void Start()
    {
        points.Value = 500; 
    }

    // Update is called once per frame
    void Update()
    {
        // Update Text
        display.text = String.Format("Server Points: {0} \n Local Points {1}", points.Value, local_points);
        
        if(Input.GetKeyDown(KeyCode.T)){// Increase Server Side
            AddPointsServerRpc(100);
        }
        if(Input.GetKeyDown(KeyCode.Y)){ // Increase Client Side
            local_points += 100;
        }
    }
    // Since Network variables arer server-side, we need an RPC from client.
    [ServerRpc(RequireOwnership = false)]
    public void AddPointsServerRpc(float add, ServerRpcParams serverRpcParams = default){
        
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {   
            points.Value += add;
        }
    }
    //Spawning and Despawning are only allowed Server-side as well
    [ServerRpc(RequireOwnership = false)]
    public void SpawnGameObjectServerRpc(int table_index, Vector3 pos, Quaternion quat, ServerRpcParams serverRpcParams = default){
        
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {    
            GameObject go = Instantiate(networkPrefabsList.PrefabList[table_index].Prefab, pos, quat);
            NetworkObject no = go.GetComponent<NetworkObject>();
            no.Spawn();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DespawnGameObjectServerRPC(NetworkObjectReference gameObject, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        gameObject.TryGet(out NetworkObject no);
        no.Despawn();
        Destroy(no.gameObject);
    }
    
}
