using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

//This script is responsible for the login window by clicking on the login button, we send our entries and obtain a response from the server

public class Scene1_LoginClick : MonoBehaviour
{
	public delegate void ConnectionClick ();

	public static event ConnectionClick OnClick;

	public GameObject PlayerSelect;
	public GameObject Registration;

	public InputField Login;
	public InputField Password;
	public Text txt;
	
	private bool click = false;

	void Start ()
	{
		Networking_client.OnNoAcc += Message;
		Networking_client.OnNoCarect += NextScene;
		Networking_client.OnAccountOnline += AccountUsed;
	}

	void Update ()
	{
		if (Networking_client.isConnection) {
			if (click) {
				Message.LoginSendMess log = new Message.LoginSendMess ();
				log.log = Data_MyData.Login;
				log.pass = Data_MyData.Password;
				Networking_client.net.Send (Networking_msgType.LoginSend, log);
				click = false;
			}
		}
	}

	public void AccountUsed (NetworkMessage netmsg)
	{
		txt.text = netmsg.reader.ReadString ();
	}

	public void Click ()
	{
		Data_MyData.Login = Login.text;
		Data_MyData.Password = Password.text;
		OnClick ();
		click = true;
	}

	void Message ()
	{
		txt.text = "No account created or wrong login and password.";
	}

	public void QuitClick ()
	{
		Application.Quit ();
	}

	public void NextScene ()
	{
		PlayerSelect.SetActive (true);
		this.gameObject.SetActive (false);
	}

	public void Registr ()
	{
		Registration.SetActive (true);
		this.gameObject.SetActive (false);
	}
}
