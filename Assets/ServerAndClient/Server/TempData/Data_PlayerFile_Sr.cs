using UnityEngine;

[System.Serializable]
public class Data_PlayerFile_Sr : ScriptableObject
{
    //For zombie
    public int attackPower = 0;
	public bool PlayerReady = false;

	public Vector3 position;

	public string login;
	public string password;

    public int sessionID;
	public string nick;
    public string title;
    public int rang;
    private int hp;
	public int HP { get { return hp; } }
	public int HPMax;

    public int zombie;
    public int zombieMutant;
    public int zombieStrong;

	public int PlayerScores;

	public int sceneID;

    public void SetHP(int value)
    {
        if (value < 0) { hp = 0; } else { hp = value; }
        if (login != null && !string.IsNullOrEmpty(login) && PlayerReady) { SendPlayerHP(hp); }
    }

    public void Damage(int value)
    {
        hp = hp - value;
        if (hp < 0) { hp = 0; }
        if (login != null && !string.IsNullOrEmpty(login)) { SendPlayerHP(hp); }
    }

    void SendPlayerHP(int hp)
    {
        RoomsManager.SendInt(Networking_msgType_Sr.SetStatHP, hp, sessionID);
    }
}

