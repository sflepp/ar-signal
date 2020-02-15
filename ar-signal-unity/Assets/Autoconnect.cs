using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Autoconnect : MonoBehaviour
{
    private NetworkManager manager;
    
    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<NetworkManager>();
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
           manager.StartHost();
        }
    }
}
