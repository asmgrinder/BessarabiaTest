using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ContinueController : MonoBehaviour
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
        button.interactable = !GameMngr.Instance.IsDisconnecting;
        text.text = PhotonNetwork.IsConnectedAndReady ? "Continue" : "Play";
        text.color = button.interactable ? Color.white : Color.gray;
    }
}
