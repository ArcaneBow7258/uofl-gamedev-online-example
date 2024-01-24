using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public NetworkVariable<float> maxHealth= new NetworkVariable<float>(1);
    public NetworkVariable<float> health = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;
        maxHealth.Value = health.Value;
        
    }
    public virtual void Update(){
    
        
        //Clamping    
        health.Value = Mathf.Min(health.Value, maxHealth.Value);
    }
    // Client RPC to send damage to all clients and update it.
    [ClientRpc]
    public void TakeDamageClientRpc(float damage, ulong attackId, ClientRpcParams clientRpcParams = default){
        if(!IsOwner) return;
        if(damage > 0){
            health.Value -= damage;
        }
        if(health.Value <= 0){
            // Need to attach script to gameobject you are transforming with spawn point
            FPSGameManager.Instance.GiveSpawnPoint(transform);
            //FPSGameManager.Instance.DespawnGameObjectServerRPC(gameObject);
            //Destroy(gameObject);
            FPSGameManager.Instance.AddPointsServerRpc(1, attackId);
            health.Value = maxHealth.Value;
        }
    }
}
