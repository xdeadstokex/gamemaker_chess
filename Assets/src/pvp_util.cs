using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class pvp_util {

    //scan for valid move
    public static bool CheckForAnyValidMove(int color) {
        data.army_data army = data.mem.get_army(color);
        
        for (int i = 0; i < army.troop_count; i++) {
            ref data.chess_piece cp = ref army.troop_list[i];
            if (cp.rect == null) continue; //glory for the died one

            for (int tx = 0; tx < data.mem.board_w; tx++) {
                for (int ty = 0; ty < data.mem.board_h; ty++) {
                    
                    if (piece_util.can_move_to(ref cp, tx, ty)) {
                        bool isAttack = board_util.Cell(tx, ty).has_piece == 1;
                        if (piece_util.IsSafeMove(i, color, tx, ty, isAttack)) {
                            return true;//can save
                        }
                    }
                }
            }
        }
        return false;//call ambulance
    }

    public static void next_player_turn() {
        int n = data.mem.total_players;
        int next = (data.mem.current_player_color + 1) % n;
 
        //multiplayer
        for (int i = 0; i < n; i++) {
            if (data.mem.armies[next].troop_count > 0) break;
            next = (next + 1) % n;
        }
 
        data.mem.current_player_color = next;

        //check next player king checked ?
        bool isChecked = false;
        data.army_data army = data.mem.get_army(next);
        
        for (int i = 0; i < army.troop_count; i++) {
            ref data.chess_piece cp = ref army.troop_list[i];
            
            if (cp.piece_type == 5 && cp.rect != null) {
                if (AI_util.IsSquareAttacked(cp.x, cp.y, next)) {
                    isChecked = true;
                    break; 
                }
            }
        }

        // check next player has any valid move
        bool hasValidMoves = CheckForAnyValidMove(next);

        //lose or draw
        if (!hasValidMoves) {
            data.mem.gameOver = true; //gg
            
            if (data.mem.endSound != null) {
                sound_util.play_sound(data.mem.endSound);
            }
            
            //!=========================================
            /*
                make ui for checkmate and draw below
            */
            //!=========================================

            if (isChecked) {
                Debug.Log($"<color=red>CHIẾU HẾT (CHECKMATE)! Phe {next} đã bị dồn vào đường cùng và Thua cuộc!</color>");
            } else {
                Debug.Log($"<color=yellow>HÒA CỜ (STALEMATE)! Phe {next} không bị chiếu nhưng hết nước đi hợp lệ!</color>");
            }
        } 
        else if (isChecked) {
            //is checked but still ok
            if (data.mem.checkSound != null) {
                sound_util.play_sound(data.mem.checkSound);
            }
            Debug.Log($"<color=red>CHIẾU! Phe {next} đang bị đe dọa Vua!</color>");
        }
    }
}