using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static int touchID = -1;
    bool down;
    public static bool forward;
    public static bool forwardRight;
    public static bool forwardLeft;
    public static bool right;
    public static bool left;
    public static bool backRight;
    public static bool backLeft;
    public static bool back;
    RectTransform rect;
    Vector3 touchPos;
    Vector2 touch;
    Vector2 forwardV = new Vector2(0, 1f);
    float angle;

    void Start()
    {
        rect = (RectTransform)transform.parent;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchID = Input.touchCount-1;
        down = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        touchID = -1;
        down = false;
        forward = false;
        back = false;
        left = false;
        right = false;
    }

    void Update()
    {
        if (down)
        {
            if (touchID < Input.touchCount)
            {
                touchPos.Set(Input.GetTouch(touchID).position.x, Input.GetTouch(touchID).position.y, 0);
                touchPos = touchPos - rect.anchoredPosition3D;
                touch.Set(touchPos.x, touchPos.y);
                angle = Vector2.Angle(touch, forwardV);

                if (angle < 22.5f)
                {
                    forward = true;
                    back = false;
                    left = false;
                    right = false;
                }
                else
                {
                    if (angle < 67.5f)
                    {
                        forward = true;
                        back = false;
                        if (touch.x > 0)
                        {
                            left = false;
                            right = true;
                        }
                        else
                        {
                            left = true;
                            right = false;
                        }
                    }
                    else
                    {
                        forward = false;

                        if (angle < 112.5f)
                        {
                            forward = false;
                            back = false;
                            if (touch.x > 0)
                            {
                                right = true;
                                left = false;
                            }
                            else
                            {
                                left = true;
                                right = false;
                            }

                        }
                        else if (angle < 157.5f)
                        {
                            forward = false;
                            back = true;
                            if (touch.x > 0)
                            {
                                right = true;
                                left = false;
                            }
                            else
                            {
                                left = true;
                                right = false;
                            }
                        }
                        else
                        {
                            forward = false;
                            left = false;
                            right = false;
                            back = true;
                        }
                    }
                }
            }
        }
    }
}
