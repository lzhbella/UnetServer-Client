using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//This script launches the game scene and spawn all the players on the map

public class Scene3_Gamescreen : MonoBehaviour
{
    [Header("MainMenu")]
    [SerializeField]
    GameObject RoomMenu;
    [Header("Game scenes prefabs")]
    [SerializeField]
    GameObject
        Scene1;
    [Header("Player and enemy controller prefabs")]
    [SerializeField]
    GameObject Player;

    GameObject map;

    void RegisterHandlers()
    {
        Networking_client.net.RegisterHandler(Networking_msgType.Death, HandleDeath);
        Networking_client.net.RegisterHandler(Networking_msgType.Respawn, HandleRespawn);
        Networking_client.net.RegisterHandler(Networking_msgType.ZombieAttack, HandleZombieAttack);
        Networking_client.net.RegisterHandler(Networking_msgType.NewPlayerConnnectOnScene, HandleOnNewPlayerConnect);
        Networking_client.net.RegisterHandler(Networking_msgType.Rang, HandleNewPlayerRang);
        Networking_client.net.RegisterHandler(Networking_msgType.Title, HandleNewPlayerTitle);
        Networking_client.net.RegisterHandler(Networking_msgType.ItemPositionUpdate, HandleUpdateItemPosition);
        Networking_client.net.RegisterHandler(Networking_msgType.PickUp, HandlePickUpItem);
        Networking_client.net.RegisterHandler(Networking_msgType.DropWeapon, HandleDropWeapon);
        Networking_client.net.RegisterHandler(Networking_msgType.NewItemOnScene, HandleNewItemOnScene);
        Networking_client.net.RegisterHandler(Networking_msgType.RemoveItem, HandleRemoveItemOnScene);
        Networking_client.net.RegisterHandler(Networking_msgType.PlayerList, HandleOnLoadPlList);
        Networking_client.net.RegisterHandler(Networking_msgType.ItemList, HandleOnItemsLoad);
        Networking_client.net.RegisterHandler(Networking_msgType.MapInstantiate, HandlerSceneLoad);
    }

    // Use this for initialization
    void Start()
    {
        Networking_client.RegisterHandl += RegisterHandlers;
        Networking_client.DisconnectMe += MeDisconnect;
    }

    void HandlerSceneLoad(NetworkMessage netmsg)
    {
        Message.RoomInstantiate instance = netmsg.ReadMessage<Message.RoomInstantiate>();

        GameObject m = Resources.Load<GameObject>("Maps/" + instance.mapID);

        if (m)
        {
           map = (GameObject)Instantiate(m, instance.position, Quaternion.identity);
        }
    }

    public void DisconnectAtRoom()
    {
        Networking_client.SendMyAction(Networking_msgType.DisconnectAtRoom);
    }

    void MeDisconnect()
    {
        Destroy(map);
        Data_ListPlayerOnScene.DisconnectAll();
    }

    #region Scene Spawn 
    void SpawnScene()
    {
        for (int i = 0; i < Data_ListPlayerOnScene.PlayerListCount; i++)
        {
            Data_PlayerFile player = Data_ListPlayerOnScene.playerList[i];
            if (player)
            {
                string name = player.nick;
                if (name == "zombie" || name == "policezombie" || name == "sickzombie" || name == "mutantzombie" || name == "strongzombie")
                {
                    ZombieSpawn(player);
                }
                else if (player.nick != Data_MyData.PlayerSelect)
                {
                    Spawn(player);
                }
                else
                {
                    MeSpawn(player);
                }
            }
        }
        for (int s = 0; s < Data_ListPlayerOnScene.ItemListCount; s++)
        {
            Data_ItemFile data = Data_ListPlayerOnScene.GetItemData(s);
            if (data)
            {
                switch (data.ItemType)
                {
                    case ItemType.weapon:
                        SpawnWeapon(data);
                        break;
                }
            }
        }
        Message.OnPlayerReady ready = new Message.OnPlayerReady();
        Networking_client.net.Send(Networking_msgType.MeReady, ready);
    }

