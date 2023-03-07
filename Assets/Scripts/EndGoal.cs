using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoal : MonoBehaviour
{
    [SerializeField]
    private string sceneName = "MainMenu";

    // On trigger enter by the player, heal the player
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerPackage testPlayer = collider.GetComponent<PlatformerPackage>();

        if (testPlayer != null) {
            SceneManager.LoadScene(sceneName);
            Object.Destroy(gameObject);
        }
    }
}
