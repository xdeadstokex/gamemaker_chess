using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
public class DragDrop : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;

    public data.Card cardData;
    public void SetupCard(data.Card newData) {
        cardData = newData;
        // if (nameText != null) nameText.text = cardData.cardName;
        // if (descText != null) descText.text = cardData.description;
        if (image != null) cardData.artwork = image.sprite;
    }
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
        // Game gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        // Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // int tx = Mathf   .RoundToInt((mouseWorldPos.x + 4.48f) / 1.28f);
        // int ty = Mathf.RoundToInt((mouseWorldPos.y + 4.48f) / 1.28f);

        // Debug.Log("Drag ended");
        // if (board_util.on_board(tx, ty)) {
        //     card_util.use_card_on_board(cardData, Input.mousePosition);
            
        //     // Xóa thẻ sau khi sử dụng thành công
        //     Destroy(gameObject, 0.1f);
        // } else {
        //     transform.SetParent(parentAfterDrag);
        //     image.raycastTarget = true;
        // }
    }

}   