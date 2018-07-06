using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Networking_OnConnect : MonoBehaviour
{
    public static NetworkConnection tempConn = new NetworkConnection();

    static List<Data_PlayerFile_Sr> data = new List<Data_PlayerFile_Sr>();
    static List<AccountData> Accounts = new List<AccountData>();
    static Dictionary<int, bool> AccountOnline = new Dictionary<int, bool>();
    static Dictionary<int, int> ConnectionIDList = new Dictionary<int, int>();
    static List<int> freeSlots = new List<int>();

    static public void LoginGet(NetworkMessage netMsg)
    {
        tempConn = netMsg.conn;
        Message_Sr.LoginSendMess_Sr Aut = netMsg.ReadMessage<Message_Sr.LoginSendMess_Sr>();
        int accID = -1;
        if (SQL_FindLogPass.CheckLP(Aut.log, Aut.pass, out data, out accID))
        {
            AccountData acc = new AccountData();
            acc.conn = netMsg.conn;
            acc.login = Aut.log;
            acc.password = Aut.pass;
            acc.accountID = accID;

            int sessionID = -1;
            if (freeSlots.Count > 0)
            {
                sessionID = freeSlots[0];
                freeSlots.RemoveAt(0);
                Accounts[sessionID] = acc;
            }
            else
            {
                sessionID = Accounts.Count;
                Accounts.Add(acc);
            }
            ConnectionIDList.Add(netMsg.conn.connectionId, sessionID);
            AccountOnline.Add(accID, true);

            Message_Sr.CharData charsList = new Message_Sr.CharData();
            charsList.index = sessionID;
            charsList.players = data;
            netMsg.conn.Send(Networking_msgType_Sr.PlayerDataGet, charsList.Serialize());
        }
    }

    public static int BotAdd(int roomID, int indexInRoom)
    {
        AccountData acc = new AccountData();
        acc.roomID = roomID;
        acc.indexInRoom = indexInRoom;

        int sessionID = -1;
        if (freeSlots.Count > 0)
        {
            sessionID = freeSlots[0];
            freeSlots.RemoveAt(0);
            Accounts[sessionID] = acc;
        }
        else
        {
            sessionID = Accounts.Count;
            Accounts.Add(acc);
        }

        return sessionID;
    }

    /// <summary>
    /// Check account in online account list
    /// </summary>
    /// <param name="accID"></param>
    /// <returns></returns>
    public static bool CheckOnlineAccount(int accID)
    {
        bool yes = false;

        if (AccountOnline.ContainsKey(accID))
        {
            yes = true;
        }

        return yes;
    }

    /// <summary>
    /// If account data equal account data in list return true
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool AccountVerefication(int sessionID, string login, string password)
    {
        bool yes = false;

        if (Accounts.Count > sessionID && sessionID > -1)
        {
            AccountData data = Accounts[sessionID];
            if (data != null)
            {
                if (data.login == login && data.password == password)
                {
                    yes = true;
                }
            }
        }

        return yes;
    }

    /// <summary>
    /// If message have only verefication data
    /// </summary>
    /// <param name="netMsg"></param>
    /// <returns></returns>
    public static bool AccountVerefication(NetworkMessage netMsg, out int sessionID)
    {
        bool yes = false;
        sessionID = -1;

        try
        {
            sessionID = netMsg.reader.ReadInt32();
            string login = netMsg.reader.ReadString();
            string password = netMsg.reader.ReadString();

            if (Accounts.Count > sessionID && sessionID > -1)
            {
                AccountData data = Accounts[sessionID];
                if (data != null)
                {
                    if (data.login == login && data.password == password)
                    {
                        yes = true;
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Account verefication: netwok data damaged");
        }

        return yes;
    }

    public static bool AccountVerefication(NetworkMessage netMsg)
    {
        bool yes = false;

        try
        {
            int sessionID = netMsg.reader.ReadInt32();
            string login = netMsg.reader.ReadString();
            string password = netMsg.reader.ReadString();

            if (Accounts.Count > sessionID && sessionID > -1)
            {
                AccountData data = Accounts[sessionID];
                if (data != null)
                {
                    if (data.login == login && data.password == password)
                    {
                        yes = true;
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Account verefication: netwok data damaged");
        }

        return yes;
    }

    public static int FindSessionID(int connectionID)
    {
        int sessionID = -1;

        ConnectionIDList.TryGetValue(connectionID, out sessionID);

        return sessionID;
    }

    public static AccountData GetAccountData(int sessionID)
    {
        AccountData acc = null;

        if (Accounts.Count > sessionID && sessionID > -1)
        {
            acc = Accounts[sessionID];
        }

        return acc;
    }

    public static void DisconnectPlayer(int sessionID)
    {
        AccountData acc = null;

        if (Accounts.Count > sessionID && sessionID > -1)
        {
            acc = Accounts[sessionID];

            if(acc != null)
            {
                AccountOnline.Remove(acc.accountID);
                ConnectionIDList.Remove(acc.conn.connectionId);
            }
            Accounts[sessionID] = null;

            freeSlots.Add(sessionID);
        }
    }

    static public void NoAccount()
    {
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.NoAccount);
        wr.Write(0);
        wr.FinishMessage();
        tempConn.SendWriter(wr, 0);
        tempConn = null;
    }

    public static void AccountUsed()
    {
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.AccountOnline);
        wr.Write("Account currently used.");
        wr.FinishMessage();
        tempConn.SendWriter(wr, 0);
        tempConn = null;
    }
}

public class AccountData
{
    public NetworkConnection conn = null;
    public string login = string.Empty;
    public string password = string.Empty;
    public int accountID = -1;
    public int roomID = -1;
    public int indexInRoom = -1;
}
