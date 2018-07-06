using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public int roomID = -1;
    public string _name = string.Empty;
    /// <summary>
    /// How much people can connect to room
    /// </summary>
    [Tooltip("How much people can connect to room")]
    public int maxPlayerNumber = 10;
    public int mapID = -1;
    
    public int playerNumber { get; private set; }
    /// <summary>
    /// The number of zombies that will be created at the start of the room
    /// </summary>
    [Header("-----Zombie setting-----")]
    [Tooltip("The number of zombies that will be created at the start of the room")]
    public int zombieNumber = 50;
    [Header("SceneData")]
    public List<PlayerData> playersData = new List<PlayerData>();
    List<int> playerListNULLVALLUE = new List<int>();
    public int playerListCount { get { return playersData.Count; } }
    //Items
    List<Player_Item_Sr> ItemsList = new List<Player_Item_Sr>();
    List<int> nullItem = new List<int>();
    public int itemListCount { get { return ItemsList.Count; } }

    private string roomPassword = string.Empty;
    bool pass = false;
    public bool password { get { return pass; } }

    public Vector3 spawnPosition { get; private set; }

    public void SetRoomPassword(string password)
    {
        if (string.IsNullOrEmpty(roomPassword))
        {
            roomPassword = password;
            pass = true;
        }
        else
        {
            Debug.Log(string.Format("Room with id '{0}' have password, you can't change him!", roomID));
        }
    }

    public bool CheckPassword(string password)
    {
        if (roomPassword == password)
            return true;
        else
            return false;
    }

    #region Zombie Load On Scene
    public void SpawnMob()
    {
        spawnPosition = new Vector3(5, transform.position.y, 5);

        GameObject obj = (GameObject)Resources.Load("Zombie");
        Vector3 vect = Vector3.zero;

        float Hight = Random.Range(0.1f, 0.15f);
        float Midle = Random.Range(0.15f, 0.25f);
        float Low = 1 - (Hight + Midle);

        //check zombie number, if more than maximum players in room
        if (zombieNumber >= maxPlayerNumber)
        {
            zombieNumber = maxPlayerNumber - 2;
        }

        if (zombieNumber >= 3)
        {
            int zombieLow = Mathf.FloorToInt((float)zombieNumber * Low);
            int zombieMidle = Mathf.FloorToInt((float)zombieNumber * Midle);
            int zombieHight = Mathf.FloorToInt((float)zombieNumber * Hight);

            int sessionID = -1;
            for (int i = 0; i < zombieLow; i++)
            {
                do
                {
                    vect = new Vector3(Random.Range(0, 100), transform.position.y, Random.Range(0, 100));
                }
                while (vect.x < 20f || vect.z < 20f);
                GameObject Zombie = (GameObject)Instantiate(obj, vect, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
                Data_PlayerFile_Sr data = ScriptableObject.CreateInstance<Data_PlayerFile_Sr>();
                data.HPMax = Random.Range(60, 110);
                data.SetHP(data.HPMax);
                switch (Random.Range(1, 3))
                {
                    case 1:
                        data.nick = "zombie";
                        break;
                    case 2:
                        data.nick = "sickzombie";
                        break;
                    case 3:
                        data.nick = "policezombie";
                        break;
                }
                data.PlayerReady = true;
                data.attackPower = Random.Range(10, 17);
                Zombie.GetComponent<Player_MovePlayer>().zombie = true;

                sessionID = Networking_OnConnect.BotAdd(roomID, playersData.Count);
                WritePlayer(sessionID, data, Zombie);
            }

            for (int i = 0; i < zombieMidle; i++)
            {
                do
                {
                    vect = new Vector3(Random.Range(0, 100), transform.position.y, Random.Range(0, 100));
                }
                while (vect.x < 20f || vect.z < 20f);
                GameObject Zombie = (GameObject)Instantiate(obj, vect, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
                Data_PlayerFile_Sr data = ScriptableObject.CreateInstance<Data_PlayerFile_Sr>();
                data.HPMax = Random.Range(180, 250);
                data.SetHP(data.HPMax);
                data.nick = "mutantzombie";
                data.PlayerReady = true;
                data.attackPower = Random.Range(20, 30);
                Zombie.GetComponent<Player_MovePlayer>().zombie = true;

                sessionID = Networking_OnConnect.BotAdd(roomID, playersData.Count);
                WritePlayer(sessionID, data, Zombie);
            }

            for (int i = 0; i < zombieHight; i++)
            {
                do
                {
                    vect = new Vector3(Random.Range(0, 100), transform.position.y, Random.Range(0, 100));
                }
                while (vect.x < 20f || vect.z < 20f);
                GameObject Zombie = (GameObject)Instantiate(obj, vect, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
                Data_PlayerFile_Sr data = ScriptableObject.CreateInstance<Data_PlayerFile_Sr>();
                data.HPMax = Random.Range(300, 400);
                data.SetHP(data.HPMax);
                data.nick = "strongzombie";
                data.PlayerReady = true;
                data.attackPower = Random.Range(36, 48);
                Zombie.GetComponent<Player_MovePlayer>().zombie = true;

                sessionID = Networking_OnConnect.BotAdd(roomID, playersData.Count);
                WritePlayer(sessionID, data, Zombie);
            }
        }
    }
    #endregion

    #region Player Metods
    public void WritePlayer(int sessionID, Data_PlayerFile_Sr player, GameObject obj)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(sessionID);
        if (acc != null)
        {
            PlayerData data = new PlayerData();
            data.playerData = player;
            data.playerObj = obj;
            data.playerMoveScript = obj.GetComponent<Player_MovePlayer>();
            data.sessionID = sessionID;

            int indexInRoom = -1;
            if (playerListNULLVALLUE.Count > 0)
            {
                indexInRoom = playerListNULLVALLUE[0];
                playerListNULLVALLUE.RemoveAt(0);

                player.sessionID = sessionID;
                data.playerMoveScript.index = sessionID;
                playersData[indexInRoom] = data;

                acc.indexInRoom = indexInRoom;
                acc.roomID = roomID;
            }
            else
            {
                indexInRoom = playersData.Count;

                player.sessionID = sessionID;
                data.playerMoveScript.index = sessionID;
                playersData.Add(data);

                acc.indexInRoom = indexInRoom;
                acc.roomID = roomID;
            }

            playerNumber++;
        }
    }

    public Data_PlayerFile_Sr GetPlayerData(int index)
    {
        Data_PlayerFile_Sr d = null;
        if (index >= 0 && playersData.Count > index)
        {
            if (playersData[index] != null)
            {
                d = playersData[index].playerData;
            }
        }
        return d;
    }

    public Player_MovePlayer GetPlayerControll(int index)
    {
        Player_MovePlayer d = null;
        if (index >= 0 && playersData.Count > index)
        {
            if (playersData[index] != null)
            {
                d = playersData[index].playerMoveScript;
            }
        }
        return d;
    }

    public bool DisconnectPlayer(int index)
    {
        bool yes = false;
        if (index >= 0 && playersData.Count > index)
        {
            AccountData acc = Networking_OnConnect.GetAccountData(playersData[index].sessionID);
            if (acc != null)
            {
                yes = true;
                //Remove rooms data at account data
                acc.indexInRoom = -1;
                acc.roomID = -1;

                Message_Sr.DisconnectPlayer disc = new Message_Sr.DisconnectPlayer();
                disc.ID = playersData[index].sessionID;
                RoomsManager.SendReliableToRoom(Networking_msgType_Sr.DisconnectPlayer, disc, roomID);
                //Remove player weapon
                Player_MovePlayer controll = playersData[index].playerMoveScript;
                if (controll.weaponOnMe)
                {
                    RoomsManager.SendIntToAllRoom(Networking_msgType_Sr.RemoveItemOnScene, controll.weaponOnMe.index, roomID);
                    RemoveItem(controll.weaponOnMe.index);
                }
                //Remove player data
                Destroy(playersData[index].playerObj);
                playersData[index] = null;
                playerListNULLVALLUE.Add(index);
                playerNumber--;
            }
        }
        return yes;
    }
    #endregion

    /// <summary>
    /// Disconnect and destroy all player, item and room
    /// </summary>
    public void DestroyRoom()
    {
        //Disconnect and destroy all player at room
        for(int index = 0; index < playersData.Count; index++)
        {
            int sessionID = playersData[index].sessionID;
            AccountData acc = Networking_OnConnect.GetAccountData(sessionID);
            if (acc != null)
            {
                //Remove rooms data at account data
                acc.indexInRoom = -1;
                acc.roomID = -1;

                Message_Sr.DisconnectPlayer disc = new Message_Sr.DisconnectPlayer();
                disc.ID = sessionID;
                RoomsManager.SendToThisPlayer(Networking_msgType_Sr.DisconnectPlayer, disc, sessionID);
                //Remove player weapon
                Player_MovePlayer controll = playersData[index].playerMoveScript;
                if (controll.weaponOnMe)
                {
                    RoomsManager.SendIntToAllRoom(Networking_msgType_Sr.RemoveItemOnScene, controll.weaponOnMe.index, roomID);
                    RemoveItem(controll.weaponOnMe.index);
                }
                //Remove player data
                Destroy(playersData[index].playerObj);
                
            }
        }
        playersData.Clear();
        playerListNULLVALLUE.Clear();
        //Destroy all item in room
        for (int index = 0; index < ItemsList.Count; index++)
        {
            Player_Item_Sr item = ItemsList[index];

            if (item)
            {
                Destroy(item.gameObject);
            }
        }
        ItemsList.Clear();
        nullItem.Clear();

        Destroy(gameObject);
    }

    #region Items Metods
    public void AddNewItemOnScene(Player_Item_Sr item)
    {
        if (nullItem.Count > 0)
        {
            ItemsList[nullItem[0]] = item;
            item.index = nullItem[0];
            item.roomID = roomID;
            nullItem.RemoveAt(0);
        }
        else
        {
            item.index = ItemsList.Count;
            item.roomID = roomID;
            ItemsList.Add(item);
        }
    }

    public void RemoveItem(int id)
    {
        if (id > -1 && ItemsList.Count > id)
        {
            ItemsList[id] = null;
            nullItem.Add(id);
        }
    }

    public Player_Item_Sr GetItem(int id)
    {
        Player_Item_Sr item = null;

        if (id > -1 && ItemsList.Count > id)
        {
            item = ItemsList[id];
        }

        return item;
    }
    #endregion
}

public class PlayerData
{
    public int sessionID = -1;
    public GameObject playerObj = null;
    public Player_MovePlayer playerMoveScript = null;
    public Data_PlayerFile_Sr playerData = null;
}
