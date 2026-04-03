using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    //---AI variable---
    public bool isVsAI = true;
    public int aiColor = 1;
    private bool isAIThinking = false;
    data dataScript;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    public void Awake() {
        dataScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<data>();
    }

    public void Start() {
        data.mem.white_pieces = new data.chess_piece[] {
            CreatePiece(0, 0, 1, 0), // Rook
            CreatePiece(1, 0, 2, 0), // Knight
            CreatePiece(2, 0, 3, 0), // Bishop
            CreatePiece(3, 0, 4, 0), // Queen
            CreatePiece(4, 0, 5, 0), // King
            CreatePiece(5, 0, 3, 0), // Bishop
            CreatePiece(6, 0, 2, 0), // Knight
            CreatePiece(7, 0, 1, 0), // Rook
            CreatePiece(0, 1, 0, 0), CreatePiece(1, 1, 0, 0), CreatePiece(2, 1, 0, 0), CreatePiece(3, 1, 0, 0),
            CreatePiece(4, 1, 0, 0), CreatePiece(5, 1, 0, 0), CreatePiece(6, 1, 0, 0), CreatePiece(7, 1, 0, 0)
        };

        data.mem.black_pieces = new data.chess_piece[] {
            CreatePiece(0, 7, 1, 1), // Rook
            CreatePiece(1, 7, 2, 1), // Knight
            CreatePiece(2, 7, 3, 1), // Bishop
            CreatePiece(3, 7, 4, 1), // Queen
            CreatePiece(4, 7, 5, 1), // King
            CreatePiece(5, 7, 3, 1), // Bishop
            CreatePiece(6, 7, 2, 1), // Knight
            CreatePiece(7, 7, 1, 1), // Rook
            CreatePiece(0, 6, 0, 1), CreatePiece(1, 6, 0, 1), CreatePiece(2, 6, 0, 1), CreatePiece(3, 6, 0, 1),
            CreatePiece(4, 6, 0, 1), CreatePiece(5, 6, 0, 1), CreatePiece(6, 6, 0, 1), CreatePiece(7, 6, 0, 1)
        };

        PlaySound(dataScript.startSound);
    }

    public void Update() {
        if (data.mem.gameOver && Input.GetMouseButtonDown(0)) {
            data.mem.gameOver = false;
            SceneManager.LoadScene("Game");
            return;
        }
        
        //random ai turn
        if(isVsAI && data.mem.current_player_color == aiColor)
        {
            if (!isAIThinking)
            {
                StartCoroutine(PlayRandomAI());
            }
            return;
        }

        HandlePieceInput();
        HandleMovePlateInput();
    }

    // =========================================================================
    // PIECE INPUT
    // =========================================================================

	void HandlePieceInput() {
		int hovered_i = -1, hovered_color = -1;

		for (int color = 0; color <= 1; color++) {
			data.chess_piece[] arr = (color == 0) ? data.mem.white_pieces : data.mem.black_pieces;

			for (int i = 0; i < arr.Length; i++) {
				ref data.chess_piece cp = ref arr[i];
				if (cp.rect == null) continue;

				bool isCurrent = cp.player_color == data.mem.current_player_color;

				// --- SELECT / UNSELECT ---
				if (cp.rect.mouse_unclick == 1) {
					cp.rect.mouse_unclick = 0;

					if (isCurrent) {
						if (cp.selected == 1) {
							// unselect same piece
							cp.selected = 0;
							data.mem.selected_a_piece = 0;
							ClearMovePlates();
						} else {
							// unselect ALL first
							for (int c2 = 0; c2 <= 1; c2++) {
								data.chess_piece[] arr2 = (c2 == 0) ? data.mem.white_pieces : data.mem.black_pieces;
								for (int j = 0; j < arr2.Length; j++) {
									if (arr2[j].rect == null) continue;
									arr2[j].selected = 0;
									arr2[j].hovered = 0;
								}
							}

							// select this one
							data.mem.selected_a_piece = 1;
							cp.selected = 1;

							ClearMovePlates();
							SpawnMovePlates(ref cp, i, color);
						}
					}
				}

				// --- block hover if selected exists ---
				if (data.mem.selected_a_piece == 1) {
					cp.hovered = 0;
					continue;
				}

				if (cp.rect.mouse_hover == 0 && cp.hovered == 1) cp.hovered = 0;

				if (cp.rect.mouse_hover == 1 && cp.selected == 0 && isCurrent && cp.hovered == 0) {
					hovered_i = i;
					hovered_color = color;
				}
			}
		}

		// --- hover logic ---
		if (data.mem.selected_a_piece == 0 && hovered_i >= 0) {
			data.chess_piece[] src = (hovered_color == 0) ? data.mem.white_pieces : data.mem.black_pieces;
			src[hovered_i].hovered = 1;

			ClearMovePlates();
			SpawnMovePlates(ref src[hovered_i], hovered_i, hovered_color);
		}

		// --- scale (visual reset always applied) ---
		for (int color = 0; color <= 1; color++) {
			data.chess_piece[] arr = (color == 0) ? data.mem.white_pieces : data.mem.black_pieces;

			for (int i = 0; i < arr.Length; i++) {
				ref data.chess_piece cp = ref arr[i];
				if (cp.rect == null) continue;

				float s = (cp.selected == 1) ? 1.2f : (cp.hovered == 1 ? 1.15f : 1f);
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
			bool hovered = mp.rect.mouse_hover == 1;
			mp.rect.set_color(hovered ? base_color + new Color(0.4f, 0.4f, 0.4f, 0f) : base_color);
			mp.rect.set_sprite_scale(hovered ? 1.15f : 1f, hovered ? 1.15f : 1f);

			if (mp.rect.mouse_unclick == 1) {
				mp.rect.mouse_unclick = 0;

				data.chess_piece[] arr = (mp.piece_color == 0) ? data.mem.white_pieces : data.mem.black_pieces;
				ref data.chess_piece attacker = ref arr[mp.piece_index];

				if (mp.attack) {
					HandleAttack(ref attacker, mp.mat_x, mp.mat_y, mp.rect.obj.transform.position);
				} else {
					PlaySound(dataScript.moveSound);
				}

				MovePiece(ref attacker, mp.mat_x, mp.mat_y);

				// --- FULL UNSELECT ALL ---
				data.mem.selected_a_piece = 0;
				for (int c = 0; c <= 1; c++) {
					data.chess_piece[] arr2 = (c == 0) ? data.mem.white_pieces : data.mem.black_pieces;
					for (int j = 0; j < arr2.Length; j++) {
						if (arr2[j].rect == null) continue;
						arr2[j].selected = 0;
						arr2[j].hovered = 0;
						arr2[j].rect.set_sprite_scale(1f, 1f);
					}
				}

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
            case 0: PawnPlates(ref cp, idx, color, cp.x, cp.y + pawnDir); break;
            case 1: RookPlates(ref cp, idx, color);   break;
            case 2: KnightPlates(ref cp, idx, color); break;
            case 3: BishopPlates(ref cp, idx, color); break;
            case 4: QueenPlates(ref cp, idx, color);  break;
            case 5: KingPlates(ref cp, idx, color);   break;
        }

        if (cp.evolved == 0) return;

        switch (cp.piece_type) {
            case 0:
                if      (cp.evolved_type == 2) RookPlates(ref cp, idx, color);
                else if (cp.evolved_type == 0) KnightPlates(ref cp, idx, color);
                else if (cp.evolved_type == 1) BishopPlates(ref cp, idx, color);
                break;
            case 2: EvolvedKnightAddon(ref cp, idx, color); break;
            case 3: KingPlates(ref cp, idx, color);         break;
            case 5: QueenPlates(ref cp, idx, color);        break;
        }
    }

    void QueenPlates (ref data.chess_piece cp, int i, int c) { RookPlates(ref cp,i,c); BishopPlates(ref cp,i,c); }
    void RookPlates  (ref data.chess_piece cp, int i, int c) { LinePlates(ref cp,i,c, 1,0); LinePlates(ref cp,i,c,-1,0); LinePlates(ref cp,i,c,0,1); LinePlates(ref cp,i,c,0,-1); }
    void BishopPlates(ref data.chess_piece cp, int i, int c) { LinePlates(ref cp,i,c, 1,1); LinePlates(ref cp,i,c,1,-1); LinePlates(ref cp,i,c,-1,1); LinePlates(ref cp,i,c,-1,-1); }

    void KnightPlates(ref data.chess_piece cp, int i, int c) {
        PointPlate(ref cp,i,c, cp.x+1,cp.y+2); PointPlate(ref cp,i,c, cp.x-1,cp.y+2);
        PointPlate(ref cp,i,c, cp.x+2,cp.y+1); PointPlate(ref cp,i,c, cp.x+2,cp.y-1);
        PointPlate(ref cp,i,c, cp.x+1,cp.y-2); PointPlate(ref cp,i,c, cp.x-1,cp.y-2);
        PointPlate(ref cp,i,c, cp.x-2,cp.y+1); PointPlate(ref cp,i,c, cp.x-2,cp.y-1);
    }

    void EvolvedKnightAddon(ref data.chess_piece cp, int i, int c) {
        PointPlate(ref cp,i,c, cp.x-2,cp.y); PointPlate(ref cp,i,c, cp.x+2,cp.y);
        PointPlate(ref cp,i,c, cp.x,cp.y+2); PointPlate(ref cp,i,c, cp.x,cp.y-2);
    }

    void KingPlates(ref data.chess_piece cp, int i, int c) {
        PointPlate(ref cp,i,c, cp.x,  cp.y+1); PointPlate(ref cp,i,c, cp.x,  cp.y-1);
        PointPlate(ref cp,i,c, cp.x-1,cp.y  ); PointPlate(ref cp,i,c, cp.x+1,cp.y  );
        PointPlate(ref cp,i,c, cp.x-1,cp.y-1); PointPlate(ref cp,i,c, cp.x-1,cp.y+1);
        PointPlate(ref cp,i,c, cp.x+1,cp.y-1); PointPlate(ref cp,i,c, cp.x+1,cp.y+1);
    }

    void LinePlates(ref data.chess_piece cp, int i, int c, int dx, int dy) {
        int x = cp.x + dx, y = cp.y + dy;
        while (OnBoard(x, y) && data.mem.board[x, y].rect == null) {
            SpawnPlate(i, c, x, y, false);
            x += dx; y += dy;
        }
        if (OnBoard(x, y) && data.mem.board[x, y].player_color != cp.player_color)
            SpawnPlate(i, c, x, y, true);
    }

    void PointPlate(ref data.chess_piece cp, int i, int c, int x, int y) {
        if (!OnBoard(x, y)) return;
        ref data.chess_piece target = ref data.mem.board[x, y];
        if (target.rect == null)
            SpawnPlate(i, c, x, y, false);
        else if (target.player_color != cp.player_color)
            SpawnPlate(i, c, x, y, true);
    }

    void PawnPlates(ref data.chess_piece cp, int i, int c, int x, int y) {
        int dir = (cp.player_color == 0) ? 1 : -1;
        if (OnBoard(x, y) && data.mem.board[x, y].rect == null) {
            SpawnPlate(i, c, x, y, false);
            int startRow = (cp.player_color == 0) ? 1 : 6;
            if (cp.y == startRow && OnBoard(x, y + dir) && data.mem.board[x, y + dir].rect == null)
                SpawnPlate(i, c, x, y + dir, false);
        }
        DiagCapture(ref cp, i, c, x + 1, y);
        DiagCapture(ref cp, i, c, x - 1, y);
    }

    void DiagCapture(ref data.chess_piece cp, int i, int c, int x, int y) {
        if (!OnBoard(x, y)) return;
        ref data.chess_piece target = ref data.mem.board[x, y];
        if (target.rect != null && target.player_color != cp.player_color)
            SpawnPlate(i, c, x, y, true);
    }

    void SpawnPlate(int piece_index, int piece_color, int mx, int my, bool isAttack) {
        Sprite sprite = (isAttack && data.mem.mp_attack != null) ? data.mem.mp_attack : data.mem.mp_normal;

        data.move_plate mp;
        mp.rect        = rect_2d.create(BoardToWorld(mx), BoardToWorld(my), -3f);
        mp.attack      = isAttack;
        mp.piece_index = piece_index;
        mp.piece_color = piece_color;
        mp.mat_x       = mx;
        mp.mat_y       = my;

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
        ref data.chess_piece target = ref data.mem.board[tx, ty];
        if (target.rect == null) return;

        AbsorbPoints(ref attacker, ref target, pos);
        PlaySound(dataScript.captureSound);

        if (target.piece_type == 5)
            Winner(target.player_color == 0 ? "black" : "white");

        target.rect.self_destroy();

        // also null it out in the piece array
        data.chess_piece[] arr = (target.player_color == 0) ? data.mem.white_pieces : data.mem.black_pieces;
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i].x == tx && arr[i].y == ty) { arr[i] = default; break; }
        }

        data.mem.board[tx, ty] = default;
    }

    void MovePiece(ref data.chess_piece cp, int tx, int ty) {
        data.mem.board[cp.x, cp.y] = default;
        cp.x = tx;
        cp.y = ty;
        cp.rect.move_to_board(tx, ty, -1f);
        data.mem.board[tx, ty] = cp;
    }

    // =========================================================================
    // PIECE — COORDS / SPRITE
    // =========================================================================

    float BoardToWorld(int v) => v * 1.28f - 4.48f;

    data.chess_piece CreatePiece(int x, int y, int piece_type, int player_color) {
        data.chess_piece cp;
        cp.rect         = rect_2d.create(BoardToWorld(x), BoardToWorld(y), -1f);
        cp.x            = x;
        cp.y            = y;
        cp.piece_type   = piece_type;
        cp.player_color = player_color;
        cp.score        = 0;
        cp.score_to_envo = 0;
        cp.unitType     = PieceType.Light;
        cp.evolved      = 0;
        cp.evolved_type = 0;
        cp.selected     = 0;
        cp.hovered      = 0;

        ApplyPieceData(ref cp);
        data.mem.board[x, y] = cp;
        return cp;
    }




	void ApplyPieceData(ref data.chess_piece cp){
		bool w = (cp.player_color == 0);

		if (cp.evolved == 0){
			switch (cp.piece_type){
				case 4: cp.rect.set_sprite(w ? data.mem.wp_queen : data.mem.bp_queen); cp.score = 9; cp.score_to_envo = 15; cp.unitType = PieceType.Core; break;
				case 5: cp.rect.set_sprite(w ? data.mem.wp_king : data.mem.bp_king); cp.score = 0; cp.score_to_envo = 7; cp.unitType = PieceType.Core; break;
				case 1: cp.rect.set_sprite(w ? data.mem.wp_rook : data.mem.bp_rook); cp.score = 5; cp.score_to_envo = 10; cp.unitType = PieceType.RHeavy; break;
				case 2: cp.rect.set_sprite(w ? data.mem.wp_knight : data.mem.bp_knight); cp.score = 3; cp.score_to_envo = 5; cp.unitType = PieceType.KHeavy; break;
				case 3: cp.rect.set_sprite(w ? data.mem.wp_bishop : data.mem.bp_bishop); cp.score = 3; cp.score_to_envo = 5; cp.unitType = PieceType.BHeavy; break;
				case 0: cp.rect.set_sprite(w ? data.mem.wp_pawn : data.mem.bp_pawn); cp.score = 1; cp.score_to_envo = 4; cp.unitType = PieceType.Light; break;
			}
		}
		else{
			switch (cp.piece_type){
				case 4: cp.rect.set_sprite(w ? data.mem.wp_e_queen : data.mem.bp_e_queen); cp.unitType = PieceType.Core; break;
				case 5: cp.rect.set_sprite(w ? data.mem.wp_e_king : data.mem.bp_e_king); cp.unitType = PieceType.Core; break;
				case 1: cp.rect.set_sprite(w ? data.mem.wp_e_rook : data.mem.bp_e_rook); cp.unitType = PieceType.RHeavy; break;
				case 2: cp.rect.set_sprite(w ? data.mem.wp_e_knight : data.mem.bp_e_knight); cp.unitType = PieceType.KHeavy; break;
				case 3: cp.rect.set_sprite(w ? data.mem.wp_e_bishop : data.mem.bp_e_bishop); cp.unitType = PieceType.BHeavy; break;
				case 0:
					if (cp.evolved_type == 2) cp.rect.set_sprite(w ? data.mem.wp_e_pawn_rook : data.mem.bp_e_pawn_rook);
					else if (cp.evolved_type == 0) cp.rect.set_sprite(w ? data.mem.wp_e_pawn_knight : data.mem.bp_e_pawn_knight);
					else if (cp.evolved_type == 1) cp.rect.set_sprite(w ? data.mem.wp_e_pawn_bishop : data.mem.bp_e_pawn_bishop);
					cp.unitType = PieceType.ELight;
					break;
			}
		}

		cp.rect.fit_collider_to_sprite(cp.rect.sprite);
	}



    // =========================================================================
    // PIECE — MOVE VALIDATION
    // =========================================================================

    bool CanMoveTo(ref data.chess_piece cp, int tx, int ty) {
        if (!OnBoard(tx, ty)) return false;

        ref data.chess_piece target = ref data.mem.board[tx, ty];
        if (target.rect != null && target.player_color == cp.player_color) return false;

        bool baseMove = false;
        switch (cp.piece_type) {
            case 0: baseMove = IsPawnMoveValid(ref cp, tx, ty);                                         break;
            case 1: baseMove = IsLineMoveValid(ref cp, tx, ty);                                         break;
            case 2: baseMove = IsKnightMoveValid(ref cp, tx, ty);                                       break;
            case 3: baseMove = IsDiagonalMoveValid(ref cp, tx, ty);                                     break;
            case 4: baseMove = IsLineMoveValid(ref cp, tx, ty) || IsDiagonalMoveValid(ref cp, tx, ty); break;
            case 5: baseMove = IsKingMoveValid(ref cp, tx, ty);                                         break;
        }

        if (cp.evolved == 0) return baseMove;

        switch (cp.piece_type) {
            case 2: return baseMove || AdditionalIsKnightMoveValid(ref cp, tx, ty);
            case 3: return baseMove || IsKingMoveValid(ref cp, tx, ty);
            case 5: return IsLineMoveValid(ref cp, tx, ty) || IsDiagonalMoveValid(ref cp, tx, ty);
            default: return baseMove;
        }
    }

    bool IsLineMoveValid(ref data.chess_piece cp, int tx, int ty) {
        if (tx != cp.x && ty != cp.y) return false;
        return !IsBlocked(ref cp, tx, ty);
    }

    bool IsDiagonalMoveValid(ref data.chess_piece cp, int tx, int ty) {
        if (Mathf.Abs(tx - cp.x) != Mathf.Abs(ty - cp.y)) return false;
        return !IsBlocked(ref cp, tx, ty);
    }

    bool IsBlocked(ref data.chess_piece cp, int tx, int ty) {
        int sx = System.Math.Sign(tx - cp.x), sy = System.Math.Sign(ty - cp.y);
        int cx = cp.x + sx, cy = cp.y + sy;
        while (cx != tx || cy != ty) {
            if (data.mem.board[cx, cy].rect != null) return true;
            cx += sx; cy += sy;
        }
        return false;
    }

    bool IsKnightMoveValid(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    bool AdditionalIsKnightMoveValid(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        if ((dx == 2 && dy == 0) || (dx == 0 && dy == 2)) {
            ref data.chess_piece t = ref data.mem.board[tx, ty];
            return t.rect == null || t.player_color != cp.player_color;
        }
        return false;
    }

    bool IsPawnMoveValid(ref data.chess_piece cp, int tx, int ty) {
        int dir = (cp.player_color == 0) ? 1 : -1;
        int dx = tx - cp.x, dy = ty - cp.y;
        if (Mathf.Abs(dx) == 1 && dy == dir) {
            ref data.chess_piece t = ref data.mem.board[tx, ty];
            return t.rect != null && t.player_color != cp.player_color;
        }
        return false;
    }

    bool IsKingMoveValid(ref data.chess_piece cp, int tx, int ty) {
        int dx = Mathf.Abs(tx - cp.x), dy = Mathf.Abs(ty - cp.y);
        if (dx <= 1 && dy <= 1 && (dx + dy > 0)) {
            ref data.chess_piece t = ref data.mem.board[tx, ty];
            return t.rect == null || t.player_color != cp.player_color;
        }
        return false;
    }

    // =========================================================================
    // PIECE — EVOLUTION
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
            PlaySound(dataScript.checkSound);
            Debug.Log("p" + data.mem.current_player_color + " is in CHECK!");
        }
    }

    bool IsKingInCheck(int kingColor) {
        int king_i = FindKing(kingColor);
        if (king_i < 0) return false;

        data.chess_piece[] kings_arr = (kingColor == 0) ? data.mem.white_pieces : data.mem.black_pieces;
        ref data.chess_piece king    = ref kings_arr[king_i];

        data.chess_piece[] enemies = (kingColor == 0) ? data.mem.black_pieces : data.mem.white_pieces;
        for (int i = 0; i < enemies.Length; i++) {
            ref data.chess_piece e = ref enemies[i];
            if (e.rect != null && CanMoveTo(ref e, king.x, king.y)) return true;
        }
        return false;
    }

    int FindKing(int color) {
        data.chess_piece[] arr = (color == 0) ? data.mem.white_pieces : data.mem.black_pieces;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].piece_type == 5 && arr[i].rect != null) return i;
        Debug.LogError("King not found for color: " + color);
        return -1;
    }

    //---AI Random---
    List<data.AIMove> GenerateAllValidMoves(int color){
        List<data.AIMove> moves = new List<data.AIMove>();
        data.chess_piece[] arr = (color == 0) ? data.mem.white_pieces : data.mem.black_pieces; //choose pieces array
        for(int i = 0; i < arr.Length; i++)
        {
            ref data.chess_piece cp = ref arr[i];
            if(cp.rect == null) continue;
            
            for(int tx = 0; tx < 8; tx++)
            {
                for(int ty = 0; ty < 8; ty++)
                {
                    if(CanMoveTo(ref cp, tx, ty))
                    {
                        bool isAttack = data.mem.board[tx, ty].rect != null;
                        moves.Add(new data.AIMove
                        {
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

    void ExecuteAIMove(data.AIMove move)
    {
        data.chess_piece[] arr = (aiColor == 0) ? data.mem.white_pieces : data.mem.black_pieces;
        ref data.chess_piece attacker = ref arr[move.piece_index];

        if (move.isAttack)
        {
            Vector3 targetPos = data.mem.board[move.targetX, move.targetY].rect.obj.transform.position;
            HandleAttack(ref attacker, move.targetX, move.targetY, targetPos);
        }
        else
        {
            PlaySound(dataScript.moveSound);
        }
        MovePiece(ref attacker, move.targetX, move.targetY);

        data.mem.selected_a_piece = 0;
        for(int c = 0; c <= 1; c++) 
        {
            data.chess_piece[] arr2 = (c == 0) ? data.mem.white_pieces : data.mem.black_pieces;
            for(int j = 0; j < arr2.Length; j++) 
            {
                if(arr2[j].rect == null) continue;
                arr2[j].selected = 0;
                arr2[j].hovered = 0;
                arr2[j].rect.set_sprite_scale(1f, 1f);
            }
        }

        ClearMovePlates();
        NextTurn();
    }
    
    IEnumerator PlayRandomAI()
    {
        isAIThinking = true;
        yield return new WaitForSeconds(0.5f); //wait 0.5s
        List<data.AIMove> validMoves = GenerateAllValidMoves(aiColor);

        if(validMoves.Count > 0 && !data.mem.gameOver)
        {
            //list eatable move
            List<data.AIMove> attackMoves = new List<data.AIMove>();
            foreach(var move in validMoves)
            {
                if (move.isAttack) attackMoves.Add(move);
            }
            data.AIMove selectedMove = default;

            if(attackMoves.Count > 0)
            {
                int maxScore = -1;
                List<data.AIMove> bestAttacks = new  List<data.AIMove>();
                foreach(var move in attackMoves)
                {
                    data.chess_piece target = data.mem.board[move.targetX, move.targetY];
                    int targetScore = (target.piece_type == 5) ? 1000 : target.score;
                    if(targetScore > maxScore)
                    {
                        maxScore = targetScore;
                        bestAttacks.Clear();
                        bestAttacks.Add(move);
                    }
                    else if (targetScore == maxScore)
                    {
                        bestAttacks.Add(move);
                    }
                }
                selectedMove = bestAttacks[Random.Range(0, bestAttacks.Count)];
            }
            else
            {
                selectedMove = validMoves[Random.Range(0, validMoves.Count)];
            }
            ExecuteAIMove(selectedMove);
        }
        else if (!data.mem.gameOver)
        {
            Debug.Log("ai so stupid and defeat");
            Winner(aiColor == 0 ? "back" : "white");
        }
        isAIThinking = false;
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    bool OnBoard(int x, int y) => x >= 0 && y >= 0 && x < 8 && y < 8;

    public void PlaySound(AudioClip clip) {
        dataScript.audioSource.PlayOneShot(clip);
    }

    public void Winner(string playerWinner) {
        data.mem.gameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text    = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
}