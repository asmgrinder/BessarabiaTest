using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class SimpleLauncher : MonoBehaviourPunCallbacks
{
	public PhotonView PlayerPrefab;
	public PhotonView PausePrefab;
    public PhotonView[] Items;
	public Vector3[] ItemsPositions;
	public Quaternion[] ItemsRotations;

	public Transform Ground;
	public PhotonView Protobot;
	public int ProtobotCount = 3;
	public float ProtoBotDelay = 3;

	public bool SpawnComplete => 0 == protobotCount;
    public static SimpleLauncher Instance => _instance;

    static SimpleLauncher _instance;

    float timer = 0;
    int protobotCount = 3;

    // Start is called before the first frame update
    void Awake()
	{
        if (null == _instance)
        {
            _instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    public void DoConnect()
	{
        timer = 1e9f;
		PhotonNetwork.ConnectUsingSettings();
    }

	public void DoDisconnect()
	{
		PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined a room.");
        PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.identity);
		PhotonNetwork.Instantiate(PausePrefab.name, Vector3.zero, Quaternion.identity);
		for (int i = 0; i < Items.Length; i++)
		{
			PhotonNetwork.Instantiate(Items[i].name, ItemsPositions[i], ItemsRotations[i]);
		}
		protobotCount = ProtobotCount;// PhotonNetwork.IsMasterClient ? ProtobotCount : 0;
        timer = ProtoBotDelay;
    }

    private void Update()
    {
		if (null != Protobot
            && protobotCount > 0
            && PhotonNetwork.IsConnectedAndReady)
		{
			timer -= Mathf.Min(timer, Time.deltaTime);
			if (0 == timer)
			{
                protobotCount--;

                float r = 3;
				float ang = Random.Range(-180, 180);// * Mathf.PI / 180;
				Vector3 newPos = r * (Mathf.Sin(ang) * Vector3.right + Mathf.Cos(ang) * Vector3.forward)
								+ 0.75f * Vector3.up;
				//Debug.Log("Instantiating protobot at " + newPos);
				GameObject go = PhotonNetwork.Instantiate(Protobot.name, newPos, Quaternion.identity);
				if (go.TryGetComponent(out RotationConstraint rc)
					&& null != rc
					&& rc.sourceCount > 0)
				{
                    rc.SetSource(0, new ConstraintSource() { sourceTransform = Ground, weight = 1 });
				}
                timer = ProtoBotDelay;
			}
		}
    }
}
