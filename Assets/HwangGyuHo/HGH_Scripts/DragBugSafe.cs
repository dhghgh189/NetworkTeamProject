using UnityEngine;
using UnityEngine.EventSystems;

public class DragBugSafe : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // �巡�� ó�� ����
        if (eventData != null)
            return;
    }
}
