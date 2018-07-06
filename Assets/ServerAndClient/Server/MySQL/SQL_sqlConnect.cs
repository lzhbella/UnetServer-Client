using UnityEngine;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System.Collections.Generic;

public class SQL_sqlConnect : MonoBehaviour
{
	string IPAdderss;
	string Port;
	string Database;
	string UserID;
	string Password;

	static bool sqlUp = false;
    public static bool SqlUp { get{ return sqlUp; } }
	public static MySqlConnection Connection = new MySqlConnection ();
	public static MySqlCommand Linq = new MySqlCommand ();

    static List<TopPlayer> TopList = new List<TopPlayer>();
    static NetworkWriter top = new NetworkWriter();

	//Connect ot MySQL of the latest data created DB
	public void Connect ()
	{
        try {
            if (!sqlUp) {
                IPAdderss = PlayerPrefs.GetString("ip");
                Port = PlayerPrefs.GetString("port");
                Database = PlayerPrefs.GetString("db");
                UserID = PlayerPrefs.GetString("user");
                Password = PlayerPrefs.GetString("pass");
                string connectString = string.Format("Server = {0}; Port= {1}; Database={2}; User ID = {3}; Password = {4};", IPAdderss, Port, Database, UserID, Password);
                Connection.ConnectionString = connectString;
                //Connection.ConnectionString = "Server=" + IPAdderss + ";Port=" + Port + ";Database=" + Database + ";UserID=" + UserID + ";Password=" + Password + ";";
                Connection.Open();
                Linq.Connection = Connection;
                sqlUp = true;
                Debug.Log("SQL connect");
            }
            SQL_SavePlayerData.CalculateRangs();
            LoadTopPlayerList();
            Networking_Server.ErrorMessage("MySQL connect done");
        }
        catch
        {
            Networking_Server.ErrorMessage("MySQL connect fail");
        }
	}

    public void LoadTopPlayerList()
    {
        TopList.Clear();
        Linq.CommandText = "SELECT PlayerName, zombie, zombie_mutant, zombie_strong, PlayerScores FROM charecter ORDER BY PlayerScores DESC";
        MySqlDataReader reader = Linq.ExecuteReader();
        int r = 0;
        while(reader.Read() && r < 10)
        {
            int i = 0;
            TopPlayer t = new TopPlayer();
            t.nick = reader.GetString(i++);
            t.zombieKill = int.Parse(reader.GetString(i++));
            t.zombieMutantKill = int.Parse(reader.GetString(i++));
            t.zombieStrongKill = int.Parse(reader.GetString(i++));
            t.Score = int.Parse(reader.GetString(i++));
            TopList.Add(t);
            r++;
        }
        reader.Close();
        top = new NetworkWriter();
        top.StartMessage(Networking_msgType_Sr.TopList);
        top.Write(TopList.Count);
        foreach(TopPlayer pl in TopList)
        {
            top.Write(pl.nick);
            top.Write(pl.zombieKill);
            top.Write(pl.zombieMutantKill);
            top.Write(pl.zombieStrongKill);
            top.Write(pl.Score);
        }
        top.FinishMessage();
        
        Invoke("LoadTopPlayerList", 30f);
    }

    public static void SendTopList(NetworkMessage netMsg)
    {
        Message_Sr.PlayerAction act = netMsg.ReadMessage<Message_Sr.PlayerAction>();
        if(Networking_OnConnect.AccountVerefication(act.index, act.log, act.pass))
        {
           NetworkConnection con = RoomsManager.GetPlayerConnection(act.index);
            if(con != null)
            {
                con.SendWriter(top, 1);
            }
        }
    }
}

[System.Serializable]
public class TopPlayer
{
    public string nick;
    public int zombieKill;
    public int zombieMutantKill;
    public int zombieStrongKill;
    public int Score;
}
