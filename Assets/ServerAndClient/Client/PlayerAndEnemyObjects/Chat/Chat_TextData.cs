using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Chat_TextData : MonoBehaviour, IPointerClickHandler
{

	public delegate void Click (int id,string nick);

	public static event Click click;

	public string nick = "";
	public int ID = -1;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (ID != -1 && nick != "") {
			click (ID, nick);
		}
	}
}
