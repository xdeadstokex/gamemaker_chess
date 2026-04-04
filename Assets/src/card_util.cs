using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class card_util {


    // =========================================================================
    // CARD
    // =========================================================================

	public static void use_card_on_board(data.Card card, Vector2 screenPos) {
        // 1. Chuyển tọa độ màn hình (UI) sang tọa độ thế giới (World Space)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));

        // 2. Chuyển đổi tọa độ thế giới sang chỉ số ô cờ (x, y)
        // Dựa trên hàm BoardToWorld gốc: v * 1.28f - 4.48f
        int tx = Mathf.RoundToInt((worldPos.x + 4.48f) / 1.28f);
        int ty = Mathf.RoundToInt((worldPos.y + 4.48f) / 1.28f);

        // 3. Kiểm tra xem ô đó có nằm trên bàn cờ và có quân cờ không
        if (board_util.on_board(tx, ty)) {
            ref data.board_cell cell = ref board_util.Cell(tx, ty);
            if (cell.has_piece == 1) {
                // Lấy tham chiếu quân cờ mục tiêu
                data.army_data army = data.mem.get_army(cell.piece_color);
                ref data.chess_piece target = ref army.troop_list[cell.piece_index];

                // 4. Thực thi logic thẻ bài trực tiếp
                card_util.apply_card_effect(card, ref target, worldPos);
                
                // Xóa MovePlates nếu đang hiện để tránh lỗi hiển thị
                move_plate_util.clear_move_plate();
            }
        }
    }

	public static void apply_card_effect(data.Card card, ref data.chess_piece target, Vector3 effectPos) {
        switch (card.type) {
            case CardType.Buff:
                target.score += card.value;
                Debug.Log($"{target.piece_type} được Buff! Score: {target.score}");
                // Kiểm tra tiến hóa sau khi tăng điểm
                if (target.score >= target.score_to_envo) piece_util.evo(ref target, effectPos);
                break;

            case CardType.Debuff:
                target.score -= card.value;
                if (target.score < 0) target.score = 1;
                break;

            case CardType.GodQueen:
                // Logic đặc biệt: Hồi sinh hoặc nâng cấp lên Queen
                if (target.piece_type != 5) { // Không áp dụng lên King
                    target.piece_type = 4;
                    target.evolved = 1;
                    piece_util.apply_piece_data(ref target);
                }
                break;
            case CardType.DemonQueen:
                if (target.piece_type == 4) { // Không áp dụng lên King
                    target.piece_type = 6;
                    target.evolved = 1;
                    piece_util.apply_piece_data(ref target);
                }
                break;
            case CardType.Item:
                if(target.piece_type == 5)
                {
                    target.piece_type = 7; //king cầm súng
                    target.evolved = 1;
                    piece_util.apply_piece_data(ref target);
                }
                break;
            case CardType.Event:
                break;
        }
        sound_util.play_sound(data.mem.startSound); // Âm thanh hiệu ứng
    }

}