using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace IntoTheWoods {
    public class Buttons : MonoBehaviour {
        [FormerlySerializedAs("Haensel")]
        public GameObject haensel;
        [FormerlySerializedAs("Gretel")]
        public GameObject gretel;

        public void Play() {
            Debug.Log("Play");
            haensel.gameObject.SetActive(true);
            gretel.gameObject.SetActive(true);
        }

        public void Quit() {
            Application.Quit();
        }

        public void HaenselPlayer() {
            LoadScene();
        }

        public void GretelPlayer() {
            LoadScene();
        }

        private void LoadScene() {
            SceneManager.LoadScene("MainScene");
        }
    }
}
