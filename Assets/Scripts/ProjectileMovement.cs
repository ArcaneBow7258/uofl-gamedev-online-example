using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileMovement : NetworkBehaviour
{
    public float speed = 100;
    public float lifeTime = 1;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        if(lifeTime < 0){
            // need  to remove from all instances
            FPSGameManager.Instance.DespawnGameObjectServerRPC(gameObject);

        }else{
            lifeTime -= Time.deltaTime;
        }
    }
}
