using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_PlayerLocate : MonoBehaviour
{
    [Header("MainMenu")]
    [SerializeField]
    GameObject mainMenu;
    [Header("Respawn button")]
    public GameObject button;
    [Header("Menu window")]
    public GameObject menu;
    public GameObject mobileMenu;
    public GameObject TopList;
    public GameObject MobileTopList;
    public Transform TopListContent;
    public Transform MobileTopListContent;
    public GameObject TopPlayer;
    [Header("Canvas")]
    public GameObject Desctop;
    public GameObject Mobile;

    bool sendFire;
    public GameObject buttonRespawnMobile;

    List<GameObject> topGo = new List<GameObject>();
    //Camera distance
    float y = 5.4f;
    float z = 4.4f;

    //This GameObject stores active dialogue.
    Transform transf;
    Quaternion defaultQuat;
    private Ray ray;
    private RaycastHit[] hits;
    private RaycastHit hit;
    private Vector3 vect;
    Vector2 oldTuoch;
    bool getCam;
    public bool sendW;
    public bool sendS;
    public bool sendA;
    public bool sendD;
    public static bool death;
    bool mobile;
    static Transform lookAtItem;
    float mouseX;

    static Camera cam;
    public static new Camera camera { get { return cam; } }
    bool game = false;

    void Start()
    {
        Networking_client.RegisterHandl += RegisterHandler;
        Networking_client.DisconnectMe += MeDisconnect;
        cam = GetComponent<Camera>();
        defaultQuat = transform.rotation;
    }

    void RegisterHandler()
    {
        Networking_client.net.RegisterHandler(Networking_msgType.TopList, HandleTopList);
    }

    public void GameStart(Transform localPlayer)
    {       
        //If mobile device
#if UNITY_WP_8 || UNITY_WP_8_1 || UNITY_ANDROID || UNITY_IPHONE
        mobile = true;
#endif
        if (mobile)
        {
            //Active mobile canvas
            if (Desctop)
            {
                Destroy(Desctop);
            }
            Mobile.SetActive(true);
        }
        else
        {
            //Active desctop canvas
            Desctop.SetActive(true);
            if (Mobile)
            {
                Destroy(Mobile);
            }
        }

        transf = localPlayer;
        transform.SetParent(transf);
        vect.Set(0, y, -z);
        transform.localPosition = vect;

        game = true;
    }

    void MeDisconnect()
    {
        game = false;
        transform.SetParent(null);
        transform.rotation = defaultQuat;
        if (mobile)
        {
            //Active mobile canvas
            Mobile.SetActive(false);
        }
        else
        {
            //Active desctop canvas
            Desctop.SetActive(false);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        mainMenu.SetActive(true);
    }

    void Update()
    {
        if (game)
        {
            if (!death)
            {
                //If mobile device
                //Use mobile touch controll
                if (mobile)
                {
                    if (MobileMoveButton.forward)
                    {
                        if (!sendW)
                        {
                            SendKeyDown(KeyCode.W, true);
                            sendW = true;
                        }
                    }

                    if (MobileMoveButton.forwardLeft)
                    {
                        if (!sendW)
                        {
                            SendKeyDown(KeyCode.W, true);
                            sendW = true;
                        }
                        if (!sendA)
                        {
                            SendKeyDown(KeyCode.A, true);
                            sendA = true;
                        }
                    }

                    if (MobileMoveButton.forwardRight)
                    {
                        if (!sendW)
                        {
                            SendKeyDown(KeyCode.W, true);
                            sendW = true;
                        }
                        if (!sendD)
                        {
                            SendKeyDown(KeyCode.D, true);
                            sendD = true;
                        }
                    }

                    if (MobileMoveButton.back)
                    {
                        if (!sendS)
                        {
                            SendKeyDown(KeyCode.S, true);
                            sendS = true;
                        }
                    }

                    if (MobileMoveButton.backLeft)
                    {
                        if (!sendS)
                        {
                            SendKeyDown(KeyCode.S, true);
                            sendS = true;
                        }
                        if (!sendA)
                        {
                            SendKeyDown(KeyCode.A, true);
                            sendA = true;
                        }
                    }

                    if (MobileMoveButton.backRight)
                    {
                        if (!sendS)
                        {
                            SendKeyDown(KeyCode.S, true);
                            sendS = true;
                        }
                        if (!sendD)
                        {
                            SendKeyDown(KeyCode.D, true);
                            sendD = true;
                        }
                    }

                    if (MobileMoveButton.left)
                    {
                        if (!sendA)
                        {
                            SendKeyDown(KeyCode.A, true);
                            sendA = true;
                        }
                    }

                    if (MobileMoveButton.right)
                    {
                        if (!sendD)
                        {
                            SendKeyDown(KeyCode.D, true);
                            sendD = true;
                        }
                    }
                }
                //If desctop
                //Use desctop controll WSDA
                else if (!Cursor.visible)
                {
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        if (!sendW)
                        {
                            SendKeyDown(KeyCode.W, true);
                            sendW = true;
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        if (!sendS)
                        {
                            SendKeyDown(KeyCode.S, true);
                            sendS = true;
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        if (!sendA)
                        {
                            SendKeyDown(KeyCode.A, true);
                            sendA = true;
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        if (!sendD)
                        {
                            SendKeyDown(KeyCode.D, true);
                            sendD = true;
                        }
                    }
                    //Fire click
                    if (Input.GetMouseButtonDown(0))
                    {
                        Message.MouseButton shoot = new Message.MouseButton();
                        shoot.down = true;
                        Networking_client.net.Send(Networking_msgType.MouseButton, shoot);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        Message.MouseButton shoot = new Message.MouseButton();
                        shoot.down = false;
                        Networking_client.net.Send(Networking_msgType.MouseButton, shoot);
                    }

                    //Drop weapon function
                    //if (Input.GetKeyDown(KeyCode.G))
                    //{
                    //    Networking_client.SendMyAction(Networking_msgType.DropWeapon);
                    //}

                    //Pick up weapon
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (lookAtItem)
                        {
                            lookAtItem.SendMessage("PickUpThisItem");
                        }
                    }
                }

                //If mobile device
                //Rotate character touching on display
                if (mobile)
                {
                    //If touch and menu not open
                    if (Input.touchCount > 0 && !mobileMenu.activeInHierarchy)
                    {
                        //Touch ID
                        int t = -1;
                        //If this touch not the on move controll
                        if (Input.touchCount == 1 && MobileMoveButton.touchID == -1)
                        {
                            t = 0;
                        }
                        else
                        {
                            //If have touch on move controller and touch count more 2
                            if (Input.touchCount > 1)
                            {
                                if (MobileMoveButton.touchID == 0)
                                {
                                    t = 1;
                                }
                                else
                                {
                                    t = 0;
                                }
                            }
                        }
                        if (t != -1)
                        {
                            //If touch begin. Calculate rotation from this start position.
                            if (Input.GetTouch(t).phase == TouchPhase.Began)
                            {
                                oldTuoch = Input.GetTouch(t).position;

                            }
                            //If touch move. Calculate rotation were 0.15f(default sens).
                            else if (Input.GetTouch(t).phase == TouchPhase.Moved)
                            {
                                if (oldTuoch != Input.GetTouch(t).position)
                                {
                                    mouseX = (Input.GetTouch(t).position.x - oldTuoch.x) * 0.15f;
                                }
                                oldTuoch = Input.GetTouch(t).position;
                            }
                            //If touch ended. Calculate rotation were 0.15f(default sens) and set touchID -1.
                            else if (Input.GetTouch(t).phase == TouchPhase.Ended)
                            {
                                if (oldTuoch != Input.GetTouch(t).position)
                                {
                                    mouseX = (Input.GetTouch(t).position.x - oldTuoch.x) * 0.15f;
                                }
                                oldTuoch = Input.GetTouch(t).position;
                                t = -1;
                            }
                        }
                        else
                        {
                            mouseX = 0;
                        }
                    }
                    else
                    {
                        mouseX = 0;
                    }
                }
                //If desctop. Calculate rotation on mouse asix.
                else
                {
                    if (!menu.activeInHierarchy && !Chat.visibleChat)
                    {
                        mouseX = Input.GetAxis("Mouse X");
                    }
                    else
                    {
                        mouseX = 0;
                    }
                }
                //Apply rotate on client and send axisY 
                vect.Set(transf.eulerAngles.x, transf.eulerAngles.y + mouseX, transf.eulerAngles.z);
                if (transf.eulerAngles != vect)
                {
                    Message.PlayerGoTo go = new Message.PlayerGoTo();
                    go.keySend = false;
                    go.axisY = transf.eulerAngles.y;
                    Networking_client.net.SendUnreliable(Networking_msgType.PlayerMove, go);
                }
                transf.eulerAngles = vect;
            }

            //Off move action on mobile
            if (mobile)
            {
                if (!MobileMoveButton.forward && !MobileMoveButton.forwardLeft && !MobileMoveButton.forwardRight)
                {
                    if (sendW)
                    {
                        SendKeyDown(KeyCode.W, false);
                        sendW = false;
                    }
                }

                if (!MobileMoveButton.forwardLeft && !MobileMoveButton.forward)
                {
                    if (sendW && sendA)
                    {
                        SendKeyDown(KeyCode.W, false);
                        sendW = false;
                        SendKeyDown(KeyCode.A, false);
                        sendA = false;
                    }
                }

                if (!MobileMoveButton.forwardRight && !MobileMoveButton.forward)
                {
                    if (sendW && sendD)
                    {
                        SendKeyDown(KeyCode.W, false);
                        sendW = false;
                        SendKeyDown(KeyCode.D, false);
                        sendD = false;
                    }
                }

                if (!MobileMoveButton.back && !MobileMoveButton.backLeft && !MobileMoveButton.backRight)
                {
                    if (sendS)
                    {
                        SendKeyDown(KeyCode.S, false);
                        sendS = false;
                    }
                }

                if (!MobileMoveButton.backLeft && !MobileMoveButton.back)
                {
                    if (sendS && sendA)
                    {
                        SendKeyDown(KeyCode.S, false);
                        sendS = false;
                        SendKeyDown(KeyCode.A, false);
                        sendA = false;
                    }
                }

                if (!MobileMoveButton.backRight && !MobileMoveButton.back)
                {
                    if (sendS && sendD)
                    {
                        SendKeyDown(KeyCode.S, false);
                        sendS = false;
                        SendKeyDown(KeyCode.D, false);
                        sendD = false;
                    }
                }

                if (!MobileMoveButton.left && !MobileMoveButton.forwardLeft && !MobileMoveButton.backLeft)
                {
                    if (sendA)
                    {
                        SendKeyDown(KeyCode.A, false);
                        sendA = false;
                    }
                }

                if (!MobileMoveButton.right && !MobileMoveButton.forwardRight && !MobileMoveButton.backRight)
                {
                    if (sendD)
                    {
                        SendKeyDown(KeyCode.D, false);
                        sendD = false;
                    }
                }
            }
            //Off move action on desctop
            else
            {
                if (Input.GetKeyUp(KeyCode.D))
                {
                    if (sendD)
                    {
                        SendKeyDown(KeyCode.D, false);
                        sendD = false;
                    }
                }

                if (Input.GetKeyUp(KeyCode.A))
                {
                    if (sendA)
                    {
                        SendKeyDown(KeyCode.A, false);
                        sendA = false;
                    }
                }

                if (Input.GetKeyUp(KeyCode.S))
                {
                    if (sendS)
                    {
                        SendKeyDown(KeyCode.S, false);
                        sendS = false;
                    }
                }

                if (Input.GetKeyUp(KeyCode.W))
                {
                    if (sendW)
                    {
                        SendKeyDown(KeyCode.W, false);
                        sendW = false;
                    }
                }
            }

            //If death set active respawn button
            if (death)
            {
                if (mobile)
                {
                    buttonRespawnMobile.SetActive(true);
                }
                else
                {
                    button.SetActive(true);
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (mobile)
                {
                    buttonRespawnMobile.SetActive(false);
                }
                else
                {
                    button.SetActive(false);
                }
            }

            //Desctop key controll
            if (!mobile)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (menu.activeInHierarchy)
                    {
                        menu.SetActive(false);
                        if (!Chat.visibleChat)
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                    }
                    else
                    {
                        menu.SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    Networking_client.SendMyAction(Networking_msgType.Reload);
                }
                if (!mobile)
                {
                    if (!death && !menu.activeInHierarchy && !Chat.visibleChat)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
            }
        }
    }

    //Mobile metod
    public void FireSend()
    {
        if (!sendFire)
        {
            sendFire = true;
            Message.MouseButton shoot = new Message.MouseButton();
            shoot.down = true;
            Networking_client.net.Send(Networking_msgType.MouseButton, shoot);
        }
    }

    //Mobile metod
    public void StopFire()
    {
        if (sendFire)
        {
            sendFire = false;
            Message.MouseButton shoot = new Message.MouseButton();
            shoot.down = false;
            Networking_client.net.Send(Networking_msgType.MouseButton, shoot);
        }
    }

    //Mobile metod
    public void Reload()
    {
        Networking_client.SendMyAction(Networking_msgType.Reload);
    }

    //Mobile metod
    public void Menu()
    {
        if (mobileMenu.activeInHierarchy)
        {
            mobileMenu.SetActive(false);
        }
        else
        {
            mobileMenu.SetActive(true);
        }
    }

    //If click respawn button
    public void Respawn()
    {
        Message.Respawn repsawn = new Message.Respawn();
        Networking_client.net.Send(Networking_msgType.Respawn, repsawn);
    }

    //Key send metod
    public static void SendKeyDown(KeyCode code, bool down)
    {
        Message.PlayerGoTo go = new Message.PlayerGoTo();
        go.keySend = true;
        go.down = down;
        go.key = (int)code;
        Networking_client.net.Send(Networking_msgType.PlayerMove, go);
    }

    //Try get top list
    public void GetTopList()
    {
        Networking_client.SendMyAction(Networking_msgType.TopList);
        if (mobile)
        {
            MobileTopList.SetActive(true);
        }
        else
        {
            TopList.SetActive(true);
        }
    }

    //If get top list
    public void HandleTopList(NetworkMessage netMsg)
    {
        while (topGo.Count > 0)
        {
            Destroy(topGo[0]);
            topGo.RemoveAt(0);
        }
        int number = netMsg.reader.ReadInt32();
        for (int i = 0; i < number; i++)
        {
            GameObject top = Instantiate(TopPlayer);
            Top_player pl = top.GetComponent<Top_player>();
            pl.nick = netMsg.reader.ReadString();
            pl.z = netMsg.reader.ReadInt32();
            pl.mz = netMsg.reader.ReadInt32();
            pl.sz = netMsg.reader.ReadInt32();
            pl.score = netMsg.reader.ReadInt32();
            if (mobile)
            {
                top.transform.SetParent(MobileTopListContent, false);
            }
            else
            {
                top.transform.SetParent(TopListContent, false);
            }
            top.transform.localPosition = Vector3.zero;
            topGo.Add(top);
        }
    }

    /// <summary>
    /// If mobile controller!
    /// </summary>
    public static void TryPickUpItem()
    {
        if (lookAtItem)
        {
            lookAtItem.SendMessage("PickUpThisItem");
        }
    }

    public void QuitInGame()
    {
        Networking_client.net.Disconnect();
        Application.Quit();
    }

    public static void LookAtObj(Transform obj)
    {
        lookAtItem = obj;
    }
}


