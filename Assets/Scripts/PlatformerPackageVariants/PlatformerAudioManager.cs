using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource effectsSpeaker;
    [SerializeField]
    private AudioSource wallSlideSpeaker;
    [SerializeField]
    private AudioClip jumpSound;


    // Start is called before the first frame update
    void Awake()
    {
        if (effectsSpeaker == null) {
            Debug.LogError("No speaker for sound effects attached to player audio manager");
        }

        if (wallSlideSpeaker == null) {
            Debug.LogWarning("No speaker for wall sliding, won't make a sound when sliding on a wall");
        }
    }

    // Main function to play the jump sound effect
    public void playJumpSound() {
        effectsSpeaker.clip = jumpSound;
        effectsSpeaker.Play();
    }


    // Main function to set wall sliding sound
    //  Pre: changed from wall sliding to not wall sliding or vice-versa
    public void setWallSlideSound(bool wallSlideState) {
        if (wallSlideSpeaker != null) {
            if (wallSlideState) {
                wallSlideSpeaker.Play();
            } else {
                wallSlideSpeaker.Stop();
            }
        }
    }
}
