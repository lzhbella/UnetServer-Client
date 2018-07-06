using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//If a player selected record his nickname

public class Scene2_NickSelect : MonoBehaviour, IPointerDownHandler
{

	public Text Nick;

	public void OnPointerDown (PointerEventData eventData)
	{
		Data_MyData.PlayerSelect = Nick.text;
	}
}
