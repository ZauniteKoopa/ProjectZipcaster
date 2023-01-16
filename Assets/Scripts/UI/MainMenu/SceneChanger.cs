using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Main public function to change the scene
    public void changeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    // Main function to exit the application
    public void exitApplication() {
        Application.Quit();
    }
}
