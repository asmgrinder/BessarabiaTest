using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class RestartController : MonoBehaviour
{
    Button button;
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (null != button)
        {
            button.interactable = PhotonNetwork.IsConnectedAndReady;
        }
        if (null != text)
        {
            text.color = PhotonNetwork.IsConnectedAndReady ? Color.white : Color.gray;
        }
    }
}
