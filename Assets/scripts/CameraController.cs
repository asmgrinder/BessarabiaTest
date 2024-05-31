using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public AudioClip MusicClip;

    AudioSource musicSound;
    // Start is called before the first frame update
    void Start()
    {
        musicSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource; 
        musicSound.volume = 0;
        musicSound.loop = true;
        musicSound.clip = MusicClip;
        musicSound.Play();

    }

    // Update is called once per frame
    void Update()
    {
        musicSound.volume = MainMenuController.Instance.MusicVolume;
    }
}
