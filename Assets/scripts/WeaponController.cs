using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviourPunCallbacks
{
    public float MaxAngle = 70;
    public float AnimTime = 0.25f;
    public Vector3 RotationAxis;

    public bool IsMine => null == photonView ? false : photonView.IsMine;

    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Mathf.Min(timer, Time.deltaTime);
            float t = 1 - timer / AnimTime;
            float _t = 0.5f * (1 - Mathf.Cos(t * 2 * Mathf.PI));
            transform.localRotation = Quaternion.AngleAxis(MaxAngle * _t, RotationAxis);
        }
    }

    public void DoHit()
    {
        if (0 == timer)
        {
            timer = AnimTime;
        }
    }
}
