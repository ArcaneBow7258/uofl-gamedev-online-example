using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileCollision : NetworkBehaviour
{
    public float speed;
    public float lifeTime = 1;
    public float damage = 1;
    private Rigidbody rb;
    public bool oneTime = true;
    void Awake(){
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(lifeTime < 0){
            // need  to remove from all instances
            FPSGameManager.Instance.DespawnGameObjectServerRPC(gameObject);

        }else{
            lifeTime -= Time.deltaTime;
        }
    }
    void FixedUpdate(){
        if(oneTime){
            oneTime = false;
            rb.velocity = transform.forward * speed;
        }
    }   
    // Requires a Collider + Rigidbody on GameObject with this script.
    void OnCollisionEnter(Collision collision)
    {
        FPSGameManager.Instance.TestVariabledServerRpc(1);
        // try and see if we hit a player or orjbect with health.
        // you can also try checking the layermask you hit.
        Health h;
        if(collision.collider.TryGetComponent<Health>(out h)){
            h.TakeDamageClientRpc(damage, OwnerClientId);
        }
        FPSGameManager.Instance.DespawnGameObjectServerRPC(gameObject);
    }
}