using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PasswordRecovery : MonoBehaviour
{

	public InputField login;
	public Text txt;

	NetworkClient client = new NetworkClient ();
	Networking_client myClient;
	bool isConnect;
	bool registerSend;

	// Use this for initialization
	void Start ()
	{
		myClient = GameObject.Find ("Networking").GetComponent<Networking_client> ();

	}

	public void Answer (NetworkMessage netmsg)
	{
		txt.text = netmsg.reader.ReadString ();
	}

	void Update ()
	{
		if (client.isConnected) {
			if (!registerSend) {
				NetworkWriter wr = new NetworkWriter ();
				wr.StartMessage (Networking_msgType.RecoveryPass);
				wr.Write (login.text);
				wr.FinishMessage ();
				client.SendWriter (wr, 0);
				registerSend = true;
			}
		} else {
			isConnect = false;
		}
	}

	public void SendLogin ()
	{
		registerSend = false;
		if (!isConnect) {
			ConnectionConfig conf = new ConnectionConfig ();
			conf.AddChannel (QosType.Reliable);
			conf.AddChannel (QosType.Unreliable);
			client = new NetworkClient ();
			client.Configure (conf, 1);
			client.Connect (myClient.IPAdress, myClient.Port);
			isConnect = true;
			client.RegisterHandler (Networking_msgType.RecoveryPass, Answer);
		}
	}
}
