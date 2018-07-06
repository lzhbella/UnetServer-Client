using UnityEngine;
using UnityEngine.Networking;

namespace Message
{

    #region Game Action Message
    public class PickUpItem : MessageBase
    {
        public int itemIndex;
        public int index;
        public bool thisPlayer;
        public int ammo;
        public int ammoMax;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
            writer.Write(itemIndex);
        }

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            itemIndex = reader.ReadInt32();
            thisPlayer = reader.ReadBoolean();
            if (thisPlayer)
            {
                ammo = reader.ReadInt32();
                ammoMax = reader.ReadInt32();
            }
        }
    }

    public class Reload : MessageBase
    {
        public int index;
        public double time;

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            time = reader.ReadDouble();
        }
    }

        public class ItemPositionUpdate : MessageBase
    {
        public int itemIndex;
        public Vector3 vect;
        public Quaternion rotate;

        public override void Deserialize(NetworkReader reader)
        {
            itemIndex = reader.ReadInt32();
            vect = reader.ReadVector3();
            rotate = reader.ReadQuaternion();
        }

    }

    public class Shot : MessageBase
    {
        public int index;
        public BulletType type;
        int bulletsNumber;
        public Vector3[] vect;
        public bool thisPlayer;
        public int ammo;

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
            type = (BulletType)reader.ReadInt32();
            bulletsNumber = reader.ReadInt32();
            if (bulletsNumber > 0)
            {
                vect = new Vector3[bulletsNumber];
                for (int i = 0; i < bulletsNumber; i++)
                {
                    vect[i] = reader.ReadVector3();
                }
            }
            thisPlayer = reader.ReadBoolean();
            if (thisPlayer)
            {
                ammo = reader.ReadInt32();
            }
        }

    }

    public class TakeItem : MessageBase
    {
        public int itemIndex;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(itemIndex);
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }

    }

    public class DropWeapon : MessageBase
    {
        public int index;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }

        public override void Deserialize(NetworkReader reader)
        {
            index = reader.ReadInt32();
        }
    }

    public class MouseButton : MessageBase
    {

        public bool down;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(down);
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }

    }


    public class PlayerGoTo : MessageBase
    {
        public bool keySend;
        public bool down;
        public int key;
        public Vector3 vect;
        public float axisY;
        public Quaternion rotate;
        public int index;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(keySend);
            if (keySend)
            {
                writer.Write(down);
                writer.Write(key);
            }
            else
            {
                writer.Write((double)axisY);
            }
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }

        public override void Deserialize(NetworkReader reader)
        {
            rotate = reader.ReadQuaternion();
            vect = reader.ReadVector3();
            index = reader.ReadInt32();
        }


    }


    public class Respawn : MessageBase
    {

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }

    }
    #endregion

    #region Load Scene Message

    public class LoadItem : MessageBase
    {
        public Data_ItemFile item;

        public override void Deserialize(NetworkReader reader)
        {
            item = ScriptableObject.CreateInstance<Data_ItemFile>();
            item.index = reader.ReadInt32();
            item.ItemType = (ItemType)reader.ReadInt32();
            switch (item.ItemType)
            {
                case ItemType.weapon:
                    item.weaponType = (weaponType)reader.ReadInt32();
                    break;
            }
            item.droped = reader.ReadBoolean();
            if (item.droped)
            {
                item.pos = reader.ReadVector3();
                item.rotate = reader.ReadQuaternion();
            }
            else
            {
                item.owner = reader.ReadInt32();
            }
            Data_ListPlayerOnScene.AddNewItem(item);
        }
    }

    public class PlayerOnScene : MessageBase
    {
        public int sendnumber;
        int number = 0;
        bool yes;
        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            try
            {
                number = reader.ReadInt32();
                for (int a = 0; a < number; a++)
                {
                    yes = reader.ReadBoolean();
                    if (yes)
                    {
                        Data_PlayerFile player = ScriptableObject.CreateInstance<Data_PlayerFile>();
                        player.sceneID = reader.ReadInt32();
                        player.charID = reader.ReadInt32();
                        player.rang = reader.ReadInt32();
                        player.nick = reader.ReadString();
                        player.title = reader.ReadString();
                        player.position = reader.ReadVector3();
                        player.rotate = reader.ReadQuaternion();
                        player.death = reader.ReadBoolean();
                        Data_ListPlayerOnScene.AddPlayer(player);
                        Debug.Log("Add player index " + player.charID);
                    }
                    else
                    {
                        Data_ListPlayerOnScene.AddPlayer(null);
                        Debug.Log("Add null player");
                    }
                }
            }
            catch
            {
                Debug.Log("Load list fail!");
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.charID);
            writer.Write(sendnumber);
            writer.Write(Data_MyData.Me.sceneID);
        }
    }

    public class ItemOnScene : MessageBase
    {
        public int sendnumber;
        int number = 0;
        bool yes;
        public bool dataEnd;
        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            try
            {
                number = reader.ReadInt32();
                dataEnd = reader.ReadBoolean();
                for (int a = 0; a < number; a++)
                {
                    yes = reader.ReadBoolean();
                    if (yes)
                    {
                        Data_ItemFile item = ScriptableObject.CreateInstance<Data_ItemFile>();
                        item.index = reader.ReadInt32();
                        item.ItemType = (ItemType)reader.ReadInt32();
                        switch (item.ItemType)
                        {
                            case ItemType.weapon:
                                item.weaponType = (weaponType)reader.ReadInt32();
                                break;
                        }
                        item.droped = reader.ReadBoolean();
                        if (item.droped)
                        {
                            item.pos = reader.ReadVector3();
                            item.rotate = reader.ReadQuaternion();
                        }
                        else
                        {
                            item.owner = reader.ReadInt32();
                            bool me = reader.ReadBoolean();
                            if (me)
                            {
                                Chat.ammoMax = reader.ReadInt32();
                            }
                        }
                        Data_ListPlayerOnScene.AddNewItem(item);
                    }
                    else
                    {
                        Data_ListPlayerOnScene.AddNewItem(null);
                    }

                }
            }
            catch
            {
                Debug.Log("Load list fail!");
            }
        }
    }


    public class NewPlayerOnScene : MessageBase
    {

        public Data_PlayerFile player;

        public override void Deserialize(NetworkReader reader)
        {
            player = ScriptableObject.CreateInstance<Data_PlayerFile>();
            player.charID = reader.ReadInt32();
            player.rang = reader.ReadInt32();
            player.nick = reader.ReadString();
            player.title = reader.ReadString();
            player.sceneID = reader.ReadInt32();
            player.position = reader.ReadVector3();
        }
    }


    public class OnPlayerReady : MessageBase
    {

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.charID);
            writer.Write(Data_MyData.roomID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }
    }
    #endregion

    #region Menu Message
    public class Registration : MessageBase
    {
        //write
        public string Mail;
        public string Login;
        public string Password;

        //read
        public bool Registr;
        public string Answer;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Login);
            writer.Write(Password);
            writer.Write(Mail);
        }

        public override void Deserialize(NetworkReader reader)
        {
            Registr = reader.ReadBoolean();
            Answer = reader.ReadString();
        }
    }


    public class LoginSendMess : MessageBase
    {

        public string log;
        public string pass;

        // This method would be generated
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(log);
            writer.Write(pass);
        }
    }


    public class JoinToRoom : MessageBase
    {

        public string playerNick;
        public int sessionID;
        public string Login;
        public string Password;
        public int roomID;
        public bool pass;
        public string roomPassword;

        public string errorMsg;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(sessionID);
            writer.Write(playerNick);
            writer.Write(Login);
            writer.Write(Password);
            writer.Write(roomID);
            writer.Write(pass);
            if (pass)
            {
                writer.Write(roomPassword);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            errorMsg = reader.ReadString();
        }
    }


    public class CreateCh : MessageBase
    {
        //read
        public string log;
        public string pass;
        public string nick;

        //write
        public string msg;

        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            msg = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.sessionID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
            writer.Write(nick);
        }
    }


    public class CharecterSelect : MessageBase
    {

        public string playerNick;
        public string Login;
        public string Password;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(playerNick);
            writer.Write(Login);
            writer.Write(Password);
        }
    }


    public class CharGet : MessageBase
    {
        public int index;
        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            Scene2_PlayerSelect.temp.Clear();
            index = reader.ReadInt32();
            int chars = reader.ReadInt32();
            for (int p = 0; p < chars; p++)
            {
                Data_PlayerFile player = ScriptableObject.CreateInstance<Data_PlayerFile>();
                player.nick = reader.ReadString();
                player.charID = reader.ReadInt32();
                player.sceneID = reader.ReadInt32();
                player.HPMax = reader.ReadInt32();
                Scene2_PlayerSelect.temp.Add(player);
            }
        }
    }

    public class RoomCreate : MessageBase
    {
        public string roomName;
        public int mapID;
        public bool pass;
        public string roomPassword;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.sessionID);
            writer.Write(Data_MyData.PlayerSelect);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
            writer.Write(roomName);
            writer.Write(mapID);
            writer.Write(pass);
            if (pass)
            {
                writer.Write(roomPassword);
            }
        }
    }

    public class RoomInstantiate : MessageBase
    {
        public int mapID;
        public Vector3 position;

        public override void Deserialize(NetworkReader reader)
        {
            mapID = reader.ReadInt32();
            position = reader.ReadVector3();
        }
    }

    public class RoomsList : MessageBase
    {
        private int roomsCount;
        public RoomStr[] Rooms;

        private int index;
        private string name;
        private int map;
        private short players;
        private short maxPlayers;
        private bool password;
        // This method would be generated
        public override void Deserialize(NetworkReader reader)
        {
            roomsCount = reader.ReadInt32();
            Rooms = new RoomStr[roomsCount];
            for(int i = 0; i < roomsCount; i++)
            {
                index = reader.ReadInt32();
                name = reader.ReadString();
                map = reader.ReadInt32();
                players = reader.ReadInt16();
                maxPlayers = reader.ReadInt16();
                password = reader.ReadBoolean();

                RoomStr r = new RoomStr(index, name, map, players, maxPlayers, password);
                Rooms[i] = r;
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Data_MyData.sessionID);
            writer.Write(Data_MyData.Login);
            writer.Write(Data_MyData.Password);
        }
    }
    #endregion

}

