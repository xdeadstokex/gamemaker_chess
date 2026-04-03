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
        CreatePiece(0,0, 1, w); CreatePiece(1,0, 2, w); CreatePiece(2,0, 3, w);
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

    bool OnBoard(int x, int y) {
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
        Cell(x, y).has_piece = 0;
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

                if (mp.attack)
                    HandleAttack(ref attacker, mp.mat_x, mp.mat_y, mp.rect.obj.transform.position);
                else
                    PlaySound(data.mem.moveSound);

                MovePiece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);

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
            case 5: SpawnLinePlates(ref cp, idx, color);
                    SpawnDiagPlates(ref cp, idx, color);      break;
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
        cp.score               = 0;
        cp.score_to_envo       = 0;
        cp.unitType            = PieceType.Light;
        cp.evolved             = 0;
        cp.evolved_type        = 0;
        cp.selected            = 0;
        cp.hovered             = 0;
        cp.hover_sprite_scale  = 1.2f;
        cp.normal_sprite_scale = 0.8f;

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
                case 5: cp.score = 0; cp.score_to_envo = 7;  cp.unitType = PieceType.Core;   break;
                case 1: cp.score = 5; cp.score_to_envo = 10; cp.unitType = PieceType.RHeavy; break;
                case 2: cp.score = 3; cp.score_to_envo = 5;  cp.unitType = PieceType.KHeavy; break;
                case 3: cp.score = 3; cp.score_to_envo = 5;  cp.unitType = PieceType.BHeavy; break;
                case 0: cp.score = 1; cp.score_to_envo = 4;  cp.unitType = PieceType.Light;  break;
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
        }

        if (cp.evolved == 0) return baseMove;

        switch (cp.piece_type) {
            case 2: return baseMove || ValidateEvoKnight(ref cp, tx, ty);
            case 3: return baseMove || ValidateKing(ref cp, tx, ty);
            case 5: return ValidateLine(ref cp, tx, ty) || ValidateDiag(ref cp, tx, ty);
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
        if (cp.score < 0) cp.score = 0;

        if (cp.unitType == PieceType.Light) {
            bool inEnemyHalf = (cp.player_color == 0) ? cp.y >= 4 : cp.y <= 3;
            bool ateHeavy    = victim.unitType == PieceType.KHeavy || victim.unitType == PieceType.BHeavy || victim.unitType == PieceType.RHeavy;
            if (inEnemyHalf && ateHeavy) { EvolveWithWeapon(ref cp, victim.unitType, pos); return; }
        }

        if (cp.score >= cp.score_to_envo) Evolve(ref cp, pos);
    }

    void Evolve(ref data.chess_piece cp, Vector3 pos) {
        if (cp.evolved == 1) return;
        cp.evolved = 1;
        ApplyPieceData(ref cp);
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(pos, 1f);
        Debug.Log($"<color=green>{cp.piece_type} HAS EVOLVED!</color>");
    }

    void EvolveWithWeapon(ref data.chess_piece cp, PieceType weapon, Vector3 pos) {
        cp.evolved      = 1;
        cp.unitType     = PieceType.ELight;
        cp.evolved_type = weapon == PieceType.KHeavy ? 0 : weapon == PieceType.BHeavy ? 1 : 2;
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