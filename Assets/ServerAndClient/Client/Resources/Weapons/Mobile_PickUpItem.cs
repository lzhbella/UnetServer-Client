using UnityEngine;
using UnityEngine.EventSystems;

public class Mobile_PickUpItem : MonoBehaviour, IPointerClickHandler{

    public void OnPointerClick(PointerEventData eventData)
    {
        Player_PlayerLocate.TryPickUpItem();
    }

}
