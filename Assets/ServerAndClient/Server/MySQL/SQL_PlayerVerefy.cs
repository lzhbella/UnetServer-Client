using UnityEngine;
using MySql.Data.MySqlClient;

public class SQL_PlayerVerefy
{
	public static int sceneID = 0;
	public static Data_PlayerFile_Sr player;
	private static string AcId;
	private static string playername;


	private static MySqlCommand Linq = SQL_sqlConnect.Linq;

	//Check player data, if data confirm, send player data and return true
	static public Data_PlayerFile_Sr CheckLP (string login, string password, string nick)
	{
		Data_PlayerFile_Sr check = null;
		Linq.CommandText = "SELECT id, AccountName, PasswordAc FROM accountlist WHERE AccountName='" + login + "' AND PasswordAc='" + password + "'";
		MySqlDataReader Reader = Linq.ExecuteReader ();

        try {
			Reader.Read ();
			AcId = Reader.GetString (0);
			Reader.Close ();

			Linq.CommandText = "SELECT PlayerName, scene_ID, MaxHP, PlayerScores, x, y, z, zombie, zombie_mutant, zombie_strong, title, rang FROM charecter WHERE account_id='" + AcId + "' AND PlayerName='" + nick + "'";
			MySqlDataReader Reader2 = Linq.ExecuteReader ();
			try {
				Reader2.Read ();
                int i = 0;
				playername = Reader2.GetString (i++);
				sceneID = int.Parse (Reader2.GetString (i++));
				player = ScriptableObject.CreateInstance<Data_PlayerFile_Sr> ();
				player.login = login;
				player.password = password;
				player.nick = playername;
				player.sceneID = sceneID;
                player.HPMax = int.Parse (Reader2.GetString (i++));
                player.PlayerScores = int.Parse (Reader2.GetString (i++));
				float x = float.Parse (Reader2.GetString (i++));
				float y = float.Parse (Reader2.GetString (i++));
				float z = float.Parse (Reader2.GetString (i++));
                player.zombie = int.Parse(Reader2.GetString(i++));
                player.zombieMutant = int.Parse(Reader2.GetString(i++));
                player.zombieStrong = int.Parse(Reader2.GetString(i++));
                player.title = Reader2.GetString(i++);
                player.rang = int.Parse(Reader2.GetString(i++));
                player.position.Set (x, y, z);
				Reader2.Close ();
				check = player;
			} catch (MySqlException ex) {
				Debug.Log (ex.ErrorCode + ex.Message);
			}
			Reader2.Close ();
		} catch (MySqlException ex) {
			Debug.Log (ex.ErrorCode + ex.Message);
		}

		Reader.Close ();
		return check;
	}
	
}
