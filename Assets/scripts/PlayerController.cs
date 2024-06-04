using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform MountPoint;
    public Transform[] WeaponsMountPoints;
    public Transform[] Weapons;
    public string[] SlotStrings;
    //public float Speed = 1f;
    public float RotSpeed = 3f;
    public bool InvertY = true;

    [Range(0, 15)] public float JumpSpeed = 10;

    [Range(1, 50)] public float MaxSpeed1 = 10;
    [Range(1, 50)] public float MaxSpeed2 = 20;

    public float Acceleration = 1;
    public float Deceleration = 2;

    [Range(20, 70)] public float MaxVertAng = 50;
    [Range(0, 3)] public float ThrowDistance = 0.8f;

    public AudioClip MusicClip;
    public AudioClip MoveClip, JumpClip, HitClip;

    float RotX = 0;
    //float RotY = 0;
    Rigidbody rb;

    Transform[] slots;

    Transform[] weaponAvatar;

    //float hp = 100;

    Vector3 InitialCameraPos;
    Quaternion InitialCameraRot;

    AudioSource musicSound;
    AudioSource moveSound, jumpSound, hitSound;

    float hitSoundTimer = 0;
    const float hitTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        slots = new Transform[WeaponsMountPoints.Length];
        weaponAvatar = new Transform[slots.Length];

        musicSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource; 
        musicSound.volume = 0;
        musicSound.loop = true;
        musicSound.clip = MusicClip;
        musicSound.Play();

        moveSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        //moveSound.playOnAwake = false;
        moveSound.volume = 0;
        moveSound.loop = true;
        moveSound.clip = MoveClip;
        moveSound.Play();

        jumpSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        jumpSound.playOnAwake = false;
        jumpSound.clip = JumpClip;

        hitSound = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        //hitSound.playOnAwake = false;
        hitSound.volume = 0;
        hitSound.loop = true;
        hitSound.clip = HitClip;
        hitSound.Play();

        rb = GetComponent<Rigidbody>();
        if (photonView.IsMine)
        {
            InitialCameraPos = Camera.main.transform.localPosition;
            InitialCameraRot = Camera.main.transform.localRotation;
            Camera.main.transform.parent = MountPoint;
        }
    }

    public void RestoreCamera()
    {
        if (photonView.IsMine)
        {
            Camera.main.transform.parent = null;
            Camera.main.transform.localPosition = InitialCameraPos;
            Camera.main.transform.localRotation = InitialCameraRot;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            float horz = Input.GetAxis("Mouse X");

            float vert = Input.GetAxis("Mouse Y");
            RotX += vert * RotSpeed * (InvertY ? -1 : 1);
            RotX = Mathf.Clamp(RotX, -MaxVertAng, MaxVertAng);

            if (!GameMngr.Instance.IsPaused)
            {
                transform.Rotate(0, horz * RotSpeed, 0);

                MountPoint.localRotation = Quaternion.AngleAxis(RotX, Vector3.right);

                float acc = Acceleration;
                Vector3 accDir = Vector3.zero;
                if (Mathf.Abs(rb.velocity.y) < 0.001f)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        rb.AddForce(JumpSpeed * Vector3.up, ForceMode.VelocityChange);
                    }

                    if (Input.GetKey(KeyCode.W))
                    {
                        accDir += Camera.main.transform.forward;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        accDir -= Camera.main.transform.forward;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        accDir -= Camera.main.transform.right;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        accDir += Camera.main.transform.right;
                    }

                    if (0 == accDir.sqrMagnitude)
                    {
                        accDir = -rb.velocity.normalized;
                        acc = Mathf.Min(Deceleration, rb.velocity.magnitude);
                    }
                    accDir.y = 0;
                }
                rb.AddForce(acc * accDir, ForceMode.Impulse);
                float maxSpeed = Input.GetKey(KeyCode.LeftShift) ? MaxSpeed2 : MaxSpeed1;
                if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
                {
                    rb.velocity = maxSpeed * rb.velocity.normalized;
                }
                if (null != MainMenuController.Instance)
                {
                    moveSound.volume = Mathf.Abs(rb.position.y) > 0.2f
                                        ? 0
                                        : MainMenuController.Instance.SoundVolume
                                            * rb.velocity.magnitude / MaxSpeed2;
                }
                if (Input.GetMouseButton(0))
                {
                    int index = getActiveSlot();
                    if (index >= 0)
                    {
                        WeaponController wc = slots[index].GetComponent<WeaponController>();
                        if (null != wc)
                        {
                            wc.DoHit();
                        }
                    }
                }
            }
        }
        else
        {
            Vector3 accDir = -rb.velocity.normalized;
            accDir.y = 0;
            float acc = Mathf.Min(Deceleration, rb.velocity.magnitude);
            rb.AddForce(acc * accDir, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)
            && !MainMenuController.Instance.IsActive)
        {
            MainMenuController.Instance.Show(true);
        }
        if (photonView.IsMine
            && !GameMngr.Instance.IsPaused)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                int slotIndex = getActiveSlot();
                if (slotIndex >= 0)
                {
                    Vector3 fwd = new(transform.forward.x, 0, transform.forward.z);
                    Vector3 newPos = slots[slotIndex].position + ThrowDistance * fwd;
                    newPos.y = weaponAvatar[slotIndex].position.y;
                    weaponAvatar[slotIndex].position = newPos;
                    weaponAvatar[slotIndex].localScale = slots[slotIndex].lossyScale;

                    slots[slotIndex].localScale = Vector3.zero;
                    slots[slotIndex] = null;
                }
            }

            int newIndex = -1;
            if (Input.GetKeyUp(KeyCode.E)
                && (newIndex = Array.FindIndex(slots, s => null != s)) >= 0)
            {
                setActiveSlot(newIndex);
            }

            KeyCode[] codes = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
            int newSlotIndex = Array.FindIndex(codes, c => Input.GetKey(c));
            if (newSlotIndex >= 0)
            {
                setActiveSlot(newSlotIndex);
            }
        }
        if (null != MainMenuController.Instance)
        {
            hitSoundTimer -= Mathf.Min(hitSoundTimer, Time.deltaTime);
            hitSound.volume = GameMngr.Instance.IsPaused ? 0 : hitSoundTimer / hitTime;
        }
        musicSound.volume = MainMenuController.Instance.MusicVolume;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground")
            && null != MainMenuController.Instance
            && null != jumpSound)
        {
            jumpSound.volume = MainMenuController.Instance.SoundVolume;
            jumpSound.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int index = -1;
        if (null != other
            && other.TryGetComponent(out WeaponController wc)
            && null != wc && wc.IsMine
            && (index = Array.FindIndex(SlotStrings, s => other.gameObject.CompareTag(s))) >= 0
            && null != slots
            && null == slots[index])
        {
            weaponAvatar[index] = other.transform;
            other.transform.localScale = Vector3.zero;

            slots[index] = Weapons[index];

            setActiveSlot(index);
        }
        if (other.gameObject.CompareTag("fan")
            && other.transform.localScale.sqrMagnitude > 0.98f)
        {
            hitSoundTimer = hitTime;
            if (photonView.IsMine)
            {
                GameMngr.Instance.PlayerHit();
            }
        }
    }

    void setActiveSlot(int index)
    {
        if (index >= 0
            && index < slots.Length
            && null != slots[index])
        {
            foreach (Transform slot in slots)
            {
                if (null != slot)
                {
                    slot.localScale = Vector3.zero;
                }
            }
            slots[index].localScale = Vector3.one;
        }
    }

    int getActiveSlot()
    {
        int index = Array.FindIndex(slots, s => null != s && s.localScale.sqrMagnitude > 0);
        return index;
    }
}
