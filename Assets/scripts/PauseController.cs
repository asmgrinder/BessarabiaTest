using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseController : MonoBehaviourPunCallbacks
{
    public bool SP;
    public bool Paused => null != photonView && !photonView.IsMine && paused;
    public bool SpawnComplete => null != photonView && spawnComplete;
    bool paused;
    bool spawnComplete;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (null != photonView
            && photonView.IsMine
            && null != SimpleLauncher.Instance
            && PhotonNetwork.IsConnectedAndReady)
        {
            photonView.RPC("setParams", RpcTarget.All,
                            MainMenuController.Instance.IsActive,
                            SimpleLauncher.Instance.SpawnComplete);
        }
        SP = SpawnComplete;
    }

    [PunRPC]
    void setParams(bool Paused, bool SpawnCompl)
    {
        paused = Paused;
        spawnComplete = SpawnCompl;
    }
}
