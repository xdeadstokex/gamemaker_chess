using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class piece_util {
    // =========================================================================
    // PIECE — CREATE / SPRITE
    // =========================================================================

    // pawn_dir_x / pawn_dir_y: the forward direction for this pawn.
    //   Vertical armies:   (0, +1) faces up,    (0, -1) faces down.
    //   Horizontal armies: (+1, 0) faces right,  (-1, 0) faces left.
    // Default (0,+1) matches old 2P white behaviour so existing callers still compile.
    public static void create_piece(int x, int y, int piece_type, data.army_data army,
                                    int pawn_dir_x = 0, int pawn_dir_y = 1) {
        bool w = army.color == 0;

        data.chess_piece cp = new data.chess_piece();
        cp.rect                = rect_2d.create(board_util.board_to_world(x), board_util.board_to_world(y), -1f);
        cp.x                   = x;
        cp.y                   = y;
        cp.piece_type          = piece_type;
        cp.player_color        = army.color;
        cp.score               = 1;
        cp.score_to_envo       = 0;
        cp.unitType            = PieceType.Light;
        cp.evolved             = 0;
        cp.evolved_type        = 0;
        cp.selected            = 0;
        cp.hovered             = 0;
        cp.hover_sprite_scale  = 1.2f;
        cp.normal_sprite_scale = 0.8f;
        cp.shield              = 0;
        cp.pawn_dir_x          = pawn_dir_x;
        cp.pawn_dir_y          = pawn_dir_y;

        switch (piece_type) {
            case 0: cp.normal_sprite = w ? data.mem.wp_pawn   : data.mem.bp_pawn;
                    cp.evo_sprite0   = w ? data.mem.wp_e_pawn_knight : data.mem.bp_e_pawn_knight;
                    cp.evo_sprite1   = w ? data.mem.wp_e_pawn_bishop : data.mem.bp_e_pawn_bishop;
                    cp.evo_sprite2   = w ? data.mem.wp_e_pawn_rook   : data.mem.bp_e_pawn_rook;
                    break;
            case 1: cp.normal_sprite = w ? data.mem.wp_rook   : data.mem.bp_rook;
                    cp.evo_sprite0   = w ? data.mem.wp_e_rook  : data.mem.bp_e_rook;
                    cp.evo_sprite1   = null; cp.evo_sprite2 = null; break;
            case 2: cp.normal_sprite = w ? data.mem.wp_knight : data.mem.bp_knight;
                    cp.evo_sprite0   = w ? data.mem.wp_e_knight: data.mem.bp_e_knight;
                    cp.evo_sprite1   = null; cp.evo_sprite2 = null; break;
            case 3: cp.normal_sprite = w ? data.mem.wp_bishop : data.mem.bp_bishop;
                    cp.evo_sprite0   = w ? data.mem.wp_e_bishop: data.mem.bp_e_bishop;
                    cp.evo_sprite1   = null; cp.evo_sprite2 = null; break;
            case 4: cp.normal_sprite = w ? data.mem.wp_queen  : data.mem.bp_queen;
                    cp.evo_sprite0   = w ? data.mem.wp_e_queen : data.mem.bp_e_queen;
                    cp.evo_sprite1   = null; cp.evo_sprite2 = null; break;
            case 5: cp.normal_sprite = w ? data.mem.wp_king   : data.mem.bp_king;
                    cp.evo_sprite0   = w ? data.mem.wp_e_king  : data.mem.bp_e_king;
                    cp.evo_sprite1   = null; cp.evo_sprite2 = null; break;
            case 6: cp.normal_sprite = w ? data.mem.wp_e_dqueen : data.mem.bp_e_dqueen;
                    cp.evo_sprite0   = null; cp.evo_sprite1 = null; cp.evo_sprite2 = null; break;
            case 7: cp.normal_sprite = w ? data.mem.wp_e_king : data.mem.bp_e_king;
                    cp.evo_sprite0   = null; cp.evo_sprite1 = null; cp.evo_sprite2 = null; break;
            default:cp.normal_sprite = null;
                    cp.evo_sprite0   = null;
                    cp.evo_sprite1   = null;
                    cp.evo_sprite2   = null; break;
        }

        piece_util.apply_piece_data(ref cp);

        int idx = army.troop_count;
        army.troop_list[army.troop_count++] = cp;
        board_util.set_cell(x, y, army.color, idx);
    }

    public static void apply_piece_data(ref data.chess_piece cp) {
        if (cp.evolved == 0) {
            cp.rect.set_sprite(cp.normal_sprite);
            switch (cp.piece_type) {
                case 4: cp.score = 9; cp.score_to_envo = 15; cp.unitType = PieceType.Core;   break;
                case 5: cp.score = 0; cp.score_to_envo = 1;  cp.unitType = PieceType.Core;   break;
                case 1: cp.score = 5; cp.score_to_envo = 7;  cp.unitType = PieceType.RHeavy; break;
                case 2: cp.score = 3; cp.score_to_envo = 5;  cp.unitType = PieceType.KHeavy; break;
                case 3: cp.score = 3; cp.score_to_envo = 5;  cp.unitType = PieceType.BHeavy; break;
                case 0: cp.score = 1; cp.score_to_envo = 4;  cp.unitType = PieceType.Light;  break;
                case 6: cp.score = 0; cp.score_to_envo = 0;  cp.unitType = PieceType.Core;   break;
            }
        } else {
            Sprite evo = cp.piece_type == 0
                ? (cp.evolved_type == 0 ? cp.evo_sprite0 : cp.evolved_type == 1 ? cp.evo_sprite1 : cp.evo_sprite2)
                : cp.evo_sprite0;
            cp.rect.set_sprite(evo);
            switch (cp.piece_type) {
                case 4: cp.unitType = PieceType.Core;   break;
                case 5: cp.unitType = PieceType.Core;   break;
                case 1: cp.unitType = PieceType.RHeavy; break;
                case 2: cp.unitType = PieceType.KHeavy; break;
                case 3: cp.unitType = PieceType.BHeavy; break;
                case 0: cp.unitType = PieceType.ELight; break;
                case 6: cp.unitType = PieceType.Core; cp.shield = 4; break;
            }
        }
        cp.rect.fit_collider_to_sprite(cp.rect.sprite);
    }


    // =========================================================================
    // MOVE VALIDATION
    // =========================================================================

    public static bool can_move_to(ref data.chess_piece cp, int tx, int ty) {
        if (!board_util.on_board(tx, ty)) return false;
        ref data.board_cell cell = ref board_util.Cell(tx, ty);
        if (cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color == cp.player_color) return false;

        bool baseMove = false;

        switch (cp.piece_type) {
            case 0: baseMove = piece_util.valid_pawn(ref cp, tx, ty);                                                                   break;
            case 1: baseMove = piece_util.valid_line(ref cp, tx, ty);                                                                   break;
            case 2: baseMove = piece_util.valid_knight(ref cp, tx, ty);                                                                 break;
            case 3: baseMove = piece_util.valid_diag(ref cp, tx, ty);                                                                   break;
            case 4: baseMove = piece_util.valid_line(ref cp, tx, ty) || piece_util.valid_diag(ref cp, tx, ty);                          break;
            case 5: baseMove = piece_util.valid_king(ref cp, tx, ty);                                                                   break;
            case 6: baseMove = piece_util.valid_line(ref cp, tx, ty) || piece_util.valid_diag(ref cp, tx, ty) || piece_util.valid_knight(ref cp, tx, ty); break;
        }

        if (cp.evolved == 0) return baseMove;

        switch (cp.piece_type) {
            case 2: return baseMove || piece_util.valid_evo_knight(ref cp, tx, ty);
            case 3: return baseMove || piece_util.valid_king(ref cp, tx, ty);
            case 6: return baseMove || piece_util.valid_knight(ref cp, tx, ty);
            default: return baseMove;
        }
    }


    public static bool valid_line(ref data.chess_piece cp, int tx, int ty) {
        if (tx != cp.x && ty != cp.y) return false;
        return !piece_util.is_blocked(ref cp, tx, ty);
    }

    public static bool valid_diag(ref data.chess_piece cp, int tx, int ty) {
        if (Mathf.Abs(tx - cp.x) != Mathf.Abs(ty - cp.y)) return false;
        return !piece_util.is_blocked(ref cp, tx, ty);
    }

    public static bool is_blocked(ref data.chess_piece cp, int tx, int ty) {
        int sx = System.Math.Sign(tx - cp.x), sy = System.Math.Sign(ty - cp.y);
        int cx = cp.x + sx, cy = cp.y + sy;
        while (cx != tx || cy != ty) {
            if (!board_util.on_board(cx, cy)) return true;
            if (board_util.Cell(cx, cy).has_piece == 1) return true;
            cx += sx; cy += sy;
        }
        return false;
    }

    public static bool valid_knight(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    public static bool valid_evo_knight(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return (dx == 2 && dy == 0) || (dx == 0 && dy == 2);
    }

    // Pawn attacks diagonally one step forward.
    // "Forward" is defined by cp.pawn_dir_x / cp.pawn_dir_y stored at spawn time.
    // Works for all 4 orientations on a cross board.
    public static bool valid_pawn(ref data.chess_piece cp, int tx, int ty) {
        int dx  = tx - cp.x;
        int dy  = ty - cp.y;

        // How far along the forward axis?
        int fwd  = dx * cp.pawn_dir_x + dy * cp.pawn_dir_y;

        // How far along the perpendicular axis?
        // Because pawn_dir is always an axis-aligned unit vector,
        // the perpendicular component is simply the other axis.
        int perp = Mathf.Abs(cp.pawn_dir_x == 0 ? dx : dy);

        // Must be exactly 1 step forward and 1 step sideways.
        if (fwd != 1 || perp != 1) return false;

        ref data.board_cell cell = ref board_util.Cell(tx, ty);
        return cell.has_piece == 1 &&
               data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color != cp.player_color;
    }

    public static bool valid_king(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return dx <= 1 && dy <= 1 && (dx + dy > 0);
    }

    // =========================================================================
    // EVOLUTION
    // =========================================================================

    public static void absorb_point(ref data.chess_piece cp, ref data.chess_piece victim, Vector3 pos) {
        if (cp.evolved == 1) return;

        cp.score += victim.score;
        Debug.Log($"<color=blue>Absorbed {victim.score} points!</color> Total score: {cp.score}");
        if (cp.score < 0) cp.score = 1;
        if (cp.piece_type == 7 && cp.score >= cp.score_to_envo) {
            cp.score = 0;
            return;
        }
        if (cp.unitType == PieceType.Light) {
            // "enemy half" check is direction-aware: positive forward progress past midpoint.
            int progress = cp.x * cp.pawn_dir_x + cp.y * cp.pawn_dir_y;
            int mid      = (cp.pawn_dir_x == 0 ? data.mem.board_h : data.mem.board_w) / 2;
            bool inEnemyHalf = progress >= mid;
            bool ateHeavy    = victim.unitType == PieceType.KHeavy || victim.unitType == PieceType.BHeavy || victim.unitType == PieceType.RHeavy;
            if (inEnemyHalf && ateHeavy) { piece_util.evo_with_weapon(ref cp, victim.unitType, pos); return; }
        }
        if (cp.score >= cp.score_to_envo) piece_util.evo(ref cp, pos);
    }

    public static void evo(ref data.chess_piece cp, Vector3 pos) {
        if (cp.piece_type == 5) {
            cp.piece_type = 7;
            cp.score = 0;
        }
        cp.evolved = 1;
        piece_util.apply_piece_data(ref cp);
        data.mem.evolving_signal = 1;
        data.mem.evolving_pos    = pos;
        Debug.Log($"<color=green>{cp.piece_type} HAS EVOLVED!</color>");
    }

    public static void evo_with_weapon(ref data.chess_piece cp, PieceType weapon, Vector3 pos) {
        cp.evolved  = 1;
        cp.unitType = PieceType.ELight;
        if      (weapon == PieceType.KHeavy) cp.evolved_type = 0;
        else if (weapon == PieceType.BHeavy) cp.evolved_type = 1;
        else if (weapon == PieceType.RHeavy) cp.evolved_type = 2;
        piece_util.apply_piece_data(ref cp);
        data.mem.evolving_signal = 1;
        data.mem.evolving_pos    = pos;
    }

    // =========================================================================
    // ATTACK / MOVE
    // =========================================================================

    public static void piece_attack(ref data.chess_piece attacker, int tx, int ty, Vector3 pos) {
        ref data.board_cell cell = ref board_util.Cell(tx, ty);
        if (cell.has_piece == 0) return;

        data.army_data       enemy  = data.mem.get_army(cell.piece_color);
        ref data.chess_piece target = ref enemy.troop_list[cell.piece_index];

        // DQueen counter-attack: attacker dies, DQueen teleports to attacker's old cell.
        if (target.piece_type == 6) {
            int oldX = attacker.x;
            int oldY = attacker.y;
            attacker.rect.self_destroy();
            attacker.rect = null;
            board_util.Cell(oldX, oldY).has_piece = 0;
            DQueenSkill(ref target, oldX, oldY);
            sound_util.play_sound(data.mem.captureSound);
            return;
        }

        piece_util.absorb_point(ref attacker, ref target, pos);
        sound_util.play_sound(data.mem.captureSound);
        target.rect.self_destroy();
        target.rect = null;
        board_util.clear_cell(tx, ty);
    }

    public static void move_piece(ref data.chess_piece cp, int idx, int color, int tx, int ty) {
        board_util.clear_cell(cp.x, cp.y);
        cp.x = tx;
        cp.y = ty;
        cp.rect.move_to_board(tx, ty, -1f);
        board_util.set_cell(tx, ty, color, idx);
    }

    public static void DQueenSkill(ref data.chess_piece dQueen, int targetX, int targetY) {
        board_util.Cell(dQueen.x, dQueen.y).has_piece = 0;
        dQueen.x = targetX;
        dQueen.y = targetY;
        dQueen.rect.move_to_board(targetX, targetY, -1f);
        board_util.set_cell(targetX, targetY, dQueen.player_color, FindPieceIndex(ref dQueen));
        Debug.Log("<color=purple>DQueen counter-attack: teleported to attacker's cell!</color>");
    }

    public static ref data.chess_piece get_piece_in_board(int x, int y) {
        ref data.board_cell cell = ref board_util.Cell(x, y);
        if (cell.has_piece == 0) return ref data.mem.void_piece;
        return ref data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
    }

    public static int FindPieceIndex(ref data.chess_piece cp) {
        data.army_data army = data.mem.get_army(cp.player_color);
        for (int i = 0; i < army.troop_count; i++) {
            if (System.Object.ReferenceEquals(army.troop_list[i].rect, cp.rect)) return i;
        }
        return -1;
    }

    // Loops all active armies, not just 0 and 1.
    public static void unselect_all_piece() {
        for (int color = 0; color < data.mem.total_players; color++) {
            data.army_data army = data.mem.get_army(color);
            for (int i = 0; i < army.troop_count; i++) {
                army.troop_list[i].selected = 0;
                army.troop_list[i].hovered  = 0;
            }
        }
    }
}