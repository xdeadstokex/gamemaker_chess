using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CardType { Resurrect, Swap, Buff } // Thêm các loại thẻ khác ở đây

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Settings")]
    public CardType type;
    public string color; // "white" hoặc "black"
    
    private Vector3 startPos;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startPos = transform.position;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Để tia Raycast xuyên qua thẻ bài xuống bàn cờ
        
        // Hiệu ứng phóng to thẻ khi cầm lên
        transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localScale = new Vector3(1f, 1f, 1f);

        // Bắn tia Raycast xuống thế giới 2D
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null) {
            Chessman target = hit.collider.GetComponent<Chessman>();
            if (target != null) {
                // Gửi thông tin về UIManager để xử lý logic tùy theo loại thẻ
                ExecuteCardEffect(target);
                return;
            }
        }
        
        // Nếu thả không trúng quân cờ nào thì quay về chỗ cũ
        transform.position = startPos;
    }

    void ExecuteCardEffect(Chessman target) {
        // Gọi hàm xử lý tập trung trong UIManager
        bool success = UIControl.Instance.ProcessCardEffect(this, target);
        
        if (success) {
            gameObject.SetActive(false); // Dùng xong thì ẩn thẻ
        } else {
            transform.position = startPos; // Không hợp lệ thì trả về
        }
    }
}