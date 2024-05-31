using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
//using static UnityEditor.PlayerSettings;
//using UnityEditor.VersionControl;

public class GameMngr : MonoBehaviourPunCallbacks
{
    public float PlayerDamage = 0.5f;
    public MessageController Message;
    public bool IsPaused
    {
        get
        {
            return paused;
        }
        set
        {
            paused = value;
            Time.timeScale = paused ? 0.005f : 1;
        }
    }
    public static GameMngr Instance => _instance;

    static GameMngr _instance;

    const string HPStr = "HitPoints";
    const string KillsStr = "Kills";
    const string MoneyStr = "Money";

    RectTransform uiHP;
    TMP_Text uiKills;
    TMP_Text uiMoney;

    bool paused = false;
    float HP = 100;
    int Kills;
    int Money;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (null == _instance)
        {
            _instance = this;
            //DontDestroyOnLoad(gameObject);

            HP = PlayerPrefs.GetFloat(HPStr, 100);
            if (HP <= 0)
            {
                HP = 100;
            }
            PlayerPrefs.SetFloat(HPStr, HP);
            Kills = PlayerPrefs.GetInt(KillsStr, 0);
            Money = PlayerPrefs.GetInt(MoneyStr, 0);

            GameObject goHP = GameObject.FindGameObjectWithTag("player_hp");
            uiHP = goHP?.GetComponent<RectTransform>();
            GameObject goKills = GameObject.FindGameObjectWithTag("player_kills");
            uiKills = goKills?.GetComponent<TMP_Text>();
            GameObject goMoney = GameObject.FindGameObjectWithTag("player_money");
            uiMoney = goMoney?.GetComponent<TMP_Text>();

            setUI();
        }
    }

    private void Update()
    {
        if (!MainMenuController.Instance.IsActive)
        {
            //GameObject[] gos = GameObject.FindGameObjectsWithTag("pause");
            //if (null != gos)
            if (FindObjectsOfType(typeof(PauseController)) is PauseController[] pcs)
            {
                //int mineIndex = Array.FindIndex(gos, go => go.TryGetComponent(out PauseController pc) && pc.photonView.IsMine);
                //int index = -1;
                //for (int i = 0; i < gos.Length; i++)
                //{
                //    if (i != mineIndex
                //        && gos[i].transform.position.sqrMagnitude > 0)
                //    {
                //        index = i;
                //        break;
                //    }
                //}
                IsPaused = Array.FindIndex(pcs, pc => pc.Paused) >= 0;
            }
            else
            {
                IsPaused = false;
            }
        }
    }

    void setUI()
    {
        if (null != uiHP)
        {
            uiHP.localScale = Vector3.one + (HP * 0.01f - 1) * Vector3.right;
        }
        if (null != uiKills)
        {
            uiKills.text = "Kills: " + Kills.ToString();
        }
        if (null != uiMoney)
        {
            uiMoney.text = "Cash: " + Money.ToString();
        }
    }

    public void IncKills(int Income)
    {
        Kills++;
        Money += Income;
        PlayerPrefs.SetInt(KillsStr, Kills);
        PlayerPrefs.SetInt(MoneyStr, Money);
    }

    public void PlayerHit()
    {
        HP -= Mathf.Min(HP, PlayerDamage);
        PlayerPrefs.SetFloat(HPStr, HP);
        setUI();
        if (0 == HP)
        {
            // lost
            roundEnd("Defeat");
            //Debug.Log("lost");
        }
    }

    public void ResetPlayerHP()
    {
        HP = 100;
        setUI();
    }

    public void CheckRoundEnd()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("protobot");
        int index = Array.FindIndex(gos, go => go.transform.localScale.sqrMagnitude > 0);
        if (index < 0
            && SimpleLauncher.Instance.SpawnComplete)
        {
            roundEnd("Victory");
            //Debug.Log("win");
        }
    }

    void roundEnd(string Result)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("player");
        if (null != players)
        {
            foreach (GameObject player in players)
            {
                if (player.TryGetComponent(out PlayerController pc))
                {
                    pc.RestoreCamera();
                }
            }
        }
        Message.Show(Result);
        StartCoroutine(disconnect());
    }

    IEnumerator disconnect()
    {
        while (Message.IsActive)
        {
            yield return new WaitForSeconds(0.05f);
        }
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return new WaitForSeconds(0.05f);
        }
        MainMenuController.Instance.Show(true);
    }
}
