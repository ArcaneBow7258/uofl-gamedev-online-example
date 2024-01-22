using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Relay; // for relayService
using UnityEngine.Events; //For Unity events
using System.Threading.Tasks; //for Service
using Unity.Services.Relay.Models; // for allocation
using Unity.Networking.Transport.Relay; //RelayServerData
using Unity.Netcode.Transports.UTP; // for the transport
using Unity.Netcode; // To get NetworkManager Instance
using Unity.Services.Core; // get Initialization for Unity Cloud Services
using Unity.Services.Authentication;
using System; // for authenticating to Unity Cloud Services
public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance; // ensures you only have 1 manager
    public bool connected = false; 
    public UnityEvent e_relayDone; //if you want triggers for when you connect
    
    public string joinCode;
    private String playerId; 
    async void Awake(){
        if(Instance != null){
            Debug.Log("You have two relay managers");
        }
        else{
            Instance = this;
        }
        e_relayDone.AddListener(delegate{connected = !connected;});
        try{
            // Need to connect to Unity Services to use their relay.
            await UnityServices.InitializeAsync();
            // Sign into unity services to use relay
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;
        }
        catch(RelayServiceException e){
            Debug.Log(e);
        }
    }
    public async Task<string> CreateRelay(int playerNumber = 4){
        try{
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(playerNumber); // Create Relay Connection
            joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId); // get joincode for allocation we made
            // joinCode is the code needed to Join a session, you can either use this instance to get it
            // Ideally, you'll display this somewhere or find away to send it to client.
            Debug.Log("Relay created " + joinCode);

            //Idk how this works I followed a tutorial
            RelayServerData serverData = new RelayServerData(alloc, "dtls");
            
            //Connect our Transport to Relay Server
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            //Same as start host button using debugger.
            NetworkManager.Singleton.StartHost();
            //Event trigger for on join (like removing a HUD)
            e_relayDone.Invoke();
            return joinCode;
        }
        catch(RelayServiceException e){
            Debug.Log(e);
            return null;
        }
    }
    public async void JoinRelay(string joinCode){
        try{
            // Join Service
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //Connect our Transport to Relay Server
            RelayServerData serverData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
             //Same as start client button in debugger.
            NetworkManager.Singleton.StartClient();
            e_relayDone.Invoke();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }
    }
}
