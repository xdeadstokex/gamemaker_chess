using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class piece_util {
    // =========================================================================
    // PIECE — CREATE / SPRITE
    // =========================================================================
	public static void create_piece(int x, int y, int piece_type, data.army_data army) {
        bool w = army.color == 0;

	    data.chess_piece cp = new data.chess_piece();
        cp.rect                = rect_2d.create(board_util.board_to_world(x), board_util.board_to_world(y), -1f);
        cp.x                   = x;
        cp.y                   = y;
        cp.piece_type          = piece_type;
        cp.player_color        = army.color;
        cp.score               = 1;
        cp.score_to_envo       = 0;
        cp.unitType            = PieceType.Light; // =_= ...
        cp.evolved             = 0;
        cp.evolved_type        = 0;
        cp.selected            = 0;
        cp.hovered             = 0;
        cp.hover_sprite_scale  = 1.2f;
        cp.normal_sprite_scale = 0.8f;
        cp.shield              = 0;

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

	public static void apply_piece_data(ref data.chess_piece cp){
    
        if (cp.evolved == 0) {
            cp.rect.set_sprite(cp.normal_sprite);
            switch (cp.piece_type) {
                case 4: cp.score = 9; cp.score_to_envo = 15; cp.unitType = PieceType.Core;   break;
                case 5: cp.score = 0; cp.score_to_envo = 1;  cp.unitType = PieceType.Core;   break;
                case 1: cp.score = 5; cp.score_to_envo = 7; cp.unitType = PieceType.RHeavy; break;
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
                case 6: cp.unitType = PieceType.Core; cp.shield = 4;   break; //added shield to dqueen
            }
        }
        cp.rect.fit_collider_to_sprite(cp.rect.sprite);
    }


    // =========================================================================
    // MOVE VALIDATION
    // =========================================================================
    public static bool can_move_to(ref data.chess_piece cp, int tx, int ty){
        if (!board_util.on_board(tx, ty)) return false;
        ref data.board_cell cell = ref board_util.Cell(tx, ty);
        
        if (cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color == cp.player_color) 
            return false;

        int  dir      = (cp.player_color == 0) ? 1 : -1;
        bool baseMove = false;


        switch (cp.piece_type) {
            case 0: baseMove = piece_util.valid_pawn(ref cp, tx, ty, dir);                             break;
            case 1: baseMove = piece_util.valid_line(ref cp, tx, ty);                                  break;
            case 2: baseMove = piece_util.valid_knight(ref cp, tx, ty);                                break;
            case 3: baseMove = piece_util.valid_diag(ref cp, tx, ty);                                  break;
            case 4: baseMove = piece_util.valid_line(ref cp, tx, ty) || piece_util.valid_diag(ref cp, tx, ty); break;
            case 5: baseMove = piece_util.valid_king(ref cp, tx, ty);                                  break;
            case 6: baseMove = piece_util.valid_line(ref cp, tx, ty) || piece_util.valid_diag(ref cp, tx, ty) || piece_util.valid_knight(ref cp, tx, ty); break;
            
            case 7: 
                baseMove = piece_util.valid_king(ref cp, tx, ty);
                if (!baseMove && Mathf.Abs(tx - cp.x) <= 2 && Mathf.Abs(ty - cp.y) <= 2) {
                    baseMove = cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color != cp.player_color;
                }
                break;
        }

        if (cp.evolved == 0) return baseMove;

        switch (cp.piece_type) {
            case 0: 
                if (cp.evolved_type == 2) return baseMove || piece_util.valid_line(ref cp, tx, ty);   
                if (cp.evolved_type == 0) return baseMove || piece_util.valid_knight(ref cp, tx, ty); 
                if (cp.evolved_type == 1) return baseMove || piece_util.valid_diag(ref cp, tx, ty);   
                return baseMove;
                
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

    public static bool valid_pawn(ref data.chess_piece cp, int tx, int ty, int dir) {
        int dx = tx - cp.x, dy = ty - cp.y;

        if (Mathf.Abs(dx) == 1 && dy == dir) {
            ref data.board_cell cell = ref board_util.Cell(tx, ty);
            return cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color != cp.player_color;
        }

        if (dx == 0 && dy == dir) {
            return board_util.Cell(tx, ty).has_piece == 0;
        }

        int startRow = (cp.player_color == 0) ? 1 : 6;
        if (dx == 0 && dy == dir * 2 && cp.y == startRow) {
            bool intermediateEmpty = board_util.Cell(cp.x, cp.y + dir).has_piece == 0;
            bool destinationEmpty  = board_util.Cell(tx, ty).has_piece == 0;
            return intermediateEmpty && destinationEmpty;
        }
        return false;
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
        if (cp.piece_type == 7 && cp.score >= cp.score_to_envo) //king có súng nhận điểm tích đạn
        {
            cp.score = 0; // cap score at evo threshold for king to prevent overleveling
            // thêm đạn cho súng của vua
            return;
        } 
        if (cp.unitType == PieceType.Light) {
            bool inEnemyHalf = (cp.player_color == 0) ? cp.y >= 3 : cp.y <= 4; // not half enough but whatever
            bool ateHeavy    = victim.unitType == PieceType.KHeavy || victim.unitType == PieceType.BHeavy || victim.unitType == PieceType.RHeavy;
            if (inEnemyHalf && ateHeavy) { piece_util.evo_with_weapon(ref cp, victim.unitType, pos); return; }
        }
        if (cp.score >= cp.score_to_envo) piece_util.evo(ref cp, pos);
    }

	public static void evo(ref data.chess_piece cp, Vector3 pos){
		if(cp.piece_type == 5) {
			cp.piece_type = 7; 
			cp.score = 0;
		}

		cp.evolved = 1;
		piece_util.apply_piece_data(ref cp);

		data.mem.evolving_signal = 1;
		data.mem.evolving_pos = pos; // STORE POSITION

		Debug.Log($"<color=green>{cp.piece_type} HAS EVOLVED!</color>");
	}

	public static void evo_with_weapon(ref data.chess_piece cp, PieceType weapon, Vector3 pos){
		cp.evolved  = 1;
		cp.unitType = PieceType.ELight;

		if (weapon == PieceType.KHeavy) cp.evolved_type = 0;
		else if (weapon == PieceType.BHeavy) cp.evolved_type = 1;
		else if (weapon == PieceType.RHeavy) cp.evolved_type = 2;

		piece_util.apply_piece_data(ref cp);

		data.mem.evolving_signal = 1;
		data.mem.evolving_pos = pos; // STORE POSITION
	}

    // =========================================================================
    // ATTACK / MOVE
    // =========================================================================

    public static void piece_attack(ref data.chess_piece attacker, int tx, int ty, Vector3 pos) {
        ref data.board_cell cell = ref board_util.Cell(tx, ty);
        if (cell.has_piece == 0) return;

        data.army_data       enemy  = data.mem.get_army(cell.piece_color);
        ref data.chess_piece target = ref enemy.troop_list[cell.piece_index];

        //dqueen start
        if (target.piece_type == 6) {
            // 1. Lưu lại vị trí cũ của kẻ tấn công
            int oldAttackerX = attacker.x;
            int oldAttackerY = attacker.y;

            // 2. Tiêu diệt kẻ tấn công (Phản đòn)
            attacker.rect.self_destroy();
            attacker.rect = null;
            // Xóa kẻ tấn công khỏi ô cờ cũ của nó
            board_util.Cell(oldAttackerX, oldAttackerY).has_piece = 0; 

            // 3. Kích hoạt kỹ năng dịch chuyển của DQueen
            DQueenSkill(ref target, oldAttackerX, oldAttackerY);
            
            sound_util.play_sound(data.mem.captureSound);
            return; // Kết thúc hàm sớm, không xóa DQueen
        }
        //dqueen end
        piece_util.absorb_point(ref attacker, ref target, pos);
        sound_util.play_sound(data.mem.captureSound);

        //if (target.piece_type == 5) Winner(target.player_color == 0 ? "black" : "white");

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
        // 1. Xóa vị trí cũ của DQueen trên board
        board_util.Cell(dQueen.x, dQueen.y).has_piece = 0;

        // 2. Cập nhật tọa độ mới (vị trí kẻ địch vừa đứng)
        dQueen.x = targetX;
        dQueen.y = targetY;

        // 3. Cập nhật vị trí hiển thị (Sprite)
        dQueen.rect.move_to_board(targetX, targetY, -1f);

        // 4. Ghi đè DQueen vào ô mới trên mảng Board
        board_util.set_cell(targetX, targetY, dQueen.player_color, FindPieceIndex(ref dQueen));
        
        Debug.Log("<color=purple>DQueen phản đòn và chiếm giữ vị trí của kẻ địch!</color>");
    }
	

    public static ref data.chess_piece get_piece_in_board(int x, int y) {
        ref data.board_cell cell = ref board_util.Cell(x, y);
        if (cell.has_piece == 0) return ref data.mem.void_piece;
        return ref data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
    }



    // Helper để tìm index chính xác của quân cờ trong danh sách quân
    public static int FindPieceIndex(ref data.chess_piece cp) {
        data.army_data army = data.mem.get_army(cp.player_color);
        for (int i = 0; i < army.troop_count; i++) {
            if (System.Object.ReferenceEquals(army.troop_list[i].rect, cp.rect)) return i;
        }
        return -1;
    }
	

    public static void unselect_all_piece(){
        for (int color = 0; color <= 1; color++) {
            data.army_data army = data.mem.get_army(color);
            for (int i = 0; i < army.troop_count; i++) {
                army.troop_list[i].selected = 0;
                army.troop_list[i].hovered  = 0;
            }
        }
    }
}