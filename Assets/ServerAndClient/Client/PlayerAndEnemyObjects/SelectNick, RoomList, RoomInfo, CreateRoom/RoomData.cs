using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomData : MonoBehaviour, IPointerDownHandler {

    [SerializeField]
    private Text roomName;
    [SerializeField]
    private Text mapName;
    [SerializeField]
    private Text Players;
    [SerializeField]
    private Image Password;

    private int RoomID = -1;
    public int roomID { get { return RoomID; } }
    private bool Pass = false;
    public bool pass { get { return Pass; } }
    private int MapID;
    public int map { get { return MapID; } }
    private string nameRoom;
    public string NameRoom { get { return nameRoom; } }
    private int players;
    public int PlayersNumber { get { return players; } }
    private int maxPlayers;
    public int MaxPlayers { get { return maxPlayers; } }
    

    public void SetRoomData(int index, int map, string name, int pl, int plMax, bool password)
    {
        RoomID = index;
        MapID = map;
        nameRoom = name;
        players = pl;
        maxPlayers = plMax;
        Pass = password;
        UpdateData();
    }

    void UpdateData()
    {
        roomName.text = nameRoom;
        mapName.text = map.ToString();
        Players.text = string.Format("{0} / {1}", players, maxPlayers);
        if (!pass)
        {
            Password.enabled = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Scene2_PlayerSelect.roomSelect = this;
    }
}