    void SpawnWeapon(Data_ItemFile data)
    {
        switch (data.weaponType)
        {
            case weaponType.Machine:
                GameObject obj = Resources.Load<GameObject>("Weapons/Machine");
                InstantiateWeapon(obj, data);
                break;
            case weaponType.ShotGun:
                obj = Resources.Load<GameObject>("Weapons/ShotGun");
                InstantiateWeapon(obj, data);
                break;
            case weaponType.Rifle:
                obj = Resources.Load<GameObject>("Weapons/Rifle");
                InstantiateWeapon(obj, data);
                break;
            case weaponType.GaussGun:
                obj = Resources.Load<GameObject>("Weapons/GaussGun");
                InstantiateWeapon(obj, data);
                break;
        }
    }

    void InstantiateWeapon(GameObject obj, Data_ItemFile data)
    {
        if (obj)
        {
            GameObject go = Instantiate(obj);
            Player_Weapon weap = go.GetComponent<Player_Weapon>();
            if (data.droped)
            {
                go.transform.position = data.pos;
                go.transform.rotation = data.rotate;
            }
            else
            {
                if (data.owner != -1)
                {
                    Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(data.owner);
                    if (controll)
                    {
                        weap.TakeWeapon(controll.rightHand, controll);
                    }
                }
            }
            weap.index = data.index;
            Data_ListPlayerOnScene.WriteNewItem(weap, data.index);
        }
        else
        {
            Debug.Log("mobel not found!");
        }
    }

    void Spawn(Data_PlayerFile player)
    {
        GameObject Enemy = (GameObject)Instantiate(Player, player.position, player.rotate);
        Data_ListPlayerOnScene.WriteGameObject(Enemy, player.charID);
        Enemy.name = player.nick;
        Player_AnimateControl en = Enemy.GetComponent<Player_AnimateControl>();
        en.index = player.charID;
        en.canvas.SetNick(player.nick);
        en.canvas.SetTitle(player.title);
        en.canvas.SetRang(player.rang);
        en.enabled = true;
        if (player.death)
        {
            en.Death();
        }
    }

    void MeSpawn(Data_PlayerFile player)
    {
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject Me = (GameObject)Instantiate(Player, player.position, player.rotate);
        Me.name = player.nick;
        SphereCollider sphere = Me.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.5f;
        Data_MyData.roomID = player.sceneID;

        Player_AnimateControl controll = Me.GetComponent<Player_AnimateControl>();
        controll.index = player.charID;
        controll.canvas.SetNick(player.nick);
        controll.canvas.SetTitle(player.title);
        controll.canvas.SetRang(player.rang);

        camera.GetComponent<Camera>().enabled = true;
        camera.GetComponent<AudioListener>().enabled = true;
        Player_PlayerLocate locate = camera.GetComponent<Player_PlayerLocate>();
        locate.enabled = true;
        locate.GameStart(Me.transform);

        Data_ListPlayerOnScene.WriteGameObject(Me, player.charID);
        Data_MyData.charID = player.charID;
        Data_MyData.Me = player;
    }

    void ZombieSpawn(Data_PlayerFile zombie)
    {
        GameObject model = null;
        switch (zombie.nick)
        {
            case "zombie":
                model = (GameObject)Resources.Load("Zombie/Zombie");
                break;
            case "policezombie":
                model = (GameObject)Resources.Load("Zombie/PoliceZombie");
                break;
            case "sickzombie":
                model = (GameObject)Resources.Load("Zombie/SickZombie");
                break;
            case "mutantzombie":
                model = (GameObject)Resources.Load("Zombie/MutantZombie");
                break;
            case "strongzombie":
                model = (GameObject)Resources.Load("Zombie/StrongZombie");
                break;
        }
        if (model != null)
        {
            GameObject Zombie = (GameObject)Instantiate(model, zombie.position, zombie.rotate);
            Data_ListPlayerOnScene.WriteGameObject(Zombie, zombie.charID);
            Zombie.name = zombie.nick;
            Player_AnimateControl zm = Zombie.GetComponent<Player_AnimateControl>();
            zm.index = zombie.charID;
            zm.zombie = true;
            if (zombie.death)
            {
                Zombie.SetActive(false);
            }
        }
    }
    #endregion

