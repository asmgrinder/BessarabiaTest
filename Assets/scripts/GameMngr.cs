using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class GameMngr : MonoBehaviourPunCallbacks
{
    public float PlayerDamage = 0.5f;
    public MessageController Message;

    //public TMP_Text DebugText;
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

    public bool IsDisconnecting => disconnecting;

    public int Wins => wins;
    public int Loses => loses;

    static GameMngr _instance;

    const string HPStr = "HitPoints";
    const string KillsStr = "Kills";
    const string MoneyStr = "Money";
    const string WinsStr = "Wins";
    const string LosesStr = "Loses";

    RectTransform uiHP;
    TMP_Text uiKills;
    TMP_Text uiMoney;

    bool paused = false;
    float HP = 100;
    int kills;
    int money;
    int wins;
    int loses;

    bool disconnecting = false;
    bool spawnComplete;

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
            kills = PlayerPrefs.GetInt(KillsStr, 0);
            money = PlayerPrefs.GetInt(MoneyStr, 0);
            wins = PlayerPrefs.GetInt(WinsStr, 0);
            loses = PlayerPrefs.GetInt(LosesStr, 0);

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
        if (null != MainMenuController.Instance
            && !MainMenuController.Instance.IsActive)
        {
            if (FindObjectsOfType(typeof(PauseController)) is PauseController[] pcs)
            {
                IsPaused = Array.FindIndex(pcs, pc => pc.Paused) >= 0;
                spawnComplete = Array.FindIndex(pcs, pc => !pc.SpawnComplete) < 0;
                //Debug.Log("spawnComplete: " + spawnComplete);
            }
            else
            {
                IsPaused = false;
            }
        }
        CheckRoundEnd();
    }

    void setUI()
    {
        if (null != uiHP)
        {
            uiHP.localScale = Vector3.one + (HP * 0.01f - 1) * Vector3.right;
        }
        if (null != uiKills)
        {
            uiKills.text = "Kills: " + kills.ToString();
        }
        if (null != uiMoney)
        {
            uiMoney.text = "Cash: " + money.ToString();
        }
    }

    public void ResetPlayerHP()
    {
        HP = 100;
        setUI();
    }

    public void IncKills(int Income)
    {
        kills++;
        money += Income;
        PlayerPrefs.SetInt(KillsStr, kills);
        PlayerPrefs.SetInt(MoneyStr, money);
    }

    public void PlayerHit()
    {
        float prevHP = HP;
        HP -= Mathf.Min(HP, PlayerDamage);
        PlayerPrefs.SetFloat(HPStr, HP);
        setUI();
        if (0 == HP
            && prevHP > 0)
        {
            // lost
            loses++;
            PlayerPrefs.SetInt(LosesStr, loses);
            Message.Show("Defeat");
            RoundEnd();
        }
    }

    public void CheckRoundEnd()
    {
        //Debug.Log("#" + (index < 0 && spawnComplete));
        if (spawnComplete
            && GameObject.FindGameObjectsWithTag("player").Length > 0)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("protobot");
            int index = Array.FindIndex(gos, go => go.TryGetComponent(out ProtobotController pc) && pc.HitPoints > 0);

            if (index < 0)
            {
                wins++;
                PlayerPrefs.SetInt(WinsStr, wins);
                Message.Show("Victory");
                RoundEnd();
            }
        }
    }

    public void RoundEnd()
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
        StartCoroutine(disconnect());
    }

    IEnumerator disconnect()
    {
        while (Message.IsActive)
        {
            yield return new WaitForSeconds(0.05f);
        }
        disconnecting = true;
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return new WaitForSeconds(0.05f);
        }
        disconnecting = false;
        MainMenuController.Instance.Show(true);
    }
}
