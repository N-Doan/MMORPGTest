using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    public void onPlayPress()
    {
        SceneManager.LoadScene(1);
    }
    public void onExitPress()
    {
        Application.Quit();
    }
}
