using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIControl : MonoBehaviour
{
    public static UIControl Instance;

    [Header("Queen Resurrection UI")]
    public GameObject whiteQueenCard; // Kéo thả Image Card Trắng vào đây
    public GameObject blackQueenCard; // Kéo thả Image Card Đen vào đây
    public Text notifyText;           // Hiển thị dòng chữ thông báo (tùy chọn)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Mặc định ẩn các thẻ bài khi mới vào game
        if (whiteQueenCard != null) whiteQueenCard.SetActive(false);
        if (blackQueenCard != null) blackQueenCard.SetActive(false);
    }

    public void ActivateQueenCard(string color)
    {
        if (color == "white")
        {
            whiteQueenCard.SetActive(true);
            ShowMessage("Hậu Trắng đã sẵn sàng hồi sinh! Kéo thẻ vào quân Nặng.");
        }
        else
        {
            blackQueenCard.SetActive(true);
            ShowMessage("Hậu Đen đã sẵn sàng hồi sinh! Kéo thẻ vào quân Nặng.");
        }
    }

    public void ShowMessage(string msg)
    {
        if (notifyText != null)
        {
            notifyText.text = msg;
            // Trí có thể thêm hiệu ứng ẩn chữ sau 3 giây ở đây
        }
    }
    public bool ProcessCardEffect(Card card, Chessman target)
{
    // 1. Tìm script Game để điều khiển bàn cờ
    Game gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

    // 2. Kiểm tra phe (Chỉ dùng thẻ lên quân của mình)
    // Trí nhớ dùng biến 'player' public mà mình vừa sửa ở Chessman nhé
    if (target.player != card.color)
    {
        ShowMessage("Không thể dùng thẻ lên quân địch!");
        return false;
    }

    // 3. Xử lý logic theo loại thẻ (CardType)
    switch (card.type)
    {
        case CardType.Resurrect:
            // KIỂM TRA LUẬT: Target phải là quân Nặng (KHeavy, BHeavy, RHeavy)
            if (target.unitType == PieceType.KHeavy || 
                target.unitType == PieceType.BHeavy || 
                target.unitType == PieceType.RHeavy)
            {
                // Thực hiện hồi sinh: Tạo Hậu mới tại vị trí quân Nặng
                gameScript.Create(card.color + "_queen", target.xBoard, target.yBoard);

                // Hiến tế: Xóa quân Nặng cũ
                Destroy(target.gameObject);

                // Thông báo thành công
                ShowMessage("Hậu Core đã tái sinh!");
                
                // Kết thúc lượt (Mất 1 lượt theo luật của Trí)
                gameScript.NextTurn();
                
                return true; // Trả về true để script Card biết là đã dùng xong thẻ
            }
            else
            {
                ShowMessage("Chỉ có thể hiến tế quân Nặng!");
                return false;
            }

        // Trí có thể thêm các case khác cho các loại thẻ sau này ở đây
        default:
            return false;
    }
}
}
