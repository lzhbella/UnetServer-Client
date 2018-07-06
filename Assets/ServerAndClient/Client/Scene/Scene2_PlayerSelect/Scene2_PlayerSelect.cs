using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//We show all available players on your account and show their data

public class Scene2_PlayerSelect : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField]
    private Text Nick1;
    [SerializeField]
    private Text Nick2;
    [SerializeField]
    private Text Nick3;
    [SerializeField]
    private Button play;
    [Header("CreateChar")]
    [SerializeField]
    private InputField nick;
    [SerializeField]
    private Text StatString;
    [Header("----Select Player Menu----")]
    [SerializeField]
    private Button Select;
    [Header("----Room List Menu----")]
    [SerializeField]
    private Button JoinToRoom;
    [SerializeField]
    private GameObject Room;
    [SerializeField]
    private Transform RoomsList;
    [Header("----Join To Room Menu----")]
    [SerializeField]
    private Text JoinRoomName;
    [SerializeField]
    private Text mapName;
    [SerializeField]
    private Text playersNumber;
    [SerializeField]
    private InputField Password;
    [SerializeField]
    private GameObject PasswordField;
    [Header("----Create To Room Menu----")]
    [SerializeField]
    private InputField RoomName;
    [SerializeField]
    private InputField RoomPassword;
    [Header("System message")]
    [SerializeField]
    private Text text;

    public static List<Data_PlayerFile> temp = new List<Data_PlayerFile>();
    static List<GameObject> RoomsInList = new List<GameObject>();
    /// <summary>
    /// Now selected room
    /// </summary>
    public static RoomData roomSelect;

    GameObject goTemp;

    // Use this for initialization
    void Start()
    {
        Nick1.text = "";
        Nick2.text = "";
        Nick3.text = "";
        Networking_client.net.RegisterHandler(Networking_msgType.CreateChar, HandlerCreateChar);
        Networking_client.net.RegisterHandler(Networking_msgType.RoomsListGet, HandlerLoadRoomsList);
        Networking_client.net.RegisterHandler(Networking_msgType.JoinToRoom, HandlerJoinToRoomError);
    }

    // Update is called once per frame
    void Update()
    {
        if (temp.Count == 3)
        {
            Nick1.text = temp[0].nick;
            Nick2.text = temp[1].nick;
            Nick3.text = temp[2].nick;
        }
        if (temp.Count == 2)
        {
            Nick1.text = temp[0].nick;
            Nick2.text = temp[1].nick;
        }
        if (temp.Count == 1)
        {
            Nick1.text = temp[0].nick;
        }
        if (temp.Count == 0)
        {
            text.text = "Charecter no created. Please create new charecter.";
            play.enabled = true;
        }
        if (temp.Count >= 1)
        {
            if (Data_MyData.PlayerSelect == temp[0].nick)
            {
                StatString.text = string.Format("Name: {0} \r\n\r\nMaxHP: {1}\r\nPlayerScore: {2}", temp[0].nick, temp[0].HPMax, temp[0].PlayerScores);
            }
        }

        if (temp.Count >= 2)
        {
            if (Data_MyData.PlayerSelect == temp[1].nick)
            {
                StatString.text = string.Format("Name: {0} \r\n\r\nMaxHP: {1}\r\nPlayerScore: {2}", temp[1].nick, temp[1].HPMax, temp[1].PlayerScores);
            }
        }

        if (temp.Count == 3)
        {
            if (Data_MyData.PlayerSelect == temp[2].nick)
            {
                StatString.text = string.Format("Name: {0} \r\n\r\nMaxHP: {1}\r\nPlayerScore: {2}", temp[2].nick, temp[2].HPMax, temp[2].PlayerScores);
            }
        }

        SelectButtonState();
        JoinToRoomState();
    }

    /// <summary>
    /// If don't select character nick, disable Select button
    /// </summary>
    void SelectButtonState()
    {
        if (!string.IsNullOrEmpty(Data_MyData.PlayerSelect))
        {
            if (!Select.interactable)
            {
                Select.interactable = true;
            }
        }
        else
        {
            if (Select.interactable)
            {
                Select.interactable = false;
            }
        }
    }

    /// <summary>
    /// If don't select room, disable JoinToRoom button
    /// </summary>
    void JoinToRoomState()
    {
        if (roomSelect)
        {
            if (roomSelect.roomID != -1)
            {
                if (!JoinToRoom.interactable)
                {
                    JoinToRoom.interactable = true;
                }
            }
        }
        else
        {
            if (JoinToRoom.interactable)
            {
                JoinToRoom.interactable = false;
            }
        }
    }

    /// <summary>
    /// Set room data in room info window
    /// </summary>
    public void JoinToRoomOpen()
    {
        if (roomSelect)
        {
            JoinRoomName.text = roomSelect.NameRoom;
            switch (roomSelect.map)
            {
                case 1:
                    mapName.text = "Wasteland";
                    break;

                default:
                    mapName.text = "Unnamed";
                    break;
            }
            Password.text = "";
            if (roomSelect.pass)
            {
                PasswordField.SetActive(true);
            }
            else
            {
                PasswordField.SetActive(false);
            }
            playersNumber.text = string.Format(" {0} / {1}", roomSelect.PlayersNumber, roomSelect.MaxPlayers);
        }
    }

    public void JoinRoom()
    {
        if (Data_MyData.PlayerSelect != "")
        {
            if (roomSelect)
            {
                Message.JoinToRoom Sel = new Message.JoinToRoom();
                Sel.sessionID = Data_MyData.sessionID;
                Sel.playerNick = Data_MyData.PlayerSelect;
                Sel.Login = Data_MyData.Login;
                Sel.Password = Data_MyData.Password;
                Sel.roomID = roomSelect.roomID;
                if (string.IsNullOrEmpty(Password.text))
                {
                    Sel.pass = false;
                }
                else
                {
                    Sel.pass = true;
                    Sel.roomPassword = Password.text;
                }
                Networking_client.net.Send(Networking_msgType.JoinToRoom, Sel);
            }
        }
    }

    void HandlerJoinToRoomError(NetworkMessage netmsg)
    {
        string error = netmsg.reader.ReadString();
        TextMessage(error);
    }

    #region SelectCharMenu
    public void OnCreateChar()
    {
        Message.CreateCh create = new Message.CreateCh();
        create.nick = nick.text;
        Networking_client.net.Send(Networking_msgType.CreateChar, create);
    }

    void HandlerCreateChar(NetworkMessage netmsg)
    {
        Message.CreateCh ch = netmsg.ReadMessage<Message.CreateCh>();
        TextMessage(ch.msg);
    }

    void TextMessage(string txt)
    {
        text.text = txt;
        Invoke("HideMessage", 4);
    }

    void HideMessage()
    {
        text.text = "";
    }
    #endregion

    #region RoomListMenu
    public void GetRoomsList()
    {
        for (int i = 0; i < RoomsInList.Count; i++)
        {
            Destroy(RoomsInList[i]);
        }

        Message.RoomsList getList = new Message.RoomsList();
        Networking_client.net.Send(Networking_msgType.RoomsListGet, getList);
    }

    void HandlerLoadRoomsList(NetworkMessage netMsg)
    {
        Message.RoomsList list = netMsg.ReadMessage<Message.RoomsList>();

        foreach (RoomStr r in list.Rooms)
        {
            goTemp = Instantiate(Room);
            RoomData data = goTemp.GetComponent<RoomData>();
            data.SetRoomData(r.index, r.map, r.name, r.players, r.maxPlayers, r.password);
            goTemp.transform.SetParent(RoomsList, false);

            RoomsInList.Add(goTemp);
        }
    }
    #endregion

    #region Room Create
    public void CreateRoom()
    {
        if (RoomName.text.Length >= 5)
        {
            Message.RoomCreate create = new Message.RoomCreate();
            create.roomName = RoomName.text;
            create.mapID = 1;
            if (RoomPassword.text.Length >= 4)
            {
                create.pass = true;
                create.roomPassword = RoomPassword.text;
            }
            else if (string.IsNullOrEmpty(RoomPassword.text))
            {
                create.pass = false;
            }
            else if (RoomPassword.text.Length < 4)
            {
                TextMessage("Minimum password length 4 symbol");
            }
            Networking_client.net.Send(Networking_msgType.RoomCreate, create);
        }
        else
        {
            TextMessage("Minimum room name length 5 symbol");
        }
    }
    #endregion

    public void Quit()
    {
        Application.Quit();
    }

    public static void NextScen()
    {
        SceneManager.LoadScene(1);
    }
}

public struct RoomStr
{
    public int index;
    public string name;
    public int map;
    public short players;
    public short maxPlayers;
    public bool password;

    public RoomStr(int i, string n, int m, short p, short mP, bool pass)
    {
        index = i;
        name = n;
        map = m;
        players = p;
        maxPlayers = mP;
        password = pass;
    }
}


