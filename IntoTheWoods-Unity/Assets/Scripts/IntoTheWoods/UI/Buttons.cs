using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Buttons : MonoBehaviour {
    public GameObject Haensel;
    public GameObject Gretel;
    
    public string targetScene;
    public void Play() {
        Debug.Log("Play");
        Haensel.gameObject.SetActive(true);
        Gretel.gameObject.SetActive(true);
    }

    public void Quit() {
        Application.Quit();
    }

    public void HaenselPlayer() {
        targetScene = "MainScene";
        LoadScene();
    }

    public void GretelPlayer() {
        targetScene = "MainScene";
        LoadScene();
    }
    
    void LoadScene()
    {
        LoadingData.sceneToLoad = targetScene;
        SceneManager.LoadScene("MainScene");
    }
}
public static class LoadingData 
{
    public static string sceneToLoad;
}
