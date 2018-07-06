using UnityEngine;

[System.Serializable]
public class Data_PlayerFile : ScriptableObject
{
    public bool death;

	public Vector3 position;
    public Quaternion rotate;
	public string nick;
    public string title;
	public int charID;
    public int rang = 0;
	public int HP;
	public int HPMax;
	public int PlayerScores;

	public int sceneID;
}

[System.Serializable]
public class Data_ItemFile : ScriptableObject
{
    public int index;
    public ItemType ItemType;
    public weaponType weaponType;
    public bool droped;
    public Vector3 pos;
    public Quaternion rotate;
    public int owner;
}

