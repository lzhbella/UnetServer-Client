using UnityEngine;
using UnityEngine.Networking;

class Networking_OnPlayerAction : RoomsManager
{

    private static string nick1;

    //Check whether it is a character of the player and turn the player on its index, and gave him the coordinates of movement
    static public void OnPlMove(NetworkMessage netms)
    {
        Message_Sr.PlayerGoTo_Sr Go = netms.ReadMessage<Message_Sr.PlayerGoTo_Sr>();
        if (Networking_OnConnect.AccountVerefication(Go.index, Go.login, Go.password))
        {
            try
            {
                if (Go.keySend)
                {
                    Player_MovePlayer move = GetPlayerController(Go.index);
                    if (move)
                    {
                        move.Newposit(Go.key, Go.down);
                    }
                }
                else
                {
                    Player_MovePlayer move = GetPlayerController(Go.index);
                    if (move)
                    {
                        move.axisY = Go.axisY;
                    }
                }
            }
            catch (UnityException ex)
            {
                Debug.Log(ex.Message);
                Debug.Log("Networking_OnPlayerMove: ERROR");
            }
        }
    }

    static public void OnMouseButton(NetworkMessage netmsg)
    {
        Message_Sr.MouseButton_Sr mouse = netmsg.ReadMessage<Message_Sr.MouseButton_Sr>();
        if (Networking_OnConnect.AccountVerefication(mouse.index, mouse.log, mouse.pass))
        {
            if (mouse.down)
            {
                GetPlayerController(mouse.index).StartFire();
            }
            else
            {
                GetPlayerController(mouse.index).StopFire();
            }
        }
    }

    public static void HandleRespawn(NetworkMessage netmsg)
    {
        Message_Sr.Respawn_Sr rs = netmsg.ReadMessage<Message_Sr.Respawn_Sr>();
        if (Networking_OnConnect.AccountVerefication(rs.index, rs.log, rs.pass))
        {
            Player_MovePlayer pl = GetPlayerController(rs.index);
            if (pl != null)
            {
                pl.Respawn();
            }
        }
    }

    public static void HandleReload(NetworkMessage netMsg)
    {
        Message_Sr.PlayerAction action = netMsg.ReadMessage<Message_Sr.PlayerAction>();
        if (Networking_OnConnect.AccountVerefication(action.index, action.log, action.pass))
        {
            Player_MovePlayer pl = GetPlayerController(action.index);
            if (pl != null)
            {
                pl.StartReload();
            }
        }
    }

    public static void HandleDropWeapon(NetworkMessage netMsg)
    {
        Message_Sr.DropWeapon_Sr drop = netMsg.ReadMessage<Message_Sr.DropWeapon_Sr>();

        if (Networking_OnConnect.AccountVerefication(drop.index, drop.log, drop.pass))
        {
            Player_MovePlayer controll = GetPlayerController(drop.index);
            if (controll)
            {
                controll.DropWeapon();
            }
        }
    }

    public static void HandlePickUpItem(NetworkMessage netMsg)
    {
        Message_Sr.PickUpWeapon_Sr pick = netMsg.ReadMessage<Message_Sr.PickUpWeapon_Sr>();

        if (Networking_OnConnect.AccountVerefication(pick.index, pick.log, pick.pass))
        {
            Player_MovePlayer controll = GetPlayerController(pick.index);
            if (controll)
            {
                controll.DropWeapon();
                PickUpItem(pick.indexItem, controll);
            }
        }
    }

    static void PickUpItem(int itemIndex, Player_MovePlayer controll)
    {
        AccountData acc = Networking_OnConnect.GetAccountData(controll.index);
        Room room = GetRoom(acc.roomID);

        if (room)
        {
            Player_Item_Sr item = room.GetItem(itemIndex);

            if (item)
            {
                switch (item.ItemType)
                {
                    case ItemType_Sr.weapon:
                        PickUpWeapon(item, controll);
                        break;
                }
            }
        }
    }

    static void PickUpWeapon(Player_Item_Sr item, Player_MovePlayer controll)
    {
        Player_Weapon_Sr weap = null;

        weap = item.gameObject.GetComponent<Player_Weapon_Sr>();

        if (weap)
        {
            weap.TakeWeapon(controll.weapon, controll.index);
        }
    }
}
