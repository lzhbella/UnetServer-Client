using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;

public class SQL_CreateDB : MonoBehaviour
{
	//DB create
	public InputField DBName;
	public Text text;

	//SQL connect
	public InputField IPAdderss;
	public InputField Port;
	public InputField UserID;
	public InputField Password;

	//Create charecter
	public InputField log;
	public InputField pass;
	public InputField CharNick;

	//DB chose
	public Dropdown DropDB;

	MySqlConnection connect;
	bool sqlUp;
	string DB;

	public void Connect ()
	{
		if (!sqlUp) {

            string connectString= string.Format("Server = {0}; Port= {1}; User ID = {2}; Password = {3};", IPAdderss.text, Port.text, UserID.text, Password.text);


            connect = new MySqlConnection (connectString);
			//connect.ConnectionString = "Server=" + IPAdderss.text + ";Port=" + Port.text + ";UserID=" + UserID.text + ";Password=" + Password.text + ";";
			connect.Open ();
            string result = connect.ServerVersion;
            Debug.Log(result);

            sqlUp = true;
		}
	}

	public void GetAllDB ()
	{
		if (sqlUp) {
			MySqlCommand Linq = new MySqlCommand ();
			Linq.Connection = connect;
			Linq.CommandText = "SHOW DATABASES";
			MySqlDataReader read = Linq.ExecuteReader ();
			DropDB.options.Clear ();
			while (read.Read ()) {
				Dropdown.OptionData dbname = new Dropdown.OptionData ();
				dbname.text = read.GetString (0);
				DropDB.options.Add (dbname);
			}
			read.Close ();
		}
	}

	public void DBSelect ()
	{
		if (sqlUp) {
			PlayerPrefs.SetString ("ip", IPAdderss.text);
			PlayerPrefs.SetString ("port", Port.text);
			DB = DropDB.options [DropDB.value].text;
			PlayerPrefs.SetString ("db", DB);
			PlayerPrefs.SetString ("user", UserID.text);
			PlayerPrefs.SetString ("pass", Password.text);
			PlayerPrefs.Save ();
		}
	}

	//Created database in MySQL
	public void CreateDB ()
	{
		if (sqlUp) {
			if (DBName.text.Length > 4) {
				MySqlCommand Linq = new MySqlCommand ();
				Linq.Connection = connect;
				Linq.CommandText = "CREATE DATABASE `" + DBName.text + "` CHARACTER SET utf8 COLLATE utf8_general_ci;";
				int i = Linq.ExecuteNonQuery ();
				if (i == 1) {
					DB = DBName.text;
					PlayerPrefs.SetString ("ip", IPAdderss.text);
					PlayerPrefs.SetString ("port", Port.text);
					PlayerPrefs.SetString ("db", DBName.text);
					PlayerPrefs.SetString ("user", UserID.text);
					PlayerPrefs.SetString ("pass", Password.text);
					PlayerPrefs.Save ();
					text.text = "Database create successfull!";
				} else if (i == -1) {
					text.text = "Database create fail.";
				}
			}
		} else {
			text.text = "First connect to MYSQL";
		}
	}

	//All needed table to work server in MySQL
	public void AddTable ()
	{
		connect.Close ();
		connect = new MySqlConnection ();
		connect.ConnectionString = "Server=" + IPAdderss.text + ";Port=" + Port.text + ";Database=" + DB + ";UserID=" + UserID.text + ";Password=" + Password.text + ";";
		connect.Open ();
		MySqlCommand Linq = new MySqlCommand ();
		Linq.Connection = connect;
        Linq.CommandText = "CREATE TABLE `accountlist` (id INT(10) NOT NULL PRIMARY KEY  AUTO_INCREMENT, AccountName TEXT(20) NOT NULL, PasswordAc TEXT(20) NOT NULL, Mail TEXT(20) NOT NULL);";
        Linq.ExecuteNonQuery ();
        Linq.CommandText = "CREATE TABLE `charecter` (char_id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, account_id INT(20) NOT NULL, PlayerName TEXT(20) NOT NULL, MaxHP INT(3), PlayerScores INT(20) DEFAULT '0', scene_ID INT(2) DEFAULT '0', x FLOAT(20, 5), y FLOAT(20, 5), z FLOAT(20, 5), zombie INT(10) DEFAULT '0', zombie_mutant INT(10) DEFAULT '0', zombie_strong INT(10) DEFAULT '0', rang INT(2) DEFAULT '0', title VARCHAR(20) DEFAULT ' ');";
        Linq.ExecuteNonQuery ();
		connect.Close ();
	}
}
