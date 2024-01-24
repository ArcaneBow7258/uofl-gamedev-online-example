using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Linq;
using Unity.VisualScripting;
public class FPSGameManager : NetworkBehaviour
{
    // Network Variables are values that are updated on each side
    public static FPSGameManager Instance;
    public NetworkList<ulong> playerList;// = new NetworkList<ulong>(); https://forum.unity.com/threads/a-native-collection-has-not-been-disposed-resulting-in-a-memory-leak.1199416/
    public NetworkList<int> points;// = new NetworkList<int>();
    public NetworkVariable<int> testVar = new NetworkVariable<int>();
    public TMP_Text display;
    public TMP_Text display_test;
    public TMP_Text playerName;
    /* Some list of network prefabs to reference 
    / You can also explicitly define it such as
    / public GameObject gameObject 
    / public NetworkObject networkObject
    / public List<GameObject> gameObject
    / 
    / We use a list since we're using a manager, but if you had for example a turret you are handling on Server-side, you can just save the reference to a single bullet there.
    */
    public NetworkPrefabsList networkPrefabsList;
    public Transform spawnPoints;
    void Awake(){
        if(Instance != null){
            Debug.Log("You have two Game managers");
        }
        else{
            Instance = this;
            playerList = new NetworkList<ulong>();
            points = new NetworkList<int>();
            foreach(var item in networkPrefabsList.PrefabList){
                Debug.Log(item.Prefab.name);
                Debug.Log(item.Prefab.name.GetHashCode());
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        // Update Text
        Dictionary<ulong, int> sorting = new Dictionary<ulong, int>();
        for(int i = 0; i < playerList.Count; i++){
            sorting.Add(playerList[i],points[i]);
        }
        var sortedDict = from entry in sorting orderby entry.Value ascending select entry;
        String text = "Scoreboard\n";
        foreach(var row in sortedDict){
            
            text += String.Format("{0} : {1}\n", row.Key, row.Value);
        }
        display.text = text;
        display_test.text = testVar.Value.ToString();
       
    }
    public void GiveSpawnPoint(Transform go){
        Transform spawnpoint = spawnPoints.GetChild(UnityEngine.Random.Range(0,spawnPoints.childCount - 1) );
        go.position = spawnpoint.position;
        go.rotation = spawnpoint.rotation;
        print(spawnpoint.name);
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreboardServerRpc(ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        playerName.text = "ClientId: " + clientId.ToString();
        playerList.Add(clientId);
        points.Add(0);
        
    }
    [ServerRpc(RequireOwnership = false)]
    public void RemoveScoreboardServerRpc(ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        int index = playerList.IndexOf(clientId);
        playerList.RemoveAt(index);
        points.RemoveAt(index);
    }
    // Since Network variables arer server-side, we need an RPC from client.
    [ServerRpc(RequireOwnership = false)]
    public void AddPointsServerRpc(int add, ulong attacker, ServerRpcParams serverRpcParams = default){
        
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {   
            points[playerList.IndexOf(attacker)] += add;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void TestVariabledServerRpc(int add, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {   
            testVar.Value += add;
        }
    }

    #region  Spawning
    //Spawning and Despawning are only allowed Server-side as well
    [ServerRpc(RequireOwnership = false)]
    public void SpawnGameObjectServerRpc(int hashCode, Vector3 pos, Quaternion quat, bool own = false, ServerRpcParams serverRpcParams = default){
        
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {   
            try{
                var test = networkPrefabsList.PrefabList.First(item => item.Prefab.name.GetHashCode() == hashCode); // you could also refer to it by name instead
                GameObject go = Instantiate(test.Prefab, pos, quat);
                NetworkObject no = go.GetComponent<NetworkObject>();
                no.Spawn();
                if(own) no.ChangeOwnership(clientId); // if we want to give possession to summoner
            }catch(Exception e){ 
                // Send to specific client only per unity's documentation
                ClientRpcParams sendParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[]{clientId}
                    }
                };
                RequestErrorClientRPC(clientId, e.ToString(), sendParams);
            }
        }
    }
    //Spawning and Despawning are only allowed Server-side as well
    [ServerRpc(RequireOwnership = false)]
    public void SpawnGameObjectServerRpc(int hashCode, Vector3 pos, Vector3 direction, bool own = false, ServerRpcParams serverRpcParams = default){
        
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {   
            try{
                var test = networkPrefabsList.PrefabList.First(item => item.Prefab.name.GetHashCode() == hashCode); // you could also refer to it by name instead
                GameObject go = Instantiate(test.Prefab, pos, Quaternion.identity);
                go.transform.rotation = Quaternion.LookRotation(direction);
                NetworkObject no = go.GetComponent<NetworkObject>();
                no.Spawn();
                if(own) no.ChangeOwnership(clientId); // if we want to give possession to summoner
            }catch(Exception e){
                // Send to specific client only per unity's documentation
                ClientRpcParams sendParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[]{clientId}
                    }
                };
                RequestErrorClientRPC(clientId, e.ToString(), sendParams);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DespawnGameObjectServerRPC(NetworkObjectReference gameObject, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        gameObject.TryGet(out NetworkObject no);
        no.Despawn();
        Destroy(no.gameObject);
    }
    #endregion
    [ClientRpc()]
    public void RequestErrorClientRPC(ulong clientID, string error, ClientRpcParams clientRpcParams = default){
        Debug.Log(error);
    }
    public override void OnDestroy(){
        base.OnDestroy();
        playerList.Dispose();
        points.Dispose();
    }
}
