using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
public class DragDrop : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler{
    public Card cardData;
    public Image image;
    public Transform parentAfterDrag;


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag ended");

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int tx = Mathf.RoundToInt((mouseWorldPos.x + 4.48f) / 1.28f);
        int ty = Mathf.RoundToInt((mouseWorldPos.y + 4.48f) / 1.28f);

        Game gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (gameScript.PositionOnBoard(tx, ty))
        {
            TryUseCard(tx, ty);
        }
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }
    void TryUseCard(int x, int y) 
    {

        if (cardData.CanActivate(x, y)) 
        {
            cardData.ActivateEffect(x, y);
            Destroy(gameObject, 0.1f);
        }
    }
}   