using UnityEngine;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

//This script registration new account

public class SQL_AccountRegistration
{
	private static MySqlCommand Linq = SQL_sqlConnect.Linq;
	static string log;
	static string pas;
	static string mail;
	static char m = '@';
	static char t = '.';
	static int minLogSymbol = 3;
	static int minPassSymbol = 4;
	static int NickSymbolMin = 4;
    public const int maxCharInAccount = 3;

	//Answers
	static string mailNotAvaileble = "This e-mail address not available.";
	static string LoginNotAvaileble = "This Login name not available.";
	static string mailNotCorrect = "This e-mail address not correct.";
	static string minLog = "Minimum Login length " + minLogSymbol.ToString () + " symbol.";
	static string minPass = "Minimum Password length " + minPassSymbol.ToString () + " symbol.";
	static string Succsess = "The account was created successfully. Enjoy the game.";

	//Charecter craet
	static string minNickSymbol = "Minimum nick length " + NickSymbolMin.ToString () + " symbol.";
	static string notValidAcc = "Account not valid.";
	static string busyNick = "This nick name is busy.";
	static string maxChars = "You have the maximum number of characters on account.";
	static string characterCreated = "Succsessful create character.";

    static NetworkConnection temp;

	public static void CreatNewAccount (NetworkMessage netmsg)
	{

		Message_Sr.Registration_Sr reg = netmsg.ReadMessage<Message_Sr.Registration_Sr> ();
		log = reg.log;
		pas = reg.pass;
		mail = reg.mail;
        temp = netmsg.conn;


		if (log.Length >= minLogSymbol) {
			if (pas.Length >= minPassSymbol) {
				if (mail.IndexOf (m) != -1 && mail.IndexOf (t) != -1) {
					Linq.CommandText = "SELECT Mail FROM accountlist WHERE Mail = '" + mail + "'";
					MySqlDataReader Reader = Linq.ExecuteReader ();
					try {
						if (Reader.Read ()) {
                            RegistrationReaply(mailNotAvaileble);
							Reader.Close ();
						} else {
							Reader.Close ();

							Linq.CommandText = "SELECT AccountName FROM accountlist WHERE AccountName = '" + log + "'";
							MySqlDataReader Reader2 = Linq.ExecuteReader ();
							try {
								if (Reader2.Read ()) {
                                    RegistrationReaply(LoginNotAvaileble);
									Reader2.Close ();
								} else {
									Reader2.Close ();
									try {
										Linq.CommandText = "INSERT INTO accountlist (AccountName, PasswordAc, Mail) VALUES ('" + log + "','" + pas + "','" + mail + "')";
										int row = Linq.ExecuteNonQuery ();
										Debug.Log ("Succsess create new account '" + log + "' " + row + ".");
										
                                        RegistrationReaply(Succsess);
									} catch (MySqlException e) {
										Debug.Log (e.Number);
									}
								}
							} catch (MySqlException ex) {
								Reader2.Close ();
								Debug.Log (ex.Number);
							}
						}

					} catch (MySqlException ex) {
						Reader.Close ();
						Debug.Log (ex.Number);
					}
				} else {
                    RegistrationReaply(mailNotCorrect);
				}
			} else {
                RegistrationReaply(minPass);
			}
		} else {
            RegistrationReaply(minLog);
		}
	}

    static void RegistrationReaply(string textMsg)
    {
        Message_Sr.Registration_Sr reaply = new Message_Sr.Registration_Sr();
        reaply.reg = false;
        reaply.msg = textMsg;
        temp.Send(Networking_msgType_Sr.Registration, reaply);
    }

	//Add new char on account
	public static void CreateChar (NetworkMessage netmsg)
	{
		Message_Sr.CreateChar character = netmsg.ReadMessage<Message_Sr.CreateChar> ();
        if (Networking_OnConnect.AccountVerefication(character.sessionID, character.log, character.pass))
        {
            if (character.nick.Length < NickSymbolMin)
            {
                Message_Sr.CreateChar create = new Message_Sr.CreateChar();
                create.msg = minNickSymbol;
                netmsg.conn.Send(Networking_msgType_Sr.CharCreate, create);
            }
            else
            {
                Linq.CommandText = "SELECT id FROM accountlist WHERE AccountName='" + character.log + "' AND PasswordAc = '" + character.pass + "'";
                MySqlDataReader read = Linq.ExecuteReader();

                if (read.Read())
                {
                    int ID = int.Parse(read.GetString(0));
                    read.Close();
                    Linq.CommandText = "SELECT COUNT(*) FROM charecter WHERE PlayerName = '" + character.nick + "'";
                    read = Linq.ExecuteReader();

                    if (read.Read())
                    {

                        if (int.Parse(read.GetString(0)) == 1)
                        {
                            read.Close();
                            Message_Sr.CreateChar create = new Message_Sr.CreateChar();
                            create.msg = busyNick;
                            netmsg.conn.Send(Networking_msgType_Sr.CharCreate, create);
                        }
                        else
                        {
                            read.Close();
                            Linq.CommandText = "SELECT COUNT(*) FROM charecter WHERE account_id = '" + ID.ToString() + "'";
                            read = Linq.ExecuteReader();

                            if (read.Read())
                            {
                                int numberChars = int.Parse(read.GetString(0));
                                read.Close();

                                if (numberChars < maxCharInAccount)
                                {
                                    Linq.CommandText = "INSERT INTO charecter (account_id, PlayerName, MaxHP, PlayerScores, scene_ID, x, y, z) VALUES (" + ID.ToString() + ", '" + character.nick + "', 100, 0, 0, '5', '120', '5')";
                                    int row = Linq.ExecuteNonQuery();
                                    Debug.Log("Succsess create new character '" + character.nick + "' " + row + ".");
                                    Message_Sr.CreateChar create = new Message_Sr.CreateChar();
                                    create.msg = characterCreated;
                                    netmsg.conn.Send(Networking_msgType_Sr.CharCreate, create);
                                    List<Data_PlayerFile_Sr> list = SQL_FindLogPass.CharDataGet(ID.ToString());
                                    Message_Sr.CharData data = new Message_Sr.CharData();
                                    data.index = character.sessionID;
                                    data.players = list;
                                    netmsg.conn.Send(Networking_msgType_Sr.PlayerDataGet, data.Serialize());
                                }
                                else
                                {
                                    Message_Sr.CreateChar create = new Message_Sr.CreateChar();
                                    create.msg = maxChars;
                                    netmsg.conn.Send(Networking_msgType_Sr.CharCreate, create);
                                }

                            }
                            else
                            {
                                Message_Sr.CreateChar create = new Message_Sr.CreateChar();
                                create.msg = notValidAcc;
                                netmsg.conn.Send(Networking_msgType_Sr.CharCreate, create);
                            }
                        }
                    }
                }
            }
        }
	}
}
