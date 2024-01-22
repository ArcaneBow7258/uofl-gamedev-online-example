using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // for input field
public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // ensures you only have 1 manager
    public GameObject ui_hud;
    public TMP_InputField input_joinCode;
    public TMP_Text display_joinCode;
    public void JoinButton(){ // Wrapper to join relay with input
        RelayManager.Instance.JoinRelay(input_joinCode.text);
        display_joinCode.text = input_joinCode.text; // should probalby move this out so you don't display a wrong code but i'm lazy
    }
    public async void HostButton(){ // you don't need to do this, you can just have button reference RelayManager and  use the CreateRelay() directly.
        await RelayManager.Instance.CreateRelay();
        display_joinCode.text = RelayManager.Instance.joinCode ;
    }
    void Awake(){
        if(Instance != null){
            Debug.Log("You have two UI managers");
        }
        else{
            Instance = this;
        }
       
    }
    void Start(){
        // When we join/create a relate, we disable this UI
        RelayManager.Instance.e_relayDone.AddListener(delegate{ ui_hud.SetActive(false); });
    }
    
}
