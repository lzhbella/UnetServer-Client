using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class Networking_Server : MonoBehaviour
{

    public Text fps;
    public Toggle useWebSockets;
    public Text error;
    private bool isAtStartup = false;
    public int Port;
    public int MaxConnection;

    delegate void Ex(string e);
    static event Ex Error;

    void Awake()
    {
        Error += ErrorMsg;
    }

    void ErrorMsg(string e)
    {
        error.text = e;
    }

    public static void ErrorMessage(string e)
    {
        Error(e);
    }

    void Update()
    {
        float f = 1 / Time.deltaTime;
        fps.text = string.Format("FPS: " + f.ToString());
    }

    public void ServerStart()
    {
        if (SQL_sqlConnect.SqlUp)
        {
            if (!isAtStartup)
            {
                
                ConnectionConfig config = new ConnectionConfig();
                config.AddChannel(QosType.Reliable);
                config.AddChannel(QosType.Unreliable);
                NetworkServer.useWebSockets = useWebSockets.isOn;
                //NetworkServer.Configure(config, MaxConnection);
                HostTopology host = new HostTopology(config, MaxConnection);
                NetworkServer.Configure(host);
                if (NetworkServer.Listen(Port))
                {
                    
                    NetworkServer.RegisterHandler(MsgType.Connect, OnConn);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.LoginSend, Networking_OnConnect.LoginGet);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.PlayerMove, Networking_OnPlayerAction.OnPlMove);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.JoinToRoom, Networking_OnPlayerSelect.HandlerJoinToRoom);
                    NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.OnPlayerReady, RoomsManager.HandlerReady);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.Registration, SQL_AccountRegistration.CreatNewAccount);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.CharCreate, SQL_AccountRegistration.CreateChar);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.RoomList, RoomsManager.GetRoomsList);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.RoomCreate, Networking_OnPlayerSelect.HandlerCreateRoom);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.Chat, Networking_Chat_Sr.ChatHandler);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.PasswordRecovery, SQL_PasswordRecovery.PasswordRecovery);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.MouseButton, Networking_OnPlayerAction.OnMouseButton);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.Respawn, Networking_OnPlayerAction.HandleRespawn);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.Reload, Networking_OnPlayerAction.HandleReload);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.TopList, SQL_sqlConnect.SendTopList);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.DropWeapon, Networking_OnPlayerAction.HandleDropWeapon);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.PickUp, Networking_OnPlayerAction.HandlePickUpItem);
                    NetworkServer.RegisterHandler(Networking_msgType_Sr.DisconnectAtRoom, RoomsManager.HandlerPlayerDisconnectAtRoom);
                    isAtStartup = true;
                    Debug.Log("Server start");
                    ErrorMsg("Server start");
                }
            }
        }
        else
        {
            ErrorMsg("Please first connect to MySQL!");
        }
    }

    void OnDisconnect(NetworkMessage netmsg)
    {
        try
        {
            RoomsManager.Disconnect(netmsg.conn.connectionId);
            netmsg.conn.Disconnect();
            Debug.Log("Disconnection successful " + netmsg.conn);
        }
        catch
        {
            Debug.Log("Disconnection fail " + netmsg.conn);
        }
    }

    void OnConn(NetworkMessage netmsg)
    {
        NetworkServer.SetClientReady(netmsg.conn);
        Debug.Log(netmsg.conn);
    }
}

