using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class HoverSelect : MonoBehaviour, IPointerMoveHandler
    {
        public void OnPointerMove(PointerEventData eventData)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
                return;
            
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}