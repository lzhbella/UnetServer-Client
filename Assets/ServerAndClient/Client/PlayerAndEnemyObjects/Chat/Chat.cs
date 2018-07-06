using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Chat : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Bar")]
    [SerializeField]
    Text Ammo;
    [SerializeField]
    Text Health;
    [Header("Hide elements")]
    public Animator SliderArea;
    Scrollbar scroll;
    float scrollOldValue;
    UnityAction<float> call;
    public Animator Slider;
    public InputField InputRow;
    public Animator ChatFone;
    [Header("Text pre")]
    public GameObject content;
    public GameObject txt;
    [Header("Chat message color")]
    public Color32 DefaultMessageColor;
    public Color32 PrivateMessageColor;
    public Color32 SystemMessageColor;
    string dfChatMsg;
    string sysChatMsg;
    string privateChatMsg;

    public static bool UI;

    string[,] nicks;
    int findIndex;
    string nick;
    //ID next nick write in array
    int WriteNickID = 0;
    //ID last write nick
    int LastWriteNick = 0;

    bool find;
    //Chat window visible
    public static bool visibleChat;
    //Cursor enter in chat window
    bool onChat;
    string priv = "\"";
    bool shift;
    //temp
    int nowNickIDSelect;
    bool addNick;
    private bool newMessage;
    bool chat;

    //weapon ammo
    public static int ammoMax;

    delegate void ChangeInt(int value);
    delegate void ChangeIntInt(int value1, int value2);
    delegate void ChangeFloat(float value);

    static event ChangeInt changeAmmo;
    static event ChangeIntInt changeAmmoMax;
    static event ChangeInt changeHp;
    static event ChangeFloat reload;

    enum ChatMessage : int
    {
        chat = 1,
        privat,
        system
    }

    void Awake()
    {
        changeAmmo += ChangeAmmo;
        changeHp += HealthChange;
        changeAmmoMax += ChangeAmmoMax;
        reload += Reload;
    }

    // Use this for initialization
    void OnEnable()
    {
        AmmoMax(ammoMax, ammoMax);
        //Change first int it is the number saved nicks in array (default = 10)
        nicks = new string[10, 2];
        for (int n = 0; n < nicks.Length / 2; n++)
        {
            nicks[n, 0] = "";
        }
        Networking_client.net.RegisterHandler(Networking_msgType.Chat, ChatHandler);
        Chat_TextData.click += TextClick;
        dfChatMsg = "#" + DefaultMessageColor.r.ToString("X2") + DefaultMessageColor.g.ToString("X2") + DefaultMessageColor.b.ToString("X2");
        privateChatMsg = "#" + PrivateMessageColor.r.ToString("X2") + PrivateMessageColor.g.ToString("X2") + PrivateMessageColor.b.ToString("X2");
        sysChatMsg = "#" + SystemMessageColor.r.ToString("X2") + SystemMessageColor.g.ToString("X2") + SystemMessageColor.b.ToString("X2");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        scroll = SliderArea.GetComponent<Scrollbar>();
    }

    //Mobile metod
    public void ChatOpen()
    {
        chat = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!onChat && visibleChat)
            {
                SliderArea.SetBool("visible", false);
                Slider.SetBool("visible", false);
                ChatFone.SetBool("visible", false);
                InputRow.gameObject.GetComponent<Animator>().SetBool("visible", false);
                InputRow.interactable = false;
                visibleChat = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return) || chat)
        {
            chat = false;
            if (!visibleChat)
            {
                SliderArea.SetBool("visible", true);
                Slider.SetBool("visible", true);
                ChatFone.SetBool("visible", true);
                InputRow.gameObject.GetComponent<Animator>().SetBool("visible", true);
                InputRow.interactable = true;
                visibleChat = true;
                InputRow.ActivateInputField();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (visibleChat)
            {
                string text = InputRow.text;
                if (text != "")
                {
                    if (-1 < text.IndexOf(priv))
                    {
                        string[] data = text.Split(new char[] { ' ' }, 2);
                        for (int i = 0; i < nicks.Length / 2; i++)
                        {
                            if (nicks[i, 0] == data[0])
                            {
                                nick = nicks[i, 0];
                                if (nicks[i, 1] != "")
                                {
                                    findIndex = int.Parse(nicks[i, 1]);
                                    LastWriteNick = i;
                                }
                                else
                                {
                                    findIndex = -1;
                                }
                                find = true;
                            }
                        }
                        if (!find)
                        {
                            nick = data[0].Remove(0, 1);
                            if (WriteNickID >= nicks.Length / 2)
                            {
                                WriteNickID = 0;
                                LastWriteNick = WriteNickID;
                                nicks[WriteNickID, 0] = nick;
                                WriteNickID++;
                            }
                            else
                            {
                                LastWriteNick = WriteNickID;
                                nicks[WriteNickID, 0] = nick;
                                WriteNickID++;
                            }
                            NetworkWriter wr = new NetworkWriter();
                            wr.StartMessage(Networking_msgType.Chat);
                            wr.Write(Data_MyData.charID);
                            wr.Write(Data_MyData.Login);
                            wr.Write(Data_MyData.Password);
                            wr.Write((int)ChatMessage.privat);
                            wr.Write(-1);
                            wr.Write(nick);
                            wr.Write(data[1]);
                            wr.FinishMessage();
                            Networking_client.net.SendWriter(wr, 0);
                            InputRow.text = "";
                            InputRow.DeactivateInputField();
                        }
                        else
                        {
                            NetworkWriter wr = new NetworkWriter();
                            wr.StartMessage(Networking_msgType.Chat);
                            wr.Write(Data_MyData.charID);
                            wr.Write(Data_MyData.Login);
                            wr.Write(Data_MyData.Password);
                            wr.Write((int)ChatMessage.privat);
                            wr.Write(findIndex);
                            wr.Write(nick);
                            wr.Write(data[1]);
                            wr.FinishMessage();
                            Networking_client.net.SendWriter(wr, 0);
                            InputRow.text = "";
                            InputRow.DeactivateInputField();
                            find = false;
                        }
                    }
                    else
                    {
                        NetworkWriter wr = new NetworkWriter();
                        wr.StartMessage(Networking_msgType.Chat);
                        wr.Write(Data_MyData.charID);
                        wr.Write(Data_MyData.Login);
                        wr.Write(Data_MyData.Password);
                        wr.Write((int)ChatMessage.chat);
                        wr.Write(text);
                        wr.FinishMessage();
                        Networking_client.net.SendWriter(wr, 0);
                        InputRow.text = "";
                        InputRow.DeactivateInputField();
                    }
                }
            }
        }
        if (visibleChat && onChat)
        {
            UI = true;
        }
        else
        {
            if (UI)
            {
                UI = false;
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            shift = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            shift = false;
        }
        if (shift)
        {
            if (addNick)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    nowNickIDSelect++;
                    if (nowNickIDSelect >= nicks.Length / 2 - 1)
                    {
                        nowNickIDSelect = 0;
                    }
                    InputRow.DeactivateInputField();
                    if (InputRow.text.Length > 2)
                    {
                        InputRow.text.Remove(1);
                    }
                    InputRow.text = "\"" + nicks[nowNickIDSelect, 0] + " ";
                    InputRow.ActivateInputField();
                    StartCoroutine("MoveTextEnd_NextFrame");
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    nowNickIDSelect--;
                    if (nowNickIDSelect <= -1)
                    {
                        nowNickIDSelect = nicks.Length / 2 - 1;
                    }
                    if (InputRow.text.Length > 2)
                    {
                        InputRow.text.Remove(1);
                    }
                    InputRow.text = "\"" + nicks[nowNickIDSelect, 0] + " ";
                    InputRow.ActivateInputField();
                    StartCoroutine("MoveTextEnd_NextFrame");
                }
            }
        }
        if (!newMessage)
        {
            if (scroll)
            {
                scrollOldValue = scroll.value;
            }
        }
    }

    IEnumerator ScrollNewMessage()
    {
        newMessage = true;
        yield return new WaitForSeconds(0.2f);
        if (scrollOldValue == 0)
        {
            scroll.value = 0;
        }
        scrollOldValue = scroll.value;
        newMessage = false;
    }

    void TextClick(int id, string n)
    {
        if (visibleChat)
        {
            addNick = true;
            InputRow.text = priv + n + " ";
            InputRow.ActivateInputField();
            StartCoroutine("MoveTextEnd_NextFrame");
        }
    }

    IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0;
        InputRow.MoveTextEnd(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onChat = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onChat = false;
    }

    void ChatHandler(NetworkMessage netmsg)
    {
        int msgType = netmsg.reader.ReadInt32();
        string nickR = netmsg.reader.ReadString();
        int id = netmsg.reader.ReadInt32();
        string msg = netmsg.reader.ReadString();

        switch (msgType)
        {
            case (int)ChatMessage.chat:
                GameObject go = Instantiate(txt);
                go.GetComponent<Chat_TextData>().ID = id;
                go.GetComponent<Chat_TextData>().nick = nickR;
                go.GetComponent<Text>().text = string.Format("<color={0}><b>{1}</b>: {2}</color>", dfChatMsg, nickR, msg);
                go.transform.SetParent(content.transform, false);
                StartCoroutine("ScrollNewMessage");
                break;
            case (int)ChatMessage.privat:
                GameObject go2 = Instantiate(txt);
                if (Data_MyData.charID == id)
                {
                    go2.GetComponent<Text>().text = string.Format("<color={0}><-<b>{1}</b>: {2}</color>", privateChatMsg, nickR, msg);
                }
                else
                {
                    go2.GetComponent<Text>().text = string.Format("<color={0}>-><b>{1}</b>: {2}</color>", privateChatMsg, nickR, msg);
                    for (int i = 0; i < nicks.Length / 2; i++)
                    {
                        if (nicks[i, 0] == nickR)
                        {
                            if (nicks[i, 1] == "")
                            {
                                nicks[i, 1] = id.ToString();
                            }
                        }
                    }
                }
                go2.GetComponent<Chat_TextData>().ID = id;
                go2.GetComponent<Chat_TextData>().nick = nickR;
                go2.transform.SetParent(content.transform, false);
                StartCoroutine("ScrollNewMessage");
                break;
            case (int)ChatMessage.system:
                GameObject go3 = Instantiate(txt);
                go3.GetComponent<Text>().text = string.Format("<color={0}><b>{1}</b>: {2}</color>", sysChatMsg, nickR, msg);
                go3.transform.SetParent(content.transform, false);
                StartCoroutine("ScrollNewMessage");
                break;
        }
    }

    public void TextChange()
    {

        if (InputRow.text == priv)
        {
            if (!addNick)
            {
                addNick = true;
                nowNickIDSelect = LastWriteNick;
                InputRow.text = InputRow.text + nicks[LastWriteNick, 0] + " ";
                StartCoroutine("MoveTextEnd_NextFrame");
            }
        }
        if (InputRow.text.Length == 0)
        {
            addNick = false;
        }
    }

    //Ammo bar event

    void ChangeAmmo(int ammo)
    {
        Ammo.text = string.Format("{0} | {1}", ammo, ammoMax);
    }

    void ChangeAmmoMax(int ammo, int maxAmmo)
    { 
        ammoMax = maxAmmo;
        Ammo.text = string.Format("{0} | {1}", ammo, ammoMax);
    }

    void Reload(float time)
    {
        Invoke("ReloadDone", time);
    }

    void ReloadDone()
    {
        Ammo.text = string.Format("{0} | {0}", ammoMax);
    }

    void HealthChange(int hp)
    {
        Health.text = hp.ToString();
    }
    //
    public static void AmmoValue(int ammo)
    {
        changeAmmo(ammo);
    }

    public static void AmmoMax(int ammo, int maxAmmo)
    {
        changeAmmoMax(ammo, maxAmmo);
    }

    public static void ReloadStart(float time)
    {
        reload(time);
    }

    public static void HealthValue(int hp)
    {
        changeHp(hp);
    }
}
