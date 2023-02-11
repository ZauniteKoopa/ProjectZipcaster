using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AmbientAddOn : ScriptableObject
{
    // Main clip
    [SerializeField]
    private AudioClip mainAmbientClip;
    [SerializeField]
    [Range(0f, 1f)]
    private float audioVolume;


    // Main function to add the ambient sound to this GameObject
    public void addAmbientSound(GameObject obj) {
        // Add component
        AudioSource speaker = obj.AddComponent<AudioSource>();

        // Set properties
        speaker.clip = mainAmbientClip;
        speaker.loop = false;
        speaker.volume = audioVolume;

    }

}
