using UnityEngine;
using UnityEngine.Networking;


//This script is responsible for the transfer of players to coordinate movement, it checks whether the data is the data, or our other players

public class Scene3_PlayerAction : MonoBehaviour
{
    void Start()
    {
        Networking_client.RegisterHandl += RegisterHandler;
    }

	// Use this for initialization
	void RegisterHandler ()
	{
		Networking_client.net.RegisterHandler (Networking_msgType.PlayerMove, HandleOnMovePlayer);
        Networking_client.net.RegisterHandler(Networking_msgType.Fire, HandleFire);
        Networking_client.net.RegisterHandler(Networking_msgType.Reload, HandleReload);
        Networking_client.net.RegisterHandler(Networking_msgType.HealthChange, HandleHealthChange);
    }

	void HandleOnMovePlayer (NetworkMessage netMsg)
	{
		Message.PlayerGoTo go = netMsg.ReadMessage<Message.PlayerGoTo> ();
		if (go.vect != Vector3.zero) {
            Player_AnimateControl pl = Data_ListPlayerOnScene.GetPlayerControll(go.index);
            if(pl != null)
            {
                pl.Move(go.vect, go.rotate);
            }
		}
	}

    void HandleFire(NetworkMessage netMsg)
    {
        Message.Shot shot = netMsg.ReadMessage<Message.Shot>();
        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(shot.index);
        if (controll)
        {
            controll.Fire(shot.vect, shot.type);
        }
        if(shot.thisPlayer)
        {
            Chat.AmmoValue(shot.ammo);
        }
    }

    void HandleReload(NetworkMessage netMsg)
    {
        int id = netMsg.reader.ReadInt32();
        float time = (float)netMsg.reader.ReadDouble();
        Player_AnimateControl controll = Data_ListPlayerOnScene.GetPlayerControll(id);
        if (controll)
        {
            controll.Reload(time);
            if (controll.index == Data_MyData.charID)
            {
                Chat.ReloadStart(time);
            }
        }
    }

    void HandleHealthChange(NetworkMessage netMsg)
    {
        int hp = netMsg.reader.ReadInt32();
        Chat.HealthValue(hp);
    }
}
