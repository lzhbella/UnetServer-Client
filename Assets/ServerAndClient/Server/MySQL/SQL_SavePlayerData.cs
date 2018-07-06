using UnityEngine;
using MySql.Data.MySqlClient;
using UnityEngine.Networking;

public class SQL_SavePlayerData
{

	private static MySqlCommand Linq = SQL_sqlConnect.Linq;

    const int soldier = 2;
    static int coolSoldier;
    static int cpl;
    static int sgt;
    static int staffSgt;
    static int coolSgt;
    static int masterSgt;
    static int firstSgt;
    static int majorSgt;
    static int majorSgtCommand;
    static int SeniorSgt;

    //Save player kills and rang to DB
    public static void Kill (int sessionID, string zombie)
	{
        Data_PlayerFile_Sr player = RoomsManager.GetPlayerData(sessionID);

        if ("strongzombie" == zombie)
        {
            player.zombieStrong++;
        }
        else if ("mutantzombie" == zombie)
        {
            player.zombieMutant++;
        }
        else if ("zombie" == zombie || "sickzombie" == zombie || "policezombie" == zombie)
        {
            player.zombie++;
        }

        player.PlayerScores = player.zombie + (player.zombieMutant * 2) + (player.zombieStrong * 3);

        if (player.rang != 11)
        {
            int rang = CheckRang(player.PlayerScores);

            if (rang > player.rang)
            {
                player.rang = rang;
                NetworkWriter wr = new NetworkWriter();
                wr.StartMessage(Networking_msgType_Sr.Rang);
                wr.Write(sessionID);
                wr.Write(player.rang);
                wr.FinishMessage();
                RoomsManager.SendReliableAtRoom(wr, sessionID);

                string title = SetTitle(player.rang);
                if (!string.IsNullOrEmpty(title))
                {
                    player.title = title;
                    wr = new NetworkWriter();
                    wr.StartMessage(Networking_msgType_Sr.Title);
                    wr.Write(sessionID);
                    wr.Write(title);
                    wr.FinishMessage();
                    RoomsManager.SendReliableAtRoom(wr, sessionID);
                }
            }
        }

		Linq.CommandText = string.Format("UPDATE charecter SET zombie = '{0}', zombie_mutant = '{1}', zombie_strong = '{2}', PlayerScores = '{3}', rang = '{4}', title = '{5}' WHERE PlayerName = '{6}'", player.zombie, player.zombieMutant, player.zombieStrong, player.PlayerScores, player.rang, player.title, player.nick);
		MySqlDataReader Reader = Linq.ExecuteReader ();
		try {
			Reader.Read ();
			Reader.Close ();
		} catch (MySqlException ex) {
			Debug.Log (ex.ErrorCode + ex.Message);
		}
		Reader.Close ();
	}

    public static void CalculateRangs()
    {
        coolSoldier = soldier * 2;
        cpl = coolSoldier * 2;
        sgt = cpl * 2;
        staffSgt = sgt * 2;
        coolSgt = staffSgt * 2;
        masterSgt = coolSgt * 2;
        firstSgt = masterSgt * 2;
        majorSgt = firstSgt * 2;
        majorSgtCommand = majorSgt * 2;
        SeniorSgt = majorSgtCommand * 2;
    }

    static int CheckRang(int score)
    {
        int rang = 0;

        if(score >= SeniorSgt)
        {
            rang = 11;
        }else if(score >= majorSgtCommand)
        {
            rang = 10;
        }
        else if (score >= majorSgt)
        {
            rang = 9;
        }
        else if (score >= firstSgt)
        {
            rang = 8;
        }
        else if (score >= masterSgt)
        {
            rang = 7;
        }
        else if (score >= coolSgt)
        {
            rang = 6;
        }
        else if (score >= staffSgt)
        {
            rang = 5;
        }
        else if (score >= sgt)
        {
            rang = 4;
        }
        else if (score >= cpl)
        {
            rang = 3;
        }
        else if (score >= coolSoldier)
        {
            rang = 2;
        }
        else if (score >= soldier)
        {
            rang = 1;
        }

        return rang;
    }

    static string SetTitle(int rang)
    {
        string title = " ";

        switch (rang)
        {
            case 1:
                title = "Soldier";
                break;
            case 2:
                title = "Cool Soldier";
                break;
            case 3:
                title = "Cpl.";
                break;
            case 4:
                title = "Sgt.";
                break;
            case 5:
                title = "Staff Sgt.";
                break;
            case 6:
                title = "Cool Sgt.";
                break;
            case 7:
                title = "Master Sgt.";
                break;
            case 8:
                title = "First Sgt.";
                break;
            case 9:
                title = "Major Sgt.";
                break;
            case 10:
                title = "Major Sgt. Command";
                break;
            case 11:
                title = "Senior Sgt.";
                break;
        }

        return title;
    }
}
