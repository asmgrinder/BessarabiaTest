using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Rendering;

public class MainMenuController : MonoBehaviour
{
    public Slider VolumeSlider;
    public static MainMenuController Instance => _instance;
    public float Volume => volume;

    static MainMenuController _instance;

    public bool IsActive => gameObject.activeSelf;

    float volume = 1;
    static string VolumeStr = "Volume";

    private void Awake()
    {
        volume = PlayerPrefs.GetFloat(VolumeStr, 1);
        if (null != VolumeSlider)
        {
            VolumeSlider.value = volume;
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
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnVolumeChanged(Slider s)
    {
        volume = s.value;
        PlayerPrefs.SetFloat(VolumeStr, volume);
    }

    public void Show(bool Show)
    {
        GameMngr gm = GameMngr.Instance;
        if (null != gm)
        {
            gameObject.SetActive(Show);
            //if (Show)
            //{
            //    gm.IncPauseReq();
            //}
            //else
            //{
            //    gm.DecPauseReq();
            //}
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
