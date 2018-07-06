using UnityEngine.Networking;

public class Networking_Chat_Sr : RoomsManager
{
	
	enum ChatMessage : int
	{
		chat = 1,
		privat,
		system
	}

	public static void ChatHandler (NetworkMessage netmsg)
	{
		Message_Sr.Chat_Sr chatR = netmsg.ReadMessage<Message_Sr.Chat_Sr> ();

        if (Networking_OnConnect.AccountVerefication(chatR.index, chatR.log, chatR.pass))
        {
            AccountData acc = Networking_OnConnect.GetAccountData(chatR.index);
            Room room = GetRoom(acc.roomID);

            if (room)
            {
                switch (chatR.msgType)
                {
                    case (int)ChatMessage.chat:
                        Message_Sr.Chat_Sr chatW = new Message_Sr.Chat_Sr();
                        chatW.msgTypeW = (int)ChatMessage.chat;
                        chatW.nickW = GetPlayerData(chatR.index).nick;
                        chatW.id = chatR.index;
                        chatW.msgW = chatR.msg;
                        SendReliableAtRoom(Networking_msgType_Sr.Chat, chatW, chatR.index);
                        break;
                    case (int)ChatMessage.privat:
                        if (chatR.indexPriv == -1)
                        {
                            for (int i = 0; i < room.playersData.Count; i++)
                            {
                                if (room.playersData[i].playerData.nick == chatR.nick)
                                {
                                    chatR.indexPriv = room.playersData[i].sessionID;
                                    break;
                                }
                            }
                        }
                        if (chatR.indexPriv == -1)
                        {
                            NetworkWriter wr = new NetworkWriter();
                            wr.StartMessage(Networking_msgType_Sr.Chat);
                            wr.Write((int)ChatMessage.system);
                            wr.Write("System");
                            wr.Write(-1);
                            wr.Write("This player is offline.");
                            wr.FinishMessage();
                            netmsg.conn.SendWriter(wr, 0);
                        }
                        else
                        {
                            NetworkWriter wr = new NetworkWriter();
                            wr.StartMessage(Networking_msgType_Sr.Chat);
                            wr.Write((int)ChatMessage.privat);
                            wr.Write(GetPlayerData(chatR.indexPriv).nick);
                            wr.Write(chatR.index);
                            wr.Write(chatR.msg);
                            wr.FinishMessage();
                            SendToThisPlayer(wr, chatR.index);

                            wr = new NetworkWriter();
                            wr.StartMessage(Networking_msgType_Sr.Chat);
                            wr.Write((int)ChatMessage.privat);
                            wr.Write(GetPlayerData(chatR.index).nick);
                            wr.Write(chatR.index);
                            wr.Write(chatR.msg);
                            wr.FinishMessage();
                            SendToThisPlayer(wr, chatR.indexPriv);
                        }
                        break;
                }
            }
        }
	}
}


