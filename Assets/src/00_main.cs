using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    public void Start() {
        InitBoard(16, 16);
		for(int y = 3; y < 7; y += 1){
		for(int x = 10; x < 14; x += 1){
		    SetCellInvalid(x, y);
		}
		}

        data.army_data w = data.mem.white_army;
        data.army_data b = data.mem.black_army;

        // white back row
        CreatePiece(0,0, 7, w); CreatePiece(1,0, 2, w); CreatePiece(2,0, 3, w);
        CreatePiece(3,0, 4, w); CreatePiece(4,0, 5, w);
        CreatePiece(5,0, 3, w); CreatePiece(6,0, 2, w); CreatePiece(7,0, 1, w);
        for (int i = 0; i < 8; i++) CreatePiece(i, 1, 0, w);

        // black back row
        CreatePiece(0,7, 1, b); CreatePiece(1,7, 2, b); CreatePiece(2,7, 3, b);
        CreatePiece(3,7, 4, b); CreatePiece(4,7, 5, b);
        CreatePiece(5,7, 3, b); CreatePiece(6,7, 2, b); CreatePiece(7,7, 1, b);
        for (int i = 0; i < 8; i++) CreatePiece(i, 6, 0, b);

        PlaySound(data.mem.startSound);
    }

    public void Update() {
        if (data.mem.gameOver && Input.GetMouseButtonDown(0)) {
            data.mem.gameOver = false;
            SceneManager.LoadScene("Game");
            return;
        }

        if (data.mem.isVsAI && data.mem.current_player_color == data.mem.aiColor) {
            if (!data.mem.isAIThinking) {
                StartCoroutine(PlayAITurn());
            }
            return;
        }

        HandlePieceInput();
        HandleMovePlateInput();
    }

    // =========================================================================
    // BOARD INIT
    // =========================================================================

    void InitBoard(int w, int h) {
        data.mem.board_w = w;
        data.mem.board_h = h;
        data.mem.board   = new data.board_cell[w * h];

        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                ref data.board_cell cell = ref Cell(x, y);
                cell.valid     = 1;
                cell.has_piece = 0;

                // assign tile sprite — checkerboard pattern
				if(y < w / 2){
                bool light     = (x + y) % 2 == 0;
                cell.tile_sprite = light ? data.mem.board_tile0 : data.mem.board_tile1;
				}
				else{
                bool light     = (x + y) % 2 == 0;
                cell.tile_sprite = light ? data.mem.board_tile2 : data.mem.board_tile3;
				}
                // spawn tile rect — z=0, behind pieces at z=-1
                cell.tile = rect_2d.create(BoardToWorld(x), BoardToWorld(y), 0f);
                cell.tile.set_sprite(cell.tile_sprite);
                cell.tile.set_sprite_scale(1f, 1f);
                // tile has no collider interaction needed — disable it
                cell.tile.col.enabled = false;
            }
        }
    }

    // punch a hole — marks cell invalid and destroys its tile
    void SetCellInvalid(int x, int y) {
        ref data.board_cell cell = ref Cell(x, y);
        cell.valid = 0;
        if (cell.tile != null) { cell.tile.self_destroy(); cell.tile = null; }
    }

    // =========================================================================
    // BOARD HELPERS
    // =========================================================================

    public bool OnBoard(int x, int y) {
        if (x < 0 || y < 0 || x >= data.mem.board_w || y >= data.mem.board_h) return false;
        return Cell(x, y).valid == 1;
    }

    ref data.board_cell Cell(int x, int y) {
        return ref data.mem.board[x + y * data.mem.board_w];
    }

    data.chess_piece _empty_piece;

    ref data.chess_piece PieceAt(int x, int y) {
        ref data.board_cell cell = ref Cell(x, y);
        if (cell.has_piece == 0) return ref _empty_piece;
        return ref data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
    }

    void SetCell(int x, int y, int color, int idx) {
        ref data.board_cell cell = ref Cell(x, y);
        cell.has_piece   = 1;
        cell.piece_color = color;
        cell.piece_index = idx;
    }

    void ClearCell(int x, int y) {
        // ref data.board_cell cell = ref Cell(x, y);
        // ref data.chess_piece cp = ref PieceAt(x, y);
        // // Nếu là DQueen (piece_type = 6) thì thực hiện hàm A()
        // if (cp.piece_type == 6) {
        //     DQueenSkill(ref cp, x, y);

        // }
        // else
        // {
            Cell(x, y).has_piece = 0;
        // }
    }
    void DQueenSkill(ref data.chess_piece dQueen, int targetX, int targetY) {
        // 1. Xóa vị trí cũ của DQueen trên board
        Cell(dQueen.x, dQueen.y).has_piece = 0;

        // 2. Cập nhật tọa độ mới (vị trí kẻ địch vừa đứng)
        dQueen.x = targetX;
        dQueen.y = targetY;

        // 3. Cập nhật vị trí hiển thị (Sprite)
        dQueen.rect.move_to_board(targetX, targetY, -1f);

        // 4. Ghi đè DQueen vào ô mới trên mảng Board
        SetCell(targetX, targetY, dQueen.player_color, FindPieceIndex(ref dQueen));
        
        Debug.Log("<color=purple>DQueen phản đòn và chiếm giữ vị trí của kẻ địch!</color>");
    }

    // Helper để tìm index chính xác của quân cờ trong danh sách quân
    int FindPieceIndex(ref data.chess_piece cp) {
        data.army_data army = data.mem.get_army(cp.player_color);
        for (int i = 0; i < army.troop_count; i++) {
            if (System.Object.ReferenceEquals(army.troop_list[i].rect, cp.rect)) return i;
        }
        return -1;
    }
    // =========================================================================
    // PIECE INPUT
    // =========================================================================

    void HandlePieceInput() {
        int hovered_i     = -1;
        int hovered_color = -1;

        for (int color = 0; color <= 1; color++) {
            data.army_data army = data.mem.get_army(color);

            for (int i = 0; i < army.troop_count; i++) {
                ref data.chess_piece cp = ref army.troop_list[i];
                if (cp.rect == null) continue;

                bool isCurrent = cp.player_color == data.mem.current_player_color;

                if (cp.rect.mouse_unclick == 1) {
                    cp.rect.mouse_unclick = 0;

                    if (isCurrent) {
                        if (cp.selected == 1) {
                            cp.selected = 0;
                            data.mem.selected_a_piece = 0;
                            ClearMovePlates();
                        } else {
                            UnselectAll();
                            data.mem.selected_a_piece = 1;
                            cp.selected = 1;
                            ClearMovePlates();
                            SpawnMovePlates(ref cp, i, color);
                        }
                    }
                }

                if (data.mem.selected_a_piece == 1) { cp.hovered = 0; continue; }
                if (cp.rect.mouse_hover == 0 && cp.hovered == 1) cp.hovered = 0;
                if (cp.rect.mouse_hover == 1 && cp.selected == 0 && isCurrent && cp.hovered == 0) {
                    hovered_i     = i;
                    hovered_color = color;
                }
            }
        }

        if (data.mem.selected_a_piece == 0 && hovered_i >= 0) {
            data.army_data src = data.mem.get_army(hovered_color);
            src.troop_list[hovered_i].hovered = 1;
            ClearMovePlates();
            SpawnMovePlates(ref src.troop_list[hovered_i], hovered_i, hovered_color);
        }

        // scale pass
        for (int color = 0; color <= 1; color++) {
            data.army_data army = data.mem.get_army(color);
            for (int i = 0; i < army.troop_count; i++) {
                ref data.chess_piece cp = ref army.troop_list[i];
                if (cp.rect == null) continue;
                float s = (cp.selected == 1 || cp.hovered == 1) ? cp.hover_sprite_scale : cp.normal_sprite_scale;
                cp.rect.set_sprite_scale(s, s);
            }
        }
    }

    // =========================================================================
    // MOVEPLATE INPUT
    // =========================================================================

    void HandleMovePlateInput() {
        for (int i = 0; i < data.mem.move_plate_list.Count; i++) {
            data.move_plate mp = data.mem.move_plate_list[i];
            if (mp.rect == null) continue;

            Color base_color = mp.attack ? Color.red : Color.white;
            bool  hovered    = mp.rect.mouse_hover == 1;
            mp.rect.set_color(hovered ? base_color + new Color(0.4f, 0.4f, 0.4f, 0f) : base_color);
            float sc = hovered ? mp.hover_sprite_scale : mp.normal_sprite_scale;
            mp.rect.set_sprite_scale(sc, sc);

            if (mp.rect.mouse_unclick == 1) {
                mp.rect.mouse_unclick = 0;

                data.army_data army = data.mem.get_army(mp.piece_color);
                ref data.chess_piece attacker = ref army.troop_list[mp.piece_index];

                if (!mp.attack)
                    {PlaySound(data.mem.moveSound);
                    MovePiece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);}

                else
                    HandleAttack(ref attacker, mp.mat_x, mp.mat_y, mp.rect.obj.transform.position);
                if (attacker.piece_type != 7){
                MovePiece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);
                }

                data.mem.selected_a_piece = 0;
                UnselectAll();
                NextTurn();
                ClearMovePlates();
                return;
            }
        }
    }

    // =========================================================================
    // MOVEPLATE SPAWN
    // =========================================================================

    void SpawnMovePlates(ref data.chess_piece cp, int idx, int color) {
        int pawnDir = (cp.player_color == 0) ? 1 : -1;

        switch (cp.piece_type) {
            case 0: SpawnPawnPlates(ref cp, idx, color, pawnDir);  break;
            case 1: SpawnLinePlates(ref cp, idx, color);            break;
            case 2: SpawnKnightPlates(ref cp, idx, color);          break;
            case 3: SpawnDiagPlates(ref cp, idx, color);            break;
            case 4: SpawnLinePlates(ref cp, idx, color);
                    SpawnDiagPlates(ref cp, idx, color);            break;
            case 5: SpawnKingPlates(ref cp, idx, color);            break;
            case 6: SpawnLinePlates(ref cp, idx, color);
                    SpawnDiagPlates(ref cp, idx, color);
                    SpawnKnightPlates(ref cp, idx, color);          break;
            case 7: SpawnGunKingPlates(ref cp, idx, color);     break;
        }

        if (cp.evolved == 0) return;

        switch (cp.piece_type) {
            case 0:
                if      (cp.evolved_type == 2) SpawnLinePlates(ref cp, idx, color);
                else if (cp.evolved_type == 0) SpawnKnightPlates(ref cp, idx, color);
                else if (cp.evolved_type == 1) SpawnDiagPlates(ref cp, idx, color);
                break;
            case 2: SpawnEvoKnightPlates(ref cp, idx, color); break;
            case 3: SpawnKingPlates(ref cp, idx, color);      break;
            case 5: SpawnKingPlates(ref cp, idx, color);     break;
        }
    }

    void SpawnLinePlates(ref data.chess_piece cp, int i, int c) {
        RayPlates(ref cp,i,c,  1, 0, 8,1);
        RayPlates(ref cp,i,c, -1, 0, 8,1);
        RayPlates(ref cp,i,c,  0, 1, 8,1);
        RayPlates(ref cp,i,c,  0,-1, 8,1);
    }

    void SpawnDiagPlates(ref data.chess_piece cp, int i, int c) {
        RayPlates(ref cp,i,c,  1, 1, 8,1);
        RayPlates(ref cp,i,c,  1,-1, 8,1);
        RayPlates(ref cp,i,c, -1, 1, 8,1);
        RayPlates(ref cp,i,c, -1,-1, 8,1);
    }

    void SpawnKingPlates(ref data.chess_piece cp, int i, int c) {
        RayPlates(ref cp,i,c,  1, 0, 1,1); RayPlates(ref cp,i,c, -1, 0, 1,1);
        RayPlates(ref cp,i,c,  0, 1, 1,1); RayPlates(ref cp,i,c,  0,-1, 1,1);
        RayPlates(ref cp,i,c,  1, 1, 1,1); RayPlates(ref cp,i,c,  1,-1, 1,1);
        RayPlates(ref cp,i,c, -1, 1, 1,1); RayPlates(ref cp,i,c, -1,-1, 1,1);
    }
    void SpawnGunKingPlates(ref data.chess_piece cp, int i, int c) {
        // Quét vùng 5x5 xung quanh (từ -2 đến +2)
        for (int xOffset = -2; xOffset <= 2; xOffset++) {
            for (int yOffset = -2; yOffset <= 2; yOffset++) {
                if (xOffset == 0 && yOffset == 0) continue; // Bỏ qua ô trung tâm

                int targetX = cp.x + xOffset;
                int targetY = cp.y + yOffset;

                if (OnBoard(targetX, targetY)) {
                    ref data.board_cell cell = ref Cell(targetX, targetY);
                    if (cell.has_piece == 1) {
                        ref data.chess_piece target = ref PieceAt(targetX, targetY);
                        // Nếu là kẻ địch thì hiện Plate tấn công
                        if (target.player_color != cp.player_color) {
                            SpawnPlate(i, c, targetX, targetY, true);
                        }
                    } else {
                        // Tùy chọn: Vẫn cho phép di chuyển như King bình thường trong 1 ô
                        if (Mathf.Abs(xOffset) <= 1 && Mathf.Abs(yOffset) <= 1) {
                            SpawnPlate(i, c, targetX, targetY, false);
                        }
                    }
                }
            }
        }
    }
    void SpawnKnightPlates(ref data.chess_piece cp, int i, int c) {
        RayPlates(ref cp,i,c,  1, 2, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c, -1, 2, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c,  2, 1, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c,  2,-1, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c,  1,-2, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c, -1,-2, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c, -2, 1, 1,1, skip_obs:true);
        RayPlates(ref cp,i,c, -2,-1, 1,1, skip_obs:true);
    }

    void SpawnEvoKnightPlates(ref data.chess_piece cp, int i, int c) {
        RayPlates(ref cp,i,c,  1, 0, 1,2, skip_obs:true);
        RayPlates(ref cp,i,c, -1, 0, 1,2, skip_obs:true);
        RayPlates(ref cp,i,c,  0, 1, 1,2, skip_obs:true);
        RayPlates(ref cp,i,c,  0,-1, 1,2, skip_obs:true);
    }

    void SpawnPawnPlates(ref data.chess_piece cp, int i, int c, int dir) {
        int startRow = (cp.player_color == 0) ? 1 : 6;
        int steps    = (cp.y == startRow) ? 2 : 1;
        RayPlates(ref cp,i,c,  0, dir, steps, 1, skip_obs:false, capture:false);
        RayPlates(ref cp,i,c,  1, dir, 1,     1, skip_obs:false, capture_only:true);
        RayPlates(ref cp,i,c, -1, dir, 1,     1, skip_obs:false, capture_only:true);
    }

    // =========================================================================
    // CORE RAY
    // =========================================================================

    void RayPlates(ref data.chess_piece cp, int idx, int color,
                   int dir_x, int dir_y, int step_count, int step_jump,
                   bool skip_obs = false, bool capture = true, bool capture_only = false) {

        int x = cp.x + dir_x * step_jump;
        int y = cp.y + dir_y * step_jump;

        for (int s = 0; s < step_count; s++) {
            if (!OnBoard(x, y)) break;

            ref data.board_cell cell = ref Cell(x, y);

            if (cell.has_piece == 1) {
                ref data.chess_piece target = ref data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
                if (!skip_obs) {
                    if (capture && target.player_color != cp.player_color)
                        SpawnPlate(idx, color, x, y, true);
                    break;
                }
                if (target.player_color != cp.player_color)
                    SpawnPlate(idx, color, x, y, true);
            } else {
                if (!capture_only)
                    SpawnPlate(idx, color, x, y, false);
            }

            x += dir_x * step_jump;
            y += dir_y * step_jump;
        }
    }

    void SpawnPlate(int piece_index, int piece_color, int mx, int my, bool isAttack) {
        Sprite sprite = (isAttack && data.mem.mp_attack != null) ? data.mem.mp_attack : data.mem.mp_normal;

        data.move_plate mp;
        mp.rect                = rect_2d.create(BoardToWorld(mx), BoardToWorld(my), -2f); // z=-2, above tile z=0 below piece z=-1
        mp.attack              = isAttack;
        mp.piece_index         = piece_index;
        mp.piece_color         = piece_color;
        mp.mat_x               = mx;
        mp.mat_y               = my;
        mp.hover_sprite_scale  = 1.2f;
        mp.normal_sprite_scale = 0.8f;
        mp.rect.set_sprite(sprite);
        mp.rect.set_color(isAttack ? Color.red : Color.white);
        if (sprite != null) mp.rect.col.size = sprite.bounds.size;

        data.mem.move_plate_list.Add(mp);
    }

    public void ClearMovePlates() {
        foreach (data.move_plate mp in data.mem.move_plate_list)
            if (mp.rect != null) mp.rect.self_destroy();
        data.mem.move_plate_list.Clear();
    }

    // =========================================================================
    // ATTACK / MOVE
    // =========================================================================

    void HandleAttack(ref data.chess_piece attacker, int tx, int ty, Vector3 pos) {
        ref data.board_cell cell = ref Cell(tx, ty);
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
            Cell(oldAttackerX, oldAttackerY).has_piece = 0; 

            // 3. Kích hoạt kỹ năng dịch chuyển của DQueen
            DQueenSkill(ref target, oldAttackerX, oldAttackerY);
            
            PlaySound(data.mem.captureSound);
            return; // Kết thúc hàm sớm, không xóa DQueen
        }
        //dqueen end
        AbsorbPoints(ref attacker, ref target, pos);
        PlaySound(data.mem.captureSound);

        if (target.piece_type == 5)
            Winner(target.player_color == 0 ? "black" : "white");

        target.rect.self_destroy();
        target.rect = null;
        ClearCell(tx, ty);
    }

    void MovePiece(ref data.chess_piece cp, int idx, int color, int tx, int ty) {
        ClearCell(cp.x, cp.y);
        cp.x = tx;
        cp.y = ty;
        cp.rect.move_to_board(tx, ty, -1f);
        SetCell(tx, ty, color, idx);
    }

    // =========================================================================
    // PIECE — CREATE / SPRITE
    // =========================================================================

    float BoardToWorld(int v) => v * 1.28f - 4.48f;

    void CreatePiece(int x, int y, int piece_type, data.army_data army) {
        bool w = army.color == 0;

        data.chess_piece cp;
        cp.rect                = rect_2d.create(BoardToWorld(x), BoardToWorld(y), -1f);
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

        ApplyPieceData(ref cp);

        int idx = army.troop_count;
        army.troop_list[army.troop_count++] = cp;
        SetCell(x, y, army.color, idx);
    }

    void ApplyPieceData(ref data.chess_piece cp) {
    
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

    bool CanMoveTo(ref data.chess_piece cp, int tx, int ty) {
        if (!OnBoard(tx, ty)) return false;
        ref data.board_cell cell = ref Cell(tx, ty);
        if (cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color == cp.player_color) return false;

        int  dir      = (cp.player_color == 0) ? 1 : -1;
        bool baseMove = false;

        switch (cp.piece_type) {
            case 0: baseMove = ValidatePawn(ref cp, tx, ty, dir);                             break;
            case 1: baseMove = ValidateLine(ref cp, tx, ty);                                  break;
            case 2: baseMove = ValidateKnight(ref cp, tx, ty);                                break;
            case 3: baseMove = ValidateDiag(ref cp, tx, ty);                                  break;
            case 4: baseMove = ValidateLine(ref cp, tx, ty) || ValidateDiag(ref cp, tx, ty); break;
            case 5: baseMove = ValidateKing(ref cp, tx, ty);                                  break;
            case 6: baseMove = ValidateLine(ref cp, tx, ty) || ValidateDiag(ref cp, tx, ty) || ValidateKnight(ref cp, tx, ty);                                  break;
        }

        if (cp.evolved == 0) return baseMove;

        switch (cp.piece_type) {
            case 2: return baseMove || ValidateEvoKnight(ref cp, tx, ty);
            case 3: return baseMove || ValidateKing(ref cp, tx, ty);
            case 6: return baseMove || ValidateKnight(ref cp, tx, ty); 
            default: return baseMove;
        }
    }

    bool ValidateLine(ref data.chess_piece cp, int tx, int ty) {
        if (tx != cp.x && ty != cp.y) return false;
        return !IsBlocked(ref cp, tx, ty);
    }

    bool ValidateDiag(ref data.chess_piece cp, int tx, int ty) {
        if (Mathf.Abs(tx - cp.x) != Mathf.Abs(ty - cp.y)) return false;
        return !IsBlocked(ref cp, tx, ty);
    }

    bool IsBlocked(ref data.chess_piece cp, int tx, int ty) {
        int sx = System.Math.Sign(tx - cp.x), sy = System.Math.Sign(ty - cp.y);
        int cx = cp.x + sx, cy = cp.y + sy;
        while (cx != tx || cy != ty) {
            if (!OnBoard(cx, cy)) return true;
            if (Cell(cx, cy).has_piece == 1) return true;
            cx += sx; cy += sy;
        }
        return false;
    }

    bool ValidateKnight(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    bool ValidateEvoKnight(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return (dx == 2 && dy == 0) || (dx == 0 && dy == 2);
    }

    bool ValidatePawn(ref data.chess_piece cp, int tx, int ty, int dir) {
        int dx = tx - cp.x, dy = ty - cp.y;
        if (Mathf.Abs(dx) == 1 && dy == dir) {
            ref data.board_cell cell = ref Cell(tx, ty);
            return cell.has_piece == 1 && data.mem.get_army(cell.piece_color).troop_list[cell.piece_index].player_color != cp.player_color;
        }
        return false;
    }

    bool ValidateKing(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return dx <= 1 && dy <= 1 && (dx + dy > 0);
    }

    // =========================================================================
    // EVOLUTION
    // =========================================================================

    public void AbsorbPoints(ref data.chess_piece cp, ref data.chess_piece victim, Vector3 pos) {
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
            if (inEnemyHalf && ateHeavy) { EvolveWithWeapon(ref cp, victim.unitType, pos); return; }
        }
        if (cp.score >= cp.score_to_envo) Evolve(ref cp, pos);
    }

    void Evolve(ref data.chess_piece cp, Vector3 pos) {     
        if(cp.piece_type == 5) {
            cp.piece_type = 7; 
            cp.score = 0;
        }
        cp.evolved = 1;
        ApplyPieceData(ref cp);
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(pos, 1f);
        Debug.Log($"<color=green>{cp.piece_type} HAS EVOLVED!</color>");
    }

    void EvolveWithWeapon(ref data.chess_piece cp, PieceType weapon, Vector3 pos) {
        cp.evolved      = 1;
        cp.unitType     = PieceType.ELight;
        // cp.evolved_type = weapon == PieceType.KHeavy ? 0 : weapon == PieceType.BHeavy ? 1 : 2;
        if (weapon == PieceType.KHeavy) cp.evolved_type = 0;
        else if (weapon == PieceType.BHeavy) cp.evolved_type = 1;
        else if (weapon == PieceType.RHeavy) cp.evolved_type = 2;
        ApplyPieceData(ref cp);
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(pos, 1f);
    }

    // =========================================================================
    // TURN / CHECK
    // =========================================================================

    public void NextTurn() {
        data.mem.current_player_color = (data.mem.current_player_color == 0) ? 1 : 0;
        if (!data.mem.gameOver && IsKingInCheck(data.mem.current_player_color)) {
            PlaySound(data.mem.checkSound);
            Debug.Log("p" + data.mem.current_player_color + " is in CHECK!");
        }
    }

    bool IsKingInCheck(int kingColor) {
        int king_i = FindKing(kingColor);
        if (king_i < 0) return false;

        ref data.chess_piece king    = ref data.mem.get_army(kingColor).troop_list[king_i];
        data.army_data       enemies = data.mem.get_enemy(kingColor);

        for (int i = 0; i < enemies.troop_count; i++) {
            ref data.chess_piece e = ref enemies.troop_list[i];
            if (e.rect != null && CanMoveTo(ref e, king.x, king.y)) return true;
        }
        return false;
    }

    int FindKing(int color) {
        data.army_data army = data.mem.get_army(color);
        for (int i = 0; i < army.troop_count; i++)
            if (army.troop_list[i].piece_type == 5 && army.troop_list[i].rect != null) return i;
        Debug.LogError("King not found for color: " + color);
        return -1;
    }
    // =========================================================================
    // CARD
    // =========================================================================

    public void UseCardOnBoard(data.Card card, Vector2 screenPos) {
        // 1. Chuyển tọa độ màn hình (UI) sang tọa độ thế giới (World Space)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));

        // 2. Chuyển đổi tọa độ thế giới sang chỉ số ô cờ (x, y)
        // Dựa trên hàm BoardToWorld gốc: v * 1.28f - 4.48f
        int tx = Mathf.RoundToInt((worldPos.x + 4.48f) / 1.28f);
        int ty = Mathf.RoundToInt((worldPos.y + 4.48f) / 1.28f);

        // 3. Kiểm tra xem ô đó có nằm trên bàn cờ và có quân cờ không
        if (OnBoard(tx, ty)) {
            ref data.board_cell cell = ref Cell(tx, ty);
            if (cell.has_piece == 1) {
                // Lấy tham chiếu quân cờ mục tiêu
                data.army_data army = data.mem.get_army(cell.piece_color);
                ref data.chess_piece target = ref army.troop_list[cell.piece_index];

                // 4. Thực thi logic thẻ bài trực tiếp
                ApplyCardEffect(card, ref target, worldPos);
                
                // Xóa MovePlates nếu đang hiện để tránh lỗi hiển thị
                ClearMovePlates();
            }
        }
    }

    private void ApplyCardEffect(data.Card card, ref data.chess_piece target, Vector3 effectPos) {
        switch (card.type) {
            case CardType.Buff:
                target.score += card.value;
                Debug.Log($"{target.piece_type} được Buff! Score: {target.score}");
                // Kiểm tra tiến hóa sau khi tăng điểm
                if (target.score >= target.score_to_envo) Evolve(ref target, effectPos);
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
                    ApplyPieceData(ref target);
                }
                break;
            case CardType.DemonQueen:
                if (target.piece_type == 4) { // Không áp dụng lên King
                    target.piece_type = 6;
                    target.evolved = 1;
                    ApplyPieceData(ref target);
                }
                break;
            case CardType.Item:
                if(target.piece_type == 5)
                {
                    target.piece_type = 7; //king cầm súng
                    target.evolved = 1;
                    ApplyPieceData(ref target);
                }
                break;
            case CardType.Event:
                break;
        }
        PlaySound(data.mem.startSound); // Âm thanh hiệu ứng
    }


    // =========================================================================
    // AI
    // =========================================================================
    List<data.AIMove> GenerateAllValidMoves(int color) {
        List<data.AIMove> moves = new List<data.AIMove>();
        data.army_data army = data.mem.get_army(color);

        for (int i = 0; i < army.troop_count; i++) {
            ref data.chess_piece cp = ref army.troop_list[i];
            
            if (cp.rect == null) continue;

            for (int tx = 0; tx < data.mem.board_w; tx++) {
                for (int ty = 0; ty < data.mem.board_h; ty++) {
                    if (CanMoveTo(ref cp, tx, ty)) {
                        bool isAttack = Cell(tx, ty).has_piece == 1;
                        moves.Add(new data.AIMove {
                            piece_index = i,
                            targetX = tx,
                            targetY = ty,
                            isAttack = isAttack
                        });
                    }
                }
            }
        }
        return moves;
    }

    void ExecuteAIMove(data.AIMove move) {
        data.army_data army = data.mem.get_army(data.mem.aiColor);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];

        if (move.isAttack) {
            Vector3 targetPos = Cell(move.targetX, move.targetY).tile.obj.transform.position;
            HandleAttack(ref attacker, move.targetX, move.targetY, targetPos);
        } else {
            PlaySound(data.mem.moveSound);
        }

        MovePiece(ref attacker, move.piece_index, data.mem.aiColor, move.targetX, move.targetY);

        data.mem.selected_a_piece = 0;
        UnselectAll();
        ClearMovePlates();
        NextTurn();
    }

    data.AIMove CalculateGreedyMove(int color) {
        List<data.AIMove> validMoves = GenerateAllValidMoves(color);
        
        if (validMoves.Count == 0) return new data.AIMove { piece_index = -1 };

        List<data.AIMove> attackMoves = new List<data.AIMove>();
        foreach (var move in validMoves) {
            if (move.isAttack) attackMoves.Add(move);
        }

        if (attackMoves.Count > 0) {
            int maxScore = -1;
            List<data.AIMove> bestAttacks = new List<data.AIMove>();

            foreach (var move in attackMoves) {
                ref data.board_cell cell = ref Cell(move.targetX, move.targetY);
                data.chess_piece target = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
                
                int targetScore = (target.piece_type == 5) ? 1000 : target.score;

                if (targetScore > maxScore) {
                    maxScore = targetScore;
                    bestAttacks.Clear();
                    bestAttacks.Add(move);
                } else if (targetScore == maxScore) {
                    bestAttacks.Add(move);
                }
            }
            return bestAttacks[Random.Range(0, bestAttacks.Count)];
        }

        return validMoves[Random.Range(0, validMoves.Count)];
    }

    IEnumerator PlayAITurn() {
        data.mem.isAIThinking = true;
        
        yield return new WaitForSeconds(0.5f);

        data.AIMove chosenMove = CalculateGreedyMove(data.mem.aiColor);

        if (chosenMove.piece_index != -1 && !data.mem.gameOver) {
            ExecuteAIMove(chosenMove);
        } else if (!data.mem.gameOver) {
            Debug.Log("AI so stupid and defeat");
            Winner(data.mem.aiColor == 0 ? "black" : "white");
        }
        data.mem.isAIThinking = false;
    }


    // =========================================================================
    // HELPERS
    // =========================================================================

    void UnselectAll() {
        for (int color = 0; color <= 1; color++) {
            data.army_data army = data.mem.get_army(color);
            for (int i = 0; i < army.troop_count; i++) {
                army.troop_list[i].selected = 0;
                army.troop_list[i].hovered  = 0;
            }
        }
    }

    public void PlaySound(AudioClip clip) { data.mem.audioSource.PlayOneShot(clip); }

    public void Winner(string playerWinner) {
        data.mem.gameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text    = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
}