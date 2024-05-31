using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class ProtobotController : MonoBehaviourPunCallbacks
{
    public float RotSpeed = 30;
    public float MaxSpeed = 5;
    public float Acc = 15;
    public int Income = 100;

    public Transform HP;

    public AudioClip HitClip;

    [Range(0.001f, 1)]public float ShrinkTime = 0.25f;

    public float[] WeaponsDamage = new float[] { 5, 4, 3 };
    string[] weaponsTags = new string[] { "slot 1", "slot 2", "slot 3" };

    float hp = 100;

    Material _hpMat;
    Material hpMat => _hpMat ?? HP?.GetComponent<MeshRenderer>()?.material;

    //float pollTimer;
    PlayerController targetPlayer;

    Rigidbody rb;

    AudioSource hitSound;

    float shrinkTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        //pollTimer = 0.05f;
        rb = GetComponent<Rigidbody>();
        hitSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        hitSound.playOnAwake = false;
        hitSound.clip = HitClip;
    }

    // Update is called once per frame
    void Update()
    {
        if (null != HP
            && null != HP.transform)
        {
            HP.transform.LookAt(Camera.main.transform);
        }
        if (null != hpMat)
        {
            hpMat.SetFloat("_Ratio", hp / 100);
        }
        if (0 == hp
            && shrinkTimer > 0)
        {
            shrinkTimer -= Mathf.Min(shrinkTimer, Time.deltaTime);
            transform.localScale = (shrinkTimer / ShrinkTime) * Vector3.one;
            if (0 == shrinkTimer)
            {
                GameMngr.Instance.IncKills(Income);
                transform.localScale = Vector3.zero;
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }

                GameMngr.Instance.CheckRoundEnd();
            }
        }
    }

    void FixedUpdate()
    {
        //pollTimer -= Mathf.Min(pollTimer, Time.deltaTime);
        //if (0 == pollTimer)
        //{
        //    pollTimer = 0.05f;
        //}
        if (null != photonView
            && photonView.IsMine)
        {
            if (FindObjectsOfType(typeof(PlayerController)) is PlayerController[] players)
            {
                float dist = 1e10f;
                int minIndex = -1;
                for (int i = 0; i < players.Length; i++)
                {
                    float d;
                    if (null != players[i]
                        && dist > (d = Vector3.Distance(players[i].transform.position, transform.position)))
                    {
                        dist = d;
                        minIndex = i;
                    }
                }
                if (minIndex >= 0)
                {
                    targetPlayer = players[minIndex];
                }
            }
            if (null != targetPlayer
                && Time.deltaTime > 0)
            {
                Vector3 v = (targetPlayer.transform.position - transform.position);
                v = new Vector3(v.x, 0, v.z).normalized;
                Vector3 cross = Vector3.Cross(transform.forward, v);
                cross.x = cross.z = 0;
                float mag = cross.magnitude;
                if (mag > 1e-4f)
                {
                    float ang = Mathf.Asin(mag);
                    float angV = Mathf.Min(ang / Time.deltaTime, RotSpeed);
                    rb.angularVelocity = angV * cross.normalized;
                    //transform.localRotation *= Quaternion.AngleAxis(angV, cross.normalized);

                    if (ang < 0.5f * Mathf.PI)
                    {
                        if (rb.velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
                        {
                            rb.velocity = MaxSpeed * rb.velocity.normalized;
                        }
                        rb.AddForce(Acc * transform.forward);
                        float cosa = Vector3.Dot(transform.right, rb.velocity.normalized);
                        Vector3 f = -cosa * rb.velocity.magnitude * transform.right;
                        rb.AddForce(f);
                    }
                    else
                    {
                        rb.velocity = Vector3.zero;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int weaponIndex = Array.FindIndex(weaponsTags, wt => other.gameObject.CompareTag(wt));
        if (weaponIndex >= 0
            && hp > 0)
        {
            hp -= Mathf.Min(hp, WeaponsDamage[weaponIndex]);
            if (null != MainMenuController.Instance)
            {
                hitSound.volume = MainMenuController.Instance.Volume;
                hitSound.Play();
            }
            photonView.RPC("setHP", RpcTarget.All, hp);
        }
    }

    [PunRPC]
    void setHP(float newHP)
    {
        hp = newHP;
        //checkZeroHP();
        if (0 == hp)
        {
            shrinkTimer = ShrinkTime;
        }
    }
}
