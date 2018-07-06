using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading;
using System;

public class RoomsManager : MonoBehaviour
{
    static Vector3 nextRoomPosition = new Vector3(0, -10000, 0);
    static Vector3 roomUp = new Vector3(0, 100, 0);

    static List<GameObject> Weapons = new List<GameObject>();
    /// <summary>
    /// Rooms loaded on server
    /// </summary>
    static List<Room> RoomsLoad = new List<Room>();
    /// <summary>
    /// Free room id and position
    /// </summary>
    static List<FreeRoom> freeRoom = new List<FreeRoom>();
    static List<NetworkWriter> RoomsList = new List<NetworkWriter>();

    static List<NetworkWriter> result = new List<NetworkWriter>();
    static Thread CreateList;

    void Start()
    {
        LoadWeapon();
        InvokeRepeating("StartCreateRoomsList", 10, 10);
    }

    void Update()
    {
        if (CreateList != null)
        {
            if (!CreateList.IsAlive)
            {
                RoomsList.Clear();
                RoomsList.AddRange(result);
                result.Clear();
                CreateList = null;
            }
        }
    }

    public static int CreateRoom(string name, int map, string password = "")
    {
        FreeRoom freeRoom = GetFreeRoom();
        int roomID = -1;

        GameObject mapGo = Resources.Load<GameObject>("Maps/" + map);
        if (mapGo)
        {
            GameObject mapInst = (GameObject)Instantiate(mapGo, freeRoom.position, Quaternion.identity);
            Room room = mapInst.GetComponent<Room>();
            room.roomID = freeRoom.id;
            roomID = freeRoom.id;
            room._name = name;
            room.mapID = map;
            if (password != "")
            {
                room.SetRoomPassword(password);
            }
            RoomsLoad.Add(room);
            room.SpawnMob();
        }

        return roomID;
    }

