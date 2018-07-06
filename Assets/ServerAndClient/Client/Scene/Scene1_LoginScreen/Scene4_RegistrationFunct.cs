using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Scene4_RegistrationFunct : MonoBehaviour
{
	public InputField login;
	public InputField password;
	public InputField mail;

	public Text serverAnswer;

	public GameObject logCanvas;

	NetworkClient client = new NetworkClient ();
	Networking_client myClient;
	bool isConnect;
	bool registerSend;

	// Use this for initialization
	void Start ()
	{
		myClient = GameObject.Find ("Networking").GetComponent<Networking_client> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (client.isConnected) {
			if (!registerSend) {
				Message.Registration regist = new Message.Registration ();
				regist.Login = login.text;
				regist.Password = password.text;
				regist.Mail = mail.text;
				client.Send (Networking_msgType.Registration, regist);
				registerSend = true;
			}
		} else {
			isConnect = false;
		}
	}

	public void OnServerAnswer (NetworkMessage netmsg)
	{
		Message.Registration reg = netmsg.ReadMessage<Message.Registration> ();
		bool regResult = reg.Registr;
		if (!regResult) {
			serverAnswer.text = reg.Answer;
		} else {
			myClient.myClient = client;
			Networking_client.isConnection = true;
			myClient.RegisterHandler ();
			serverAnswer.text = reg.Answer;
		}
	}

	public void OnRegistrationClick ()
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
			client.RegisterHandler (Networking_msgType.Registration, OnServerAnswer);
		}
	}

	public void OnBackClick ()
	{
		logCanvas.SetActive (true);
		this.gameObject.SetActive (false);
	}
}
