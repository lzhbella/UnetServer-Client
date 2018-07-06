using UnityEngine;
using UnityEngine.Networking;

public class Networking_OnPlayerSelect : RoomsManager
{

    //If a player has selected a character check his data and send him to his character data
    static public void HandlerJoinToRoom(NetworkMessage netmsg)
    {
        Message_Sr.PlayerJoinToRoom_Sr join = netmsg.ReadMessage<Message_Sr.PlayerJoinToRoom_Sr>();
        if (Networking_OnConnect.AccountVerefication(join.sessionID, join.Login, join.Password))
        {
            Data_PlayerFile_Sr player = SQL_PlayerVerefy.CheckLP(join.Login, join.Password, join.playerNick);
            if (player)
            {
                AccountData acc = Networking_OnConnect.GetAccountData(join.sessionID);

                if (acc != null)
                {
                    if (acc.roomID == -1 && acc.indexInRoom == -1)
                    {
                        player.sessionID = join.sessionID;

                        Room room = GetRoom(join.roomID);
                        if (room)
                        {
                            bool connect = true;
                            if (room.password)
                            {
                                if (!room.CheckPassword(join.roomPassword))
                                {
                                    connect = false;
                                    Message_Sr.PlayerJoinToRoom_Sr error = new Message_Sr.PlayerJoinToRoom_Sr();
                                    error.errorMsg = "Password not correct";
                                    netmsg.conn.Send(Networking_msgType_Sr.JoinToRoom, error);
                                }
                            }
                            if(room.maxPlayerNumber == room.playerNumber)
                            {
                                connect = false;
                                Message_Sr.PlayerJoinToRoom_Sr error = new Message_Sr.PlayerJoinToRoom_Sr();
                                error.errorMsg = "Room have maximum players number";
                                netmsg.conn.Send(Networking_msgType_Sr.JoinToRoom, error);
                            }
                            if (connect)
                            {
                                SendRoomInstantiate(room.roomID, join.sessionID);
                                PlayerLoad(netmsg.conn, player, join.roomID);
                                Networking_PlayerListSend.OnPlayerList(netmsg.conn, join.roomID);
                                Networking_PlayerListSend.SendItems(netmsg.conn, join.sessionID);
                            }
                        }
                    }
                }
            }
            else
            {
                Disconnect(netmsg.conn.connectionId);
            }
        }
    }

    public static void HandlerCreateRoom(NetworkMessage netmsg)
    {
        Message_Sr.PlayerCreateRoom_Sr create = netmsg.ReadMessage<Message_Sr.PlayerCreateRoom_Sr>();
        if (Networking_OnConnect.AccountVerefication(create.sessionID, create.Login, create.Password))
        {
            Data_PlayerFile_Sr player = SQL_PlayerVerefy.CheckLP(create.Login, create.Password, create.playerNick);
            if (player)
            { AccountData acc = Networking_OnConnect.GetAccountData(create.sessionID);

                if (acc != null)
                {
                    if (acc.roomID == -1 && acc.indexInRoom == -1)
                    {
                        if (create.roomName.Length >= 5)
                        {
                            player.sessionID = create.sessionID;
                            int roomID = -1;

                            if (create.pass && !string.IsNullOrEmpty(create.roomPassword) && create.roomPassword.Length >= 4)
                            {
                                roomID = CreateRoom(create.roomName, create.mapID, create.roomPassword);
                            }
                            else if (!create.pass)
                            {
                                roomID = CreateRoom(create.roomName, create.mapID);
                            }

                            if (roomID != -1)
                            {
                                SendRoomInstantiate(roomID, create.sessionID);
                                PlayerLoad(netmsg.conn, player, roomID);
                                Networking_PlayerListSend.OnPlayerList(netmsg.conn, roomID);
                                Networking_PlayerListSend.SendItems(netmsg.conn, create.sessionID);
                            }
                        }
                    }
                }
            }
            else
            {
                Disconnect(netmsg.conn.connectionId);
            }
        }
    }

    //Add new player on map
    static void PlayerLoad(NetworkConnection con, Data_PlayerFile_Sr data, int roomID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            GameObject go = (GameObject)Instantiate((GameObject)Resources.Load("Player"), room.spawnPosition, Quaternion.identity);
            go.name = data.nick;
            Player_MovePlayer move = go.GetComponent<Player_MovePlayer>();
            move.playerCon = con;
            PlayerConnectToRoom(roomID, data, go);

            Message_Sr.NewPlayerOnScene_Sr newPL = new Message_Sr.NewPlayerOnScene_Sr();
            newPL.player = GetPlayerData(data.sessionID);
            SendReliableToRoom(Networking_msgType_Sr.NewPlayerConnnectOnScene, newPL, roomID);

            Player_Weapon_Sr weap = InstantiateWeapon(weaponType_Sr.GaussGun, roomID);
            if (weap)
            {
                weap.TakeWeapon(move.weapon, data.sessionID);
            }
        }
    }

    static void SendRoomInstantiate(int roomID, int sessionID)
    {
        Room room = GetRoom(roomID);

        if (room)
        {
            Message_Sr.RoomInstantiate instance = new Message_Sr.RoomInstantiate();
            instance.mapID = room.mapID;
            instance.position = room.transform.position;

            NetworkConnection conn = GetPlayerConnection(sessionID);

            if (conn != null)
            {
                conn.Send(Networking_msgType_Sr.MapInstantiate, instance);
            }
        }
    }

}
