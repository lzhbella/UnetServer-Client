using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

//This script is responsible for the connection, as it logs all events

public class Networking_client : MonoBehaviour
{
	public static NetworkClient _Client;
	public string IPAdress = "127.0.0.1";
	public int Port = 45000;


    [SerializeField]
    GameObject LoginWindow;
    [SerializeField]
    GameObject PlayerSelect;

    public delegate void MyClient ();

	public static event MyClient OnNoAcc;
	public static event MyClient OnNoCarect;
	public static event NetworkMessageDelegate OnAccountOnline;
    public static event MyClient RegisterHandl;
    public static event MyClient DisconnectMe;

	public static NetworkConnection net = new NetworkConnection ();
	public NetworkClient myClient = new NetworkClient ();
	public static bool isConnection = false;
	bool con;

	//FPS
	private string fps;
	private float time;
	public float updateFPSTime = 1f;

	void Awake ()
	{
		DontDestroyOnLoad (this);
	}

	void OnGUI ()
	{
		GUIStyle style = new GUIStyle ();
        style.richText = true;
		style.fontSize = 18;
		GUI.Label (new Rect (20, 20, 50, 20), string.Format("<color=#ffffffff>{0}</color>", fps), style);
	}

	public void StartClient ()
	{
		if (!isConnection) {
			
			ConnectionConfig conf = new ConnectionConfig ();
			conf.AddChannel (QosType.Reliable);
			conf.AddChannel (QosType.Unreliable);
			myClient = new NetworkClient ();
			myClient.Configure (conf, 1);
			myClient.Connect(IPAdress, Port);

			RegisterHandler ();
		}
	}

	public void RegisterHandler ()
	{
		myClient.RegisterHandler (MsgType.Connect, OnConn);
		myClient.RegisterHandler (Networking_msgType.PlayerDataGet, HandleOnPlayerDataGet);
		myClient.RegisterHandler (Networking_msgType.NoAccount, OnNoAccount);
		myClient.RegisterHandler (Networking_msgType.NoChar, OnNoChar);
		myClient.RegisterHandler (Networking_msgType.AccountOnline, OnAccountOnline);

        myClient.RegisterHandler (Networking_msgType.DisconnectPlayer, OnDisconnect);
    }

	void OnConn (NetworkMessage netmsg)
	{

	}

    /// <summary>
    /// Get session ID and all chars on account
    /// </summary>
    /// <param name="netMsg"></param>
    void HandleOnPlayerDataGet(NetworkMessage netMsg)
    {
        Message.CharGet get = netMsg.ReadMessage<Message.CharGet>();
        Data_MyData.sessionID = get.index;
        if (LoginWindow.activeInHierarchy)
        {
            LoginWindow.SetActive(false);
            PlayerSelect.SetActive(true);
        }
    }

    void OnDisconnect (NetworkMessage netmsg)
	{
		try {
			int ID = netmsg.reader.ReadInt32 ();
            if (ID != Data_MyData.sessionID)
            {
                ID = Data_ListPlayerOnScene.FindPlayerID(ID);
                Destroy(Data_ListPlayerOnScene.PlayersOnScene[ID]);
                Data_ListPlayerOnScene.DisconnectPlayer(ID);
            }
            else
            {
                DisconnectMe();
            }
		} catch {
			Debug.Log ("Disconnect fail.");
		}
	}

	void Start ()
	{
		Scene1_LoginClick.OnClick += StartClient;
	}

	void Update ()
	{
		isConnection = myClient.isConnected;
		if (Time.time - time >= updateFPSTime) {
			time = Time.time;
			fps = (1 / Time.deltaTime).ToString ();
		}
		if (isConnection) {
			if (!con) {
				net = myClient.connection;
                RegisterHandl();
                con = true;
			}
		}
	}

	void OnNoAccount (NetworkMessage netmsg)
	{
		OnNoAcc ();
	}

	void OnNoChar (NetworkMessage netmsg)
	{
		OnNoCarect ();
	}

    public static void SendMyAction(short msgType)
    {
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(msgType);
        wr.Write(Data_MyData.sessionID);
        wr.Write(Data_MyData.Login);
        wr.Write(Data_MyData.Password);
        wr.FinishMessage();
        net.SendWriter(wr, 1);
    }
}




