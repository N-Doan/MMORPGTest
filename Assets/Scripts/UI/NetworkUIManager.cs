using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] 
    private Button serverButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private Button hostButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() => onServer());
        clientButton.onClick.AddListener(() => onClient());
        hostButton.onClick.AddListener(() => onHost());
    }
    private void onServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    private void onClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    private void onHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