    public static void PlayerConnectToRoom(int roomID, Data_PlayerFile_Sr data, GameObject obj)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            room.WritePlayer(data.sessionID, data, obj);
        }
    }

    public static void PlayerLeaveRoom(int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if(acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                room.DisconnectPlayer(acc.indexInRoom);
            }
        }
    }

    /// <summary>
    /// Destroy room by ID
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="roomID"></param>
    public static void DestroyItemInRoom(int itemID, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            room.RemoveItem(itemID);

            FreeRoom free = new FreeRoom();
            free.id = room.roomID;
            free.position = room.transform.position;

            freeRoom.Add(free);
            RoomsList[roomID] = null;
        }
    }

    public static void DestroyRoom(int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            room.DestroyRoom();
        }
    }

    public static void GetRoomsList(NetworkMessage netMsg)
    {
        if (Networking_OnConnect.AccountVerefication(netMsg))
        {
            foreach (NetworkWriter wr in RoomsList)
            {
                netMsg.conn.SendWriter(wr, 0);
            }
        }
    }

    public static void HandlerPlayerDisconnectAtRoom(NetworkMessage netmsg) {
        int sessionID = -1;
        if(Networking_OnConnect.AccountVerefication(netmsg, out sessionID))
        {
            AccountData data = Networking_OnConnect.GetAccountData(sessionID);

            if (data != null && data.roomID != -1)
            {
                Room room = GetRoom(data.roomID);

                if (room)
                {
                    room.DisconnectPlayer(data.indexInRoom);
                }
            }
        }
    } 

    static FreeRoom GetFreeRoom()
    {
        FreeRoom free = new FreeRoom();

        if (freeRoom.Count != 0)
        {
            free = freeRoom[0];
            freeRoom.RemoveAt(0);
        }
        else
        {
            nextRoomPosition = nextRoomPosition + roomUp;

            free.id = RoomsLoad.Count;
            free.position = nextRoomPosition;
        }

        return free;
    }

    public static Data_PlayerFile_Sr GetPlayerData(int sessionID)
    {
        Data_PlayerFile_Sr data = null;

        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            if (acc.roomID != -1 && acc.indexInRoom != -1)
            {
                Room room = GetRoom(acc.roomID);
                if (room)
                {
                    data = room.GetPlayerData(acc.indexInRoom);
                }
            }
        }

        return data;
    }

    public static Player_MovePlayer GetPlayerController(int sessionID)
    {
        Player_MovePlayer data = null;

        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            if (acc.roomID != -1 && acc.indexInRoom != -1)
            {
                Room room = GetRoom(acc.roomID);
                if (room)
                {
                    data = room.GetPlayerControll(acc.indexInRoom);
                }
            }
        }

        return data;
    }

    public static NetworkConnection GetPlayerConnection(int sessionID)
    {
        NetworkConnection data = null;

        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            data = acc.conn;
        }

        return data;
    }

    public static int GetPlayerConnectionID(int sessionID)
    {
        int data = -1;

        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            data = acc.conn.connectionId;
        }

        return data;
    }

    internal static Room GetRoom(int roomID)
    {
        Room room = null;

        if (roomID > -1 && RoomsLoad.Count > roomID)
        {
            room = RoomsLoad[roomID];
        }

        return room;
    }

    public static bool Disconnect(int connectionID)
    {
        bool yes = false;

        if (connectionID != -1)
        {
            int sessionID = Networking_OnConnect.FindSessionID(connectionID);
            if (sessionID != -1)
            {
                AccountData data = Networking_OnConnect.GetAccountData(sessionID);

                if (data.indexInRoom != -1 && data.roomID != -1)
                {
                    Room room = GetRoom(data.roomID);
                    if (room)
                    {
                        room.DisconnectPlayer(data.indexInRoom);
                    }
                }
                Networking_OnConnect.DisconnectPlayer(sessionID);
            }
        }

        return yes;
    }

    public static void HandlerReady(NetworkMessage netmsg)
    {
        Message_Sr.PlayerSetReady ready = netmsg.ReadMessage<Message_Sr.PlayerSetReady>();
        if (Networking_OnConnect.AccountVerefication(ready.id, ready.log, ready.pass))
        {
            Data_PlayerFile_Sr data = GetPlayerData(ready.id);

            if (data)
            {
                data.PlayerReady = true;
                data.SetHP(data.HPMax);
            }
        }
    }

    #region Weapon Event
    void LoadWeapon()
    {
        GameObject go = Resources.Load<GameObject>("Weapons/Machine");
        if (go)
        {
            Weapons.Add(go);
        }
        else
        {
            new NotImplementedException("Not find weapon model!");
        }
        go = Resources.Load<GameObject>("Weapons/ShotGun");
        if (go)
        {
            Weapons.Add(go);
        }
        else
        {
            new NotImplementedException("Not find weapon model!");
        }
        go = Resources.Load<GameObject>("Weapons/Rifle");
        if (go)
        {
            Weapons.Add(go);
        }
        else
        {
            new NotImplementedException("Not find weapon model!");
        }
        go = Resources.Load<GameObject>("Weapons/GaussGun");
        if (go)
        {
            Weapons.Add(go);
        }
        else
        {
            new NotImplementedException("Not find weapon model!");
        }
    }

    public static Player_Weapon_Sr InstantiateWeapon(weaponType_Sr type, int roomID)
    {
        Player_Weapon_Sr weap = null;
        GameObject go = null;

        switch (type)
        {
            case weaponType_Sr.Machine:
                go = Instantiate(Weapons[0]);
                break;
            case weaponType_Sr.ShotGun:
                go = Instantiate(Weapons[1]);
                break;
            case weaponType_Sr.Rifle:
                go = Instantiate(Weapons[2]);
                break;
            case weaponType_Sr.GaussGun:
                go = Instantiate(Weapons[3]);
                break;
        }

        if (go)
        {
            weap = go.GetComponent<Player_Weapon_Sr>();
            Room room = GetRoom(roomID);
            if (room)
            {
                room.AddNewItemOnScene(weap);
            }
            SendInstantiateWeapon(weap);
        }

        return weap;
    }

    public static Player_Weapon_Sr InstantiateWeapon(weaponType_Sr type, Vector3 position, int roomID)
    {
        Player_Weapon_Sr weap = null;
        GameObject go = null;

        switch (type)
        {
            case weaponType_Sr.Machine:
                go = (GameObject)Instantiate(Weapons[0], position, Quaternion.identity);
                break;
            case weaponType_Sr.ShotGun:
                go = (GameObject)Instantiate(Weapons[1], position, Quaternion.identity);
                break;
            case weaponType_Sr.Rifle:
                go = (GameObject)Instantiate(Weapons[2], position, Quaternion.identity);
                break;
            case weaponType_Sr.GaussGun:
                go = (GameObject)Instantiate(Weapons[3], position, Quaternion.identity);
                break;
        }

        if (go)
        {
            weap = go.GetComponent<Player_Weapon_Sr>();
            Room room = GetRoom(roomID);
            if (room)
            {
                room.AddNewItemOnScene(weap);
            }
            SendInstantiateWeapon(weap);
        }

        return weap;
    }

    public static void SendInstantiateWeapon(Player_Weapon_Sr weapon)
    {
        Message_Sr.LoadItem_Sr item = new Message_Sr.LoadItem_Sr();
        item.item = weapon;
        SendReliableToRoom(Networking_msgType_Sr.NewItemOnScene, item, weapon.roomID);
    }
    #endregion

    #region Send Message
    /// <summary>
    /// Send reliable message to all players in room
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="roomID"></param>
    public static void SendReliableToRoom(short msgType, MessageBase msg, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            foreach (PlayerData data in room.playersData)
            {
                if (data != null)
                {
                    if (data.playerData.PlayerReady)
                    {
                        NetworkConnection conn = GetPlayerConnection(data.sessionID);

                        if (conn != null && conn.connectionId != -1)
                        {
                            conn.Send(msgType, msg);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send message to all players in room by channel 0
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="roomID"></param>
    public static void SendReliableToRoom(NetworkWriter msg, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            foreach (PlayerData data in room.playersData)
            {
                if (data != null)
                {
                    if (data.playerData.PlayerReady)
                    {
                        NetworkConnection conn = GetPlayerConnection(data.sessionID);

                        if (conn != null && conn.connectionId != -1)
                        {
                            conn.SendWriter(msg, 0);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send reliable message to all in room of this player
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendReliableAtRoom(short msgType, MessageBase msg, int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                foreach (PlayerData data in room.playersData)
                {
                    if (data != null)
                    {
                        if (data.playerData.PlayerReady)
                        {
                            NetworkConnection conn = GetPlayerConnection(data.sessionID);

                            if (conn != null && conn.connectionId != -1)
                            {
                                conn.Send(msgType, msg);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send message to all in room of this player by channel 0
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendReliableAtRoom(NetworkWriter msg, int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                foreach (PlayerData data in room.playersData)
                {
                    if (data != null)
                    {
                        if (data.playerData.PlayerReady)
                        {
                            NetworkConnection conn = GetPlayerConnection(data.sessionID);

                            if (conn != null && conn.connectionId != -1)
                            {
                                conn.SendWriter(msg, 0);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send reliable message to all other players in room of this player
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendToAllOtherPlayer(short msgType, MessageBase msg, int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                foreach (PlayerData data in room.playersData)
                {
                    if (data != null)
                    {
                        if (data.playerData.PlayerReady)
                        {
                            if (data.sessionID != sessionID)
                            {
                                NetworkConnection conn = GetPlayerConnection(data.sessionID);

                                if (conn != null && conn.connectionId != -1)
                                {
                                    conn.Send(msgType, msg);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send reliable message to this player
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendToThisPlayer(short msgType, MessageBase msg, int sessionID)
    {
        NetworkConnection conn = GetPlayerConnection(sessionID);

        if (conn != null && conn.connectionId != -1)
        {
            conn.Send(msgType, msg);
        }
    }

    /// <summary>
    /// Send message to this player by channel 0
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendToThisPlayer(NetworkWriter msg, int sessionID)
    {
        NetworkConnection conn = GetPlayerConnection(sessionID);

        if (conn != null && conn.connectionId != -1)
        {
            conn.SendWriter(msg, 0);
        }
    }

    /// <summary>
    /// Send int value by reliable channel 0
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="value"></param>
    /// <param name="sessionID"></param>
    public static void SendInt(short msgType, int value, int sessionID)
    {
        NetworkConnection conn = GetPlayerConnection(sessionID);

        if (conn != null)
        {
            NetworkWriter wr = new NetworkWriter();
            wr.StartMessage(msgType);
            wr.Write(value);
            wr.FinishMessage();
            conn.SendWriter(wr, 0);
        }
    }

    /// <summary>
    /// Send int value by reliable channel 0 to all room
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="value"></param>
    /// <param name="sessionID"></param>
    public static void SendIntToAllRoom(short msgType, int value, int roomID)
    {
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(msgType);
        wr.Write(value);
        wr.FinishMessage();

        Room room = GetRoom(roomID);

        if (room)
        {
            foreach (PlayerData data in room.playersData)
            {
                if (data != null)
                {
                    if (data.playerData.PlayerReady)
                    {
                        NetworkConnection conn = GetPlayerConnection(data.sessionID);

                        if (conn != null && conn.connectionId != -1)
                        {
                            conn.SendWriter(wr, 0);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send unreliable message to all players in room
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="roomID"></param>
    public static void SendUnreliableToRoom(short msgType, MessageBase msg, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            foreach (PlayerData data in room.playersData)
            {
                if (data != null)
                {
                    if (data.playerData.PlayerReady)
                    {
                        NetworkConnection conn = GetPlayerConnection(data.sessionID);

                        if (conn != null && conn.connectionId != -1)
                        {
                            conn.SendUnreliable(msgType, msg);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send message to all players in room by channel 1
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="roomID"></param>
    public static void SendUnreliableToRoom(NetworkWriter msg, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            foreach (PlayerData data in room.playersData)
            {
                if (data != null)
                {
                    if (data.playerData.PlayerReady)
                    {
                        NetworkConnection conn = GetPlayerConnection(data.sessionID);

                        if (conn != null && conn.connectionId != -1)
                        {
                            conn.SendWriter(msg, 1);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send unreliable message to all in room of this player
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendUnreliableAtRoom(short msgType, MessageBase msg, int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                foreach (PlayerData data in room.playersData)
                {
                    if (data != null)
                    {
                        if (data.playerData.PlayerReady)
                        {
                            NetworkConnection conn = GetPlayerConnection(data.sessionID);

                            if (conn != null && conn.connectionId != -1)
                            {
                                conn.SendUnreliable(msgType, msg);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send message to all in room of this player by channel 1
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sessionID"></param>
    public static void SendUnreliableAtRoom(NetworkWriter msg, int sessionID)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);

        if (acc != null)
        {
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                foreach (PlayerData data in room.playersData)
                {
                    if (data != null)
                    {
                        if (data.playerData.PlayerReady)
                        {
                            NetworkConnection conn = GetPlayerConnection(data.sessionID);

                            if (conn != null && conn.connectionId != -1)
                            {
                                conn.SendWriter(msg, 1);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Start thread to create rooms list for send players
    /// </summary>
    void StartCreateRoomsList()
    {
        Room[] rooms = RoomsLoad.ToArray();

        CreateList = new Thread(CreateRoomsList);
        CreateList.Start(rooms);
    }

    /// <summary>
    /// Using in thread
    /// </summary>
    /// <param name="obj"></param>
    static void CreateRoomsList(object obj)
    {
        Room[] rooms = (Room[])obj;

        int w = 0;
        NetworkWriter wr = new NetworkWriter();
        NetworkWriter constructor = new NetworkWriter();
        byte[] buffer;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (w == 10)
            {
                wr.StartMessage(Networking_msgType_Sr.RoomList);
                wr.Write(w);
                buffer = constructor.ToArray();
                foreach (byte b in buffer)
                {
                    wr.Write(b);
                }
                wr.FinishMessage();
                result.Add(wr);
                w = 0;
            }

            if (w == 0)
            {
                wr = new NetworkWriter();
                constructor = new NetworkWriter();
            }

            if (rooms[i] != null)
            {
                if (rooms[i].maxPlayerNumber > rooms[i].playerNumber)
                {
                    Room room = rooms[i];
                    constructor.Write(i);
                    constructor.Write(room._name);
                    constructor.Write(room.mapID);
                    constructor.Write((short)room.playerNumber);
                    constructor.Write((short)room.maxPlayerNumber);
                    if (room.password)
                    {
                        constructor.Write(true);
                    }
                    else
                    {
                        constructor.Write(false);
                    }

                    w++;
                }
            }

            if (i + 1 == rooms.Length)
            {
                wr.StartMessage(Networking_msgType_Sr.RoomList);
                wr.Write(w);
                buffer = constructor.ToArray();
                foreach (byte b in buffer)
                {
                    wr.Write(b);
                }
                wr.FinishMessage();
                result.Add(wr);
            }
        }
    }
}

public struct FreeRoom
{
    public int id;
    public Vector3 position;

    public FreeRoom(int i, Vector3 p)
    {
        id = i;
        position = p;
    }
}
