using UnityEngine;

public static class pvp_util {

    // =========================================================
    // CHECK ANY VALID MOVE
    // =========================================================
    public static bool CheckForAnyValidMove(int color) {
        var army = data.mem.get_army(color);

        for (int i = 0; i < army.troop_count; i++) {
            ref var cp = ref army.troop_list[i];
            if (cp.rect == null) continue;

            for (int x = 0; x < data.mem.board_w; x++) {
            for (int y = 0; y < data.mem.board_h; y++) {

                if (!piece_util.can_move_to(ref cp, x, y)) continue;

                bool atk = board_util.Cell(x, y).has_piece == 1;
                if (piece_util.IsSafeMove(i, color, x, y, atk))
                    return true;
            }}
        }
        return false;
    }

    // =========================================================
    // CHECK KING STATE
    // =========================================================
    static bool IsKingChecked(int color) {
        var army = data.mem.get_army(color);

        for (int i = 0; i < army.troop_count; i++) {
            ref var cp = ref army.troop_list[i];

            if (cp.piece_type == 5 && cp.rect != null) {
                return AI_util.IsSquareAttacked(cp.x, cp.y, color);
            }
        }
        return false;
    }

    // =========================================================
    // FIND NEXT ALIVE PLAYER
    // =========================================================
    static int GetNextPlayer(int current) {
        int n = data.mem.total_players;
        int next = (current + 1) % n;

        for (int i = 0; i < n; i++) {
            if (data.mem.armies[next].troop_count > 0)
                return next;

            next = (next + 1) % n;
        }
        return next;
    }

    // =========================================================
    // NEXT TURN
    // =========================================================
    public static void next_player_turn() {

        int next = GetNextPlayer(data.mem.current_player_color);
        data.mem.current_player_color = next;

        bool checkedKing = IsKingChecked(next);
        bool hasMove     = CheckForAnyValidMove(next);

        // reset state
        data.mem.turn_state = 0;

        // =====================================================
        // END GAME
        // =====================================================
        if (!hasMove) {
            data.mem.gameOver = true;
            data.mem.turn_state = checkedKing ? 2 : 3;

            if (data.mem.endSound)
                sound_util.play_sound(data.mem.endSound);

            if (checkedKing) {
                Debug.Log($"<color=red>CHECKMATE! Player {next} lost.</color>");
            } else {
                Debug.Log($"<color=yellow>STALEMATE! Player {next} has no moves.</color>");
            }
            return;
        }

        // =====================================================
        // CHECK
        // =====================================================
        if (checkedKing) {
            data.mem.turn_state = 1;

            if (data.mem.checkSound)
                sound_util.play_sound(data.mem.checkSound);

            Debug.Log($"<color=red>CHECK! Player {next} king is threatened.</color>");
        }
    }
}