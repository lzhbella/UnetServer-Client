using UnityEngine;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Mail;
using UnityEngine.Networking;

public class SQL_PasswordRecovery
{

	private static MySqlCommand Linq = SQL_sqlConnect.Linq;

	//Name SMTP server (defaul for gmail 'smtp.gmail.com')
	static string smtpServer = "smtp.gmail.com";
	//Port SMTP server (defaul for gmail '587')
	static int port = 587;
	//SSL encoding SMTP server (defaul for gmail 'true')
	static bool SSL = true;
	//Name in mail
	static string _name = "My Server. Password recovery service.";
	//Name your mail addres
	static string From = "";
	//Password your mail
	static string password = "";
	//Mail theme
	static string mailTheme = "Passwor Recovery";
	//Mail message
	static string message = "Please do not forget your password: ";

	static MailMessage mail;

	public static void PasswordRecovery (NetworkMessage netmsg)
	{
		string login = netmsg.reader.ReadString ();
		Linq.CommandText = "SELECT PasswordAc, Mail FROM accountlist WHERE AccountName='" + login + "';";
		MySqlDataReader reader = Linq.ExecuteReader ();
		try {
			reader.Read ();
			string pass = message + reader.GetString (0);
			string mailto = reader.GetString (1);
			ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) {
				return true;
			};
			MailAddress _From = new MailAddress (From, _name);
			MailAddress _mailto = new MailAddress (mailto);
			mail = new MailMessage (_From, _mailto);
			mail.Subject = mailTheme;
			mail.Body = pass;
			SmtpClient smtp = new SmtpClient (smtpServer, port);
			smtp.EnableSsl = SSL;
			smtp.Credentials = (ICredentialsByHost)new NetworkCredential(From.Split('@')[0], password);
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			string callback = "mail";
			smtp.SendCompleted += CallBack;
			smtp.SendAsync (mail, callback);
			NetworkWriter wr = new NetworkWriter ();
			wr.StartMessage (Networking_msgType_Sr.PasswordRecovery);
			wr.Write ("Password sent to your email.");
			wr.FinishMessage ();
			netmsg.conn.SendWriter (wr, 0);
			mail.Dispose ();
			reader.Close ();
		} catch {
			NetworkWriter wr = new NetworkWriter ();
			wr.StartMessage (Networking_msgType_Sr.PasswordRecovery);
			wr.Write ("Login not found.");
			wr.FinishMessage ();
			netmsg.conn.SendWriter (wr, 0);
		}
		return;
	}

	public static void CallBack (object AsyncCallback, System.ComponentModel.AsyncCompletedEventArgs e)
	{
		if (e.Cancelled) {
			Debug.Log ("Mail not sent");
		}
		if (e.Error != null) {
			Debug.Log (e.Error.Message);
			Debug.Log (e.Error.HelpLink);
		} else {
			Debug.Log ("Mail bean sent");
		}
	}
}