    #region Scene Load Handle
    /// <summary>
    /// Get all players and mobs in room
    /// </summary>
    /// <param name="netMsg"></param>
    void HandleOnLoadPlList(NetworkMessage netMsg)
    {
        Message.PlayerOnScene players = netMsg.ReadMessage<Message.PlayerOnScene>();
    }

    /// <summary>
    /// Get all items in room
    /// </summary>
    /// <param name="netMsg"></param>
    void HandleOnItemsLoad(NetworkMessage netMsg)
    {
        Message.ItemOnScene items = netMsg.ReadMessage<Message.ItemOnScene>();
        if (items.dataEnd)
        {
            RoomMenu.SetActive(false);
            SpawnScene();
        }
    }

    void HandleOnNewPlayerConnect(NetworkMessage netMsg)
    {
        Message.NewPlayerOnScene player = netMsg.ReadMessage<Message.NewPlayerOnScene>();
        try
        {
            if (player.player.nick != Data_MyData.PlayerSelect)
            {
                Data_ListPlayerOnScene.WritePlayer(player.player);
                Spawn(player.player);
            }
        }
        catch
        {
            Debug.Log("Player load fail!");
        }
    }

    void HandleNewItemOnScene(NetworkMessage netMsg)
    {
        Message.LoadItem item = netMsg.ReadMessage<Message.LoadItem>();
        SpawnWeapon(item.item);
    }

    void HandleRemoveItemOnScene(NetworkMessage netMsg)
    {
        int indexItem = netMsg.reader.ReadInt32();

        if (indexItem > -1)
        {
            Data_ListPlayerOnScene.RemoveItem(indexItem);
        }
    }
    #endregion

    #region Game Handle
    void HandleDeath(NetworkMessage netMsg)
    {
        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(netMsg.reader.ReadInt32());
        if (controll != null)
        {
            controll.Death();
        }
    }

    void HandleRespawn(NetworkMessage netMsg)
    {
        int id = netMsg.reader.ReadInt32();
        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(id);
        if (controll != null)
        {
            controll.transform.position = netMsg.reader.ReadVector3();
            controll.transform.rotation = netMsg.reader.ReadQuaternion();
            controll.gameObject.SetActive(true);
            controll.Respawn();
            if (id == Data_MyData.charID)
            {
                Player_PlayerLocate.death = false;
            }
        }
    }

    void HandleZombieAttack(NetworkMessage netMsg)
    {
        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(netMsg.reader.ReadInt32());
        if (controll != null)
        {
            controll.ZombieAttack();
        }
    }

    void HandleNewPlayerRang(NetworkMessage netMsg)
    {
        int index = netMsg.reader.ReadInt32();
        int rang = netMsg.reader.ReadInt32();

        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(index);

        if (controll)
        {
            controll.canvas.SetRang(rang);
        }
    }

    void HandleNewPlayerTitle(NetworkMessage netMsg)
    {
        int index = netMsg.reader.ReadInt32();
        string title = netMsg.reader.ReadString();

        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(index);

        if (controll)
        {
            controll.canvas.SetTitle(title);
        }
    }

    void HandleUpdateItemPosition(NetworkMessage netMsg)
    {
        Message.ItemPositionUpdate update = netMsg.ReadMessage<Message.ItemPositionUpdate>();

        Player_Item item = Data_ListPlayerOnScene.GetItem(update.itemIndex);
        if (item)
        {
            item.SyncPosition(update.vect, update.rotate);
        }
    }

    void HandlePickUpItem(NetworkMessage netMsg)
    {
        Message.PickUpItem up = netMsg.ReadMessage<Message.PickUpItem>();

        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(up.index);
        if (controll)
        {
            Player_Item item = Data_ListPlayerOnScene.GetItem(up.itemIndex);
            if (item)
            {
                item.TakeWeapon(controll.rightHand, controll);
            }
            if (up.thisPlayer)
            {
                Chat.AmmoMax(up.ammo, up.ammoMax);
            }
        }
    }

    void HandleDropWeapon(NetworkMessage netMsg)
    {
        Message.DropWeapon drop = netMsg.ReadMessage<Message.DropWeapon>();

        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(drop.index);
        if (controll)
        {
            controll.DropWeapon();
        }
    }
    #endregion
}
