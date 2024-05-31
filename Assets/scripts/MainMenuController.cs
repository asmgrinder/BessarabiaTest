using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Rendering;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public Slider SoundVolumeSlider, MusicVolumeSlider;

    public TMP_Text Stats;
    public static MainMenuController Instance => _instance;
    public float SoundVolume => soundVolume;
    public float MusicVolume => musicVolume;

    static MainMenuController _instance;

    public bool IsActive => gameObject.activeSelf;

    float soundVolume = 1, musicVolume = 1;
    static string SoundVolumeStr = "SoundVolume";
    static string MusicVolumeStr = "MusicVolume";

    private void Awake()
    {
        soundVolume = PlayerPrefs.GetFloat(SoundVolumeStr, 1);
        if (null != SoundVolumeSlider)
        {
            SoundVolumeSlider.value = soundVolume;
        }
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeStr, 1);
        if (null != MusicVolumeSlider)
        {
            MusicVolumeSlider.value = musicVolume;
        }
        if (null == _instance)
        {
            _instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameMngr.Instance.IsPaused = true;
    }

    // Update is called once per frame
    void Update()
    {
        GameMngr gm = GameMngr.Instance;
        if (null != gm)
        {
            Stats.text = "Wins / Defeats: " + gm.Wins + " / " + gm.Loses;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnSoundVolumeChanged(Slider s)
    {
        soundVolume = s.value;
        PlayerPrefs.SetFloat(SoundVolumeStr, soundVolume);
    }

    public void OnMusicVolumeChanged(Slider s)
    {
        musicVolume = s.value;
        PlayerPrefs.SetFloat(MusicVolumeStr, musicVolume);
    }

    public void Show(bool Show)
    {
        GameMngr gm = GameMngr.Instance;
        if (null != gm)
        {
            gameObject.SetActive(Show);
            gm.IsPaused = Show;
            Cursor.visible = Show;
            Cursor.lockState = Show ? CursorLockMode.None : CursorLockMode.Locked;
            if (!Show
                && !PhotonNetwork.IsConnected)
            {
                gm.ResetPlayerHP();
                SimpleLauncher.Instance.DoConnect();
            }
        }
    }
}
