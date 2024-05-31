using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseController : MonoBehaviourPunCallbacks
{
    public bool Mine;
    public bool PP;
    public bool P;
    public bool Paused => null != photonView && !photonView.IsMine && paused;
    bool paused;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Mine = photonView.IsMine;
        PP = paused;
        if (photonView.IsMine)
        {
            transform.position = (GameMngr.Instance.IsPaused ? 1 : 0) * Vector3.right;
            if (null != photonView
                && PhotonNetwork.IsConnectedAndReady)
            {
                photonView.RPC("setPaused", RpcTarget.All, MainMenuController.Instance.IsActive);
            }
        }
        P = Paused;
    }

    [PunRPC]
    void setPaused(bool Paused)
    {
        paused = Paused;
    }
}
