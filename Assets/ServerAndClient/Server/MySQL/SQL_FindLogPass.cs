using UnityEngine;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

//Associated with the database and retrieves data of players, as well as putting a flag "online" if the account is used

public class SQL_FindLogPass
{
	public delegate void ServerReply ();

	private static MySqlCommand Linq = SQL_sqlConnect.Linq;

	public static Data_PlayerFile_Sr player;
    static List<Data_PlayerFile_Sr> charList = new List<Data_PlayerFile_Sr>();

	public static bool CheckLP (string login, string password, out List<Data_PlayerFile_Sr> list, out int accID)
	{
        bool check = true;
        accID = -1;
        charList.Clear();

		Linq.CommandText = string.Format("SELECT id FROM accountlist WHERE AccountName='{0}' AND PasswordAc='{1}'", login, password);
		MySqlDataReader Reader = Linq.ExecuteReader ();
		try {
			Reader.Read ();
			string AccId = Reader.GetString (0);
			Reader.Close ();

            accID = int.Parse(AccId);
			if (!Networking_OnConnect.CheckOnlineAccount (accID)) {
				charList = CharDataGet (AccId);
			} else {
                check = false;
				Networking_OnConnect.AccountUsed ();
			}
		} catch (MySqlException ex) {
			Reader.Close ();
			Debug.Log (ex.ErrorCode + ex.Message);
            check = false;
			Networking_OnConnect.NoAccount ();
		}
		Reader.Close ();
        list = charList;

        return check;
	}

	public static List<Data_PlayerFile_Sr> CharDataGet (string AccId)
	{
        List<Data_PlayerFile_Sr> list = new List<Data_PlayerFile_Sr>();

		Linq.CommandText = "SELECT PlayerName, scene_ID, MaxHP, PlayerScores FROM charecter WHERE account_id = '" + AccId + "'";
		MySqlDataReader Reader = Linq.ExecuteReader ();
		try {
			while (Reader.Read ()) {
                int i = 0;
				player = ScriptableObject.CreateInstance<Data_PlayerFile_Sr> ();
				player.nick = Reader.GetString (i++);
				player.sceneID = int.Parse (Reader.GetString (i++));
				player.HPMax = int.Parse (Reader.GetString (i++));
				player.PlayerScores = int.Parse (Reader.GetString (i++));

                list.Add(player);
			}
			Reader.Close ();
		} catch (MySqlException ex) {
			Reader.Close ();
			Debug.Log (ex.ErrorCode + ex.Message);
		}

        return list;
	}
}
