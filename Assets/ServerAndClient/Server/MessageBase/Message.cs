using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Message_Sr
{

    public class Reload_Sr : MessageBase
    {
        public int index;
        public double reloadTime;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(index);
            writer.Write(reloadTime);
        }
    }

        public class LoadItem_Sr : MessageBase
    {
        public Player_Item_Sr item;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(item.index);
            writer.Write((int)item.ItemType);
            switch (item.ItemType)
            {
                case ItemType_Sr.weapon:
                    Player_Weapon_Sr weap = (Player_Weapon_Sr)item;
                    writer.Write((int)weap.WeaponType);
                    writer.Write(weap.Dropped);
                    if (weap.Dropped)
                    {
                        writer.Write(weap.transform.position);
                        writer.Write(weap.transform.rotation);
                    }
                    else
                    {
                        writer.Write(weap.ShoterIndex);
                    }
                    break;
            }
        }
    }

    public class Shot_Sr : MessageBase
    {
        public int index;
        public BulletType_Sr type;
        public Vector3[] vect;
        public bool thisPlayer;
        public int ammo;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(index);
            writer.Write((int)type);
            writer.Write(vect.Length);
            if (vect.Length > 0)
            {
                foreach(Vector3 v in vect)
                {
                    writer.Write(v);
                }
            }
            writer.Write(thisPlayer);
            if (thisPlayer)
            {
                writer.Write(ammo);
            }
        }

    }

    public class DropWeapon_Sr : MessageBase
    {

        //Player index
        public int index;
        public string log;
        public string pass;
        public int indexItem;

        public override void Serialize(NetworkWriter write)
        {
            write.Write(index);
            write.Write(indexItem);
        }

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
        }
    }

    public class PickUpWeapon_Sr : MessageBase
    {

        //Player index
        public int index;
        public string log;
        public string pass;
        public int indexItem;
        public int ammo;
        public int ammoMax;
        public bool thisPlayer;

        public override void Serialize(NetworkWriter write)
        {
            write.Write(index);
            write.Write(indexItem);
            write.Write(thisPlayer);
            if (thisPlayer)
            {
                write.Write(ammo);
                write.Write(ammoMax);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
            indexItem = reader.ReadInt32();
        }
    }


    public class CharData
    {
        public int index;
        public List<Data_PlayerFile_Sr> players;

        public CharData_Msg Serialize()
        {
            NetworkWriter write = new NetworkWriter();
            write.Write(index);
            write.Write(players.Count);
            for (int p = 0; p < players.Count; p++)
            {
                write.Write(players[p].nick);
                write.Write(players[p].sessionID);
                write.Write(players[p].sceneID);
                write.Write(players[p].HPMax);
            }

            CharData_Msg data = new CharData_Msg();
            data.array = write.ToArray();

            return data;
        }

        public class CharData_Msg : MessageBase
        {
            public byte[] array;

            public override void Serialize(NetworkWriter writer)
            {
                foreach(byte b in array)
                {
                    writer.Write(b);
                }
            }
        }
    }


    public class Chat_Sr : MessageBase
    {

        //read
        public int index;
        public string log;
        public string pass;
        public int msgType;
        public int indexPriv;
        public string nick;
        public string msg;

        //write
        public int msgTypeW;
        public string nickW;
        public string msgW;
        public int id;

        public override void Serialize(NetworkWriter write)
        {
            write.Write(msgTypeW);
            write.Write(nickW);
            write.Write(id);
            write.Write(msgW);
        }

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
            msgType = reader.ReadInt32();
            //In the ChatMessage enum 2 == "privat"
            if (msgType == 2)
            {
                indexPriv = reader.ReadInt32();
                nick = reader.ReadString();
            }
            msg = reader.ReadString();
        }
    }


    public class CreateChar : MessageBase
    {
        //read
        public int sessionID;
        public string log;
        public string pass;
        public string nick;

        //write
        public string msg;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(msg);
        }

        public override void Deserialize(NetworkReader reader)
        {
            sessionID = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
            nick = reader.ReadString();
        }
    }


    public class DisconnectPlayer : MessageBase
    {

        public int ID;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(ID);
        }
    }


    public class LoginSendMess_Sr : MessageBase
    {

        public string log;
        public string pass;

        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            log = reader.ReadString();
            pass = reader.ReadString();
        }
    }


    public class MouseButton_Sr : MessageBase
    {

        public bool down;
        public int index;
        public string log;
        public string pass;


        public override void Deserialize(NetworkReader reader)
        {
            down = reader.ReadBoolean();
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
        }
    }


    public class NewPlayerOnScene_Sr : MessageBase
    {
        public Vector3 position;
        public Data_PlayerFile_Sr player;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(player.sessionID);
            writer.Write(player.rang);
            writer.Write(player.nick);
            writer.Write(player.title);
            writer.Write(player.sceneID);
            writer.Write(position);
        }
    }


    public class PlayerAction : MessageBase
    {

        public int index;
        public string log;
        public string pass;

        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
        }
    }


    public class PlayerGoTo_Sr : MessageBase
    {
        public bool keySend;
        public bool down;
        public int key;
        public int index;
        public string login;
        public string password;
        public string nick;
        public float axisY;
        public Vector3 sendVec;
        public Quaternion rotate;

        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            keySend = reader.ReadBoolean();
            if (keySend)
            {
                down = reader.ReadBoolean();
                key = reader.ReadInt32();
            }
            else
            {
                axisY = (float)reader.ReadDouble();
            }
            index = reader.ReadInt32();
            login = reader.ReadString();
            password = reader.ReadString();
        }

        public override void Serialize(NetworkWriter write)
        {
            write.Write(rotate);
            write.Write(sendVec);
            write.Write(index);
        }


    }


    public class PlayerJoinToRoom_Sr : MessageBase
    {

        public string playerNick;
        public int sessionID;
        public string Login;
        public string Password;
        public int roomID;
        public string roomPassword;

        public string errorMsg;

        public override void Deserialize(NetworkReader reader)
        {
            sessionID = reader.ReadInt32();
            playerNick = reader.ReadString();
            Login = reader.ReadString();
            Password = reader.ReadString();
            roomID = reader.ReadInt32();
            bool pass = reader.ReadBoolean();
            if (pass)
            {
                roomPassword = reader.ReadString();
            }
        }

        public override void Serialize(NetworkWriter write)
        {
            write.Write(errorMsg);
        }
    }

    public class PlayerCreateRoom_Sr : MessageBase
    {

        public string playerNick;
        public int sessionID;
        public string Login;
        public string Password;
        public string roomName;
        public int mapID;
        public bool pass;
        public string roomPassword;

        public string errorMsg;

        public override void Deserialize(NetworkReader reader)
        {
            sessionID = reader.ReadInt32();
            playerNick = reader.ReadString();
            Login = reader.ReadString();
            Password = reader.ReadString();
            roomName = reader.ReadString();
            mapID = reader.ReadInt32();
            pass = reader.ReadBoolean();
            if (pass)
            {
                roomPassword = reader.ReadString();
            }
        }

        public override void Serialize(NetworkWriter write)
        {
            write.Write(errorMsg);
        }
    }

    public class RoomInstantiate : MessageBase
    {
        public int mapID;
        public Vector3 position;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(mapID);
            writer.Write(position);
        }
    }

    public class PlayerSetReady : MessageBase
    {

        public int id;
        public int scene;
        public string log;
        public string pass;

        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            id = reader.ReadInt32();
            scene = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
        }

    }


    public class Registration_Sr : MessageBase
    {
        //read
        public string log;
        public string pass;
        public string mail;

        //write
        public bool reg;
        public string msg;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(reg);
            writer.Write(msg);
        }

        public override void Deserialize(NetworkReader reader)
        {
            log = reader.ReadString();
            pass = reader.ReadString();
            mail = reader.ReadString();
        }
    }


    public class Respawn_Sr : MessageBase
    {

        public int index;
        public string log;
        public string pass;

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            log = reader.ReadString();
            pass = reader.ReadString();
        }

    }
}
