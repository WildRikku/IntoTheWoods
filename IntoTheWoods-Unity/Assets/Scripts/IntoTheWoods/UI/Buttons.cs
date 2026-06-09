using UnityEngine;

using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour {
    public GameObject Haensel;
    public GameObject Gretel;
    
    public string targetScene;
    public void Play() {
        Debug.Log("Play");
        Haensel.SetActive(true);
        Gretel.SetActive(true);
    }

    public void Quit() {
        Application.Quit();
    }

    public void HaenselPlayer() {
        targetScene = "MainScene";
    }

    public void GretelPlayer() {
        targetScene = "MainScene";
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
