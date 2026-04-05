using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    void Start() {
        ShowMainMenu();
    }

    void Update() {
        HandleMenuInput();

        if (data.mem.game_started == 0) return;

        // Camera drag + zoom
        cam_2d.zoom(mouse_util.scroll);
        if (mouse_util.right.hold == 1)
            cam_2d.move(-mouse_util.dx * 0.3f, -mouse_util.dy * 0.3f);

        zoom_on_evolving_piece();

        // Restart on click after game over
        if (data.mem.gameOver) {
            if (Input.GetMouseButtonDown(0)) {
                data.mem.gameOver = false;
                SceneManager.LoadScene("Game");
            }
            return;
        }

        // AI turn
        if (data.mem.play_against_AI == 1 && data.mem.current_player_color != 0) {
            if (!data.mem.isAIThinking)
                StartCoroutine(AI_util.PlayAITurn());
            return;
        }

        HandlePieceInput();
        HandleMovePlateInput();
        card_util.handle_card_input(data.mem.current_player_color);

        // Debug shortcuts
        if (Input.GetKeyDown(KeyCode.K)) card_util.draw_debug_card();
        if (Input.GetKeyDown(KeyCode.L)) card_util.add_card(1, CardType.DemonQueen);
    }

    // =========================================================================
    // MENU — SHOW
    // =========================================================================

    void MakeMenuBg() {
        data.mem.main_screen_gui = rect_2d.create(0f, 0f);
        data.mem.main_screen_gui.set_sprite(data.mem.rect_2d_sprite);
        data.mem.main_screen_gui.set_sprite_size(20f, 12f);
        data.mem.main_screen_gui.set_collider_size(20f, 12f);
    }

    void ShowMainMenu() {
        gui_util.clear_menu();
        MakeMenuBg();
        data.mem.pvp_button = gui_util.make_button( 2.5f, 0f, Color.cyan,   "PvP");
        data.mem.pve_button = gui_util.make_button(-2.5f, 0f, Color.yellow, "PvE");
        data.mem.menu_state = data.MenuState.Main;
    }
    void ShowPlayerCountMenu(bool is_pve) {
        gui_util.clear_menu();
        MakeMenuBg();
        if (is_pve) {
            data.mem.btn_count1 = gui_util.make_button( 3f, 0f, Color.red,   "1 vs 1");
            data.mem.btn_count2 = gui_util.make_button( 0f, 0f, Color.cyan,  "1 vs 2");
            data.mem.btn_count3 = gui_util.make_button(-3f, 0f, Color.green, "1 vs 3");
            data.mem.menu_state = data.MenuState.PickBotCount;
        } else {
            data.mem.btn_count1 = gui_util.make_button( 3f, 0f, Color.red,   "2 Players");
            data.mem.btn_count2 = gui_util.make_button( 0f, 0f, Color.cyan,  "3 Players");
            data.mem.btn_count3 = gui_util.make_button(-3f, 0f, Color.green, "4 Players");
            data.mem.menu_state = data.MenuState.PickPlayerCount;
        }
        data.mem.back_button = gui_util.make_button(0f, -3f, Color.red, "Back");
    }

    void ShowDifficultyMenu() {
        gui_util.clear_menu();
        MakeMenuBg();
        data.mem.btn_diff1   = gui_util.make_button( 4.5f, 0f, Color.green,  "Baby");
        data.mem.btn_diff2   = gui_util.make_button( 1.5f, 0f, Color.cyan,   "Easy");
        data.mem.btn_diff3   = gui_util.make_button(-1.5f, 0f, Color.yellow, "Normal");
        data.mem.btn_diff4   = gui_util.make_button(-4.5f, 0f, Color.red,    "Asean");
        data.mem.back_button = gui_util.make_button(0f,   -3f, Color.red,    "Back");
        data.mem.menu_state  = data.MenuState.PickBotDifficulty;
    }
    // =========================================================================
    // MENU — INPUT
    // =========================================================================

    void HandleMenuInput() {
        switch (data.mem.menu_state) {
            case data.MenuState.Main:
                if (gui_util.clicked(data.mem.pvp_button)) ShowPlayerCountMenu(false);
                if (gui_util.clicked(data.mem.pve_button)) ShowPlayerCountMenu(true);
                break;

            case data.MenuState.PickPlayerCount:
                if (gui_util.clicked(data.mem.btn_count1)) StartGame(2, 0);
                if (gui_util.clicked(data.mem.btn_count2)) StartGame(3, 0);
                if (gui_util.clicked(data.mem.btn_count3)) StartGame(4, 0);
                if (gui_util.clicked(data.mem.back_button)) ShowMainMenu();
                break;

            case data.MenuState.PickBotCount:
                if (gui_util.clicked(data.mem.btn_count1)) { data.mem.total_players = 2; data.mem.bot_count = 1; ShowDifficultyMenu(); }
                if (gui_util.clicked(data.mem.btn_count2)) { data.mem.total_players = 3; data.mem.bot_count = 2; ShowDifficultyMenu(); }
                if (gui_util.clicked(data.mem.btn_count3)) { data.mem.total_players = 4; data.mem.bot_count = 3; ShowDifficultyMenu(); }
                if (gui_util.clicked(data.mem.back_button)) ShowMainMenu();
                break;

            case data.MenuState.PickBotDifficulty:
                if (gui_util.clicked(data.mem.btn_diff1)) { data.mem.ai_difficulty = AIDifficulty.Baby;   StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff2)) { data.mem.ai_difficulty = AIDifficulty.Easy;   StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff3)) { data.mem.ai_difficulty = AIDifficulty.Normal; StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff4)) { data.mem.ai_difficulty = AIDifficulty.Asean;  StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.back_button)) ShowPlayerCountMenu(true);
                break;
        }
    }

	// =========================================================================
	// GAME START
	// =========================================================================

	void StartGame(int total_players, int bot_count) {
		gui_util.clear_menu();

		data.mem.total_players        = total_players;
		data.mem.bot_count            = bot_count;
		data.mem.current_player_color = 0;
		data.mem.game_started         = 1;
		data.mem.menu_state           = data.MenuState.None;
		data.mem.play_against_AI      = bot_count > 0 ? 1 : 0;

		data.mem.armies = new data.army_data[total_players];
		for (int i = 0; i < total_players; i++)
			data.mem.armies[i] = new data.army_data(i);

		data.mem.white_army = data.mem.armies[0];
		if (total_players > 1)
			data.mem.black_army = data.mem.armies[1];

		if (total_players == 2)
			SetupBoard_2P();
		else
			SetupBoard_Cross(total_players);

		sound_util.play_sound(data.mem.startSound);
        card_util.init_card_table();
        card_util.refresh_card_visuals(0);
    }

	// =========================================================================
	// 2P BOARD
	// =========================================================================

	void SetupBoard_2P() {
		board_util.InitFlat(8, 8);

		int H = data.mem.board_h;

		for (int y = 0; y < 8; y++)
		for (int x = 0; x < 8; x++) {
			ref data.board_cell c = ref board_util.Cell(x, y);

			bool light = (x + y) % 2 == 0;
			c.tile_sprite = light ? data.mem.board_tile0 : data.mem.board_tile1;

			int fy = H - 1 - y;

			c.tile = rect_2d.create(
				board_util.board_to_world(x),
				board_util.board_to_world(fy),
				0f
			);

			c.tile.set_sprite(c.tile_sprite);
			c.tile.set_sprite_scale(1f, 1f);
			c.tile.col.enabled = false;
		}

		int[] back_white = { 7, 2, 3, 4, 5, 3, 2, 1 };
		for (int x = 0; x < 8; x++)
			piece_util.create_piece(x, 0, back_white[x], data.mem.armies[0], 0, +1);
		for (int x = 0; x < 8; x++)
			piece_util.create_piece(x, 1, 0, data.mem.armies[0], 0, +1);

		int[] back_black = { 1, 2, 3, 5, 4, 3, 2, 7 };
		for (int x = 0; x < 8; x++)
			piece_util.create_piece(x, 7, back_black[x], data.mem.armies[1], 0, -1);
		for (int x = 0; x < 8; x++)
			piece_util.create_piece(x, 6, 0, data.mem.armies[1], 0, -1);

		cam_2d.scale(1.2f);
	}


	// =========================================================================
	// CROSS BOARD (3P = SAME AS 4P BOARD)
	// =========================================================================

	const int CROSS_SIZE = 24;
	const int ARM        = 8;

	static bool IsCrossCell(int x, int y) {
		return (x >= ARM && x < ARM * 2) || (y >= ARM && y < ARM * 2);
	}

	void SetupBoard_Cross(int n) {
		board_util.InitFlat(CROSS_SIZE, CROSS_SIZE);

		int H = data.mem.board_h;

		// 🔥 IMPORTANT: NO TILE REMOVAL ANYMORE
		for (int y = 0; y < CROSS_SIZE; y++)
		for (int x = 0; x < CROSS_SIZE; x++) {
			ref data.board_cell c = ref board_util.Cell(x, y);

			if (!IsCrossCell(x, y)) {
				c.valid = 0;
				continue;
			}

			c.valid = 1;

			bool light = (x + y) % 2 == 0;

			Sprite s0, s1;
			if      (y >= ARM * 2) { s0 = data.mem.board_tile0; s1 = data.mem.board_tile1; }
			else if (y < ARM)      { s0 = data.mem.board_tile2; s1 = data.mem.board_tile3; }
			else if (x < ARM)      { s0 = data.mem.board_tile0; s1 = data.mem.board_tile2; }
			else if (x >= ARM * 2) { s0 = data.mem.board_tile1; s1 = data.mem.board_tile3; }
			else                   { s0 = data.mem.board_tile0; s1 = data.mem.board_tile1; }

			int fy = H - 1 - y;

			c.tile_sprite = light ? s0 : s1;
			c.tile = rect_2d.create(
				board_util.board_to_world(x),
				board_util.board_to_world(fy),
				0f
			);

			c.tile.set_sprite(c.tile_sprite);
			c.tile.set_sprite_scale(1f, 1f);
			c.tile.col.enabled = false;
		}

		// ---------------------------------------------------------------------
		// SAME PLACEMENT LOGIC AS 4P
		// ---------------------------------------------------------------------

		// WHITE (bottom)
		PlaceArm_V(data.mem.armies[0], 0, 1, +1);

		// LEFT
		PlaceArm_H(data.mem.armies[n == 3 ? 1 : 2], 0, 1, +1);

		// RIGHT
		PlaceArm_H(data.mem.armies[n == 3 ? 2 : 3], CROSS_SIZE - 1, CROSS_SIZE - 2, -1);

		// TOP ONLY EXISTS IN 4P
		if (n == 4)
			PlaceArm_V(data.mem.armies[1], CROSS_SIZE - 1, CROSS_SIZE - 2, -1);

		cam_2d.set_pos(10.5f, 10.5f);
		cam_2d.scale(3.7f);
	}


	// =========================================================================
	// ARM HELPERS
	// =========================================================================

	static void PlaceArm_V(data.army_data army, int back_row, int pawn_row, int dy) {
		int[] back = { 1, 2, 3, 5, 4, 3, 2, 1 };

		for (int i = 0; i < ARM; i++)
			piece_util.create_piece(ARM + i, back_row, back[i], army, 0, dy);

		for (int i = 0; i < ARM; i++)
			piece_util.create_piece(ARM + i, pawn_row, 0, army, 0, dy);
	}

	static void PlaceArm_H(data.army_data army, int back_col, int pawn_col, int dx) {
		int[] back = { 1, 2, 3, 5, 4, 3, 2, 1 };

		for (int i = 0; i < ARM; i++)
			piece_util.create_piece(back_col, ARM + i, back[i], army, dx, 0);

		for (int i = 0; i < ARM; i++)
			piece_util.create_piece(pawn_col, ARM + i, 0, army, dx, 0);
	}

    // =========================================================================
    // PIECE INPUT
    // =========================================================================

    void HandlePieceInput() {
        int hovered_i     = -1;
        int hovered_color = -1;

        for (int color = 0; color < data.mem.total_players; color++) {
            data.army_data army = data.mem.armies[color];
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
                            move_plate_util.clear_move_plate();
                        } else {
                            piece_util.unselect_all_piece();
                            data.mem.selected_a_piece = 1;
                            cp.selected = 1;
                            move_plate_util.clear_move_plate();
                            move_plate_util.spawn_plate(ref cp, i, color);
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
            data.army_data src = data.mem.armies[hovered_color];
            src.troop_list[hovered_i].hovered = 1;
            move_plate_util.clear_move_plate();
            move_plate_util.spawn_plate(ref src.troop_list[hovered_i], hovered_i, hovered_color);
        }

        // Scale pass
        for (int color = 0; color < data.mem.total_players; color++) {
            data.army_data army = data.mem.armies[color];
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
            bool  hov        = mp.rect.mouse_hover == 1;
            mp.rect.set_color(hov ? base_color + new Color(0.4f, 0.4f, 0.4f, 0f) : base_color);
            float sc = hov ? mp.hover_sprite_scale : mp.normal_sprite_scale;
            mp.rect.set_sprite_scale(sc, sc);

            if (mp.rect.mouse_unclick != 1) continue;
            mp.rect.mouse_unclick = 0;

            data.army_data       army     = data.mem.armies[mp.piece_color];
            ref data.chess_piece attacker = ref army.troop_list[mp.piece_index];

            if (!mp.attack) {
                sound_util.play_sound(data.mem.moveSound);

                if (attacker.piece_type == 0 && Mathf.Abs(mp.mat_y - attacker.y) == 2) {
                    data.mem.en_passant_x = attacker.x;
                    data.mem.en_passant_y = attacker.y + (mp.mat_y - attacker.y) / 2;
                } else {
                    data.mem.en_passant_x = -1;
                    data.mem.en_passant_y = -1;
                }

                if ((attacker.piece_type == 5 || attacker.piece_type == 7) && Mathf.Abs(mp.mat_x - attacker.x) == 2) {
                    bool isKingside = mp.mat_x > attacker.x;
                    int rookX = isKingside ? (attacker.x + 3) : (attacker.x - 4);
                    int newRookX = isKingside ? (attacker.x + 1) : (attacker.x - 1);
                    
                    ref data.board_cell rookCell = ref board_util.Cell(rookX, attacker.y);
                    if (rookCell.has_piece == 1) {
                        ref data.chess_piece rook = ref data.mem.get_army(rookCell.piece_color).troop_list[rookCell.piece_index];
                        piece_util.move_piece(ref rook, rookCell.piece_index, rookCell.piece_color, newRookX, attacker.y);
                        rook.has_moved = 1;
                    }
                }

                attacker.has_moved = 1;
                piece_util.move_piece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);
            } 
            else {
                if (board_util.Cell(mp.mat_x, mp.mat_y).has_piece == 0) {
                    piece_util.piece_attack(ref attacker, mp.mat_x, attacker.y, mp.rect.obj.transform.position); 
                } else {
                    piece_util.piece_attack(ref attacker, mp.mat_x, mp.mat_y, mp.rect.obj.transform.position);
                }
                
                if (attacker.piece_type != 7)
                    piece_util.move_piece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);
                    
                attacker.has_moved = 1;
                data.mem.en_passant_x = -1;
                data.mem.en_passant_y = -1;
            }

            data.mem.selected_a_piece = 0;
            piece_util.unselect_all_piece();
            pvp_util.next_player_turn();
            move_plate_util.clear_move_plate();
            return;
        }
    }

    // =========================================================================
    // EVO ZOOM
    // =========================================================================

    void zoom_on_evolving_piece() {
        if (data.mem.evolving_signal == 1) {
            data.mem.evolving_signal = 0;
            data.mem.evoStartX       = cam_2d.x;
            data.mem.evoStartY       = cam_2d.y;
            data.mem.evoStartSize    = cam_2d.size;
            data.mem.evoTargetSize   = cam_2d.size * 0.7f;
            data.mem.evoTimer        = 0f;
            data.mem.evoZoom         = 1;
            cam_2d.set_pos(data.mem.evolving_pos.x, data.mem.evolving_pos.y);
        }

        if (data.mem.evoZoom != 1) return;

        data.mem.evoTimer += Time.deltaTime;
        float t = data.mem.evoTimer / 0.5f;

        if (t <= 1f) {
            cam_2d.size = Mathf.Lerp(data.mem.evoStartSize, data.mem.evoTargetSize, t);
        } else if (t <= 2f) {
            float k = t - 1f;
            cam_2d.size = Mathf.Lerp(data.mem.evoTargetSize, data.mem.evoStartSize, k);
            cam_2d.x    = Mathf.Lerp(data.mem.evolving_pos.x, data.mem.evoStartX, k);
            cam_2d.y    = Mathf.Lerp(data.mem.evolving_pos.y, data.mem.evoStartY, k);
        } else {
            cam_2d.size      = data.mem.evoStartSize;
            cam_2d.x         = data.mem.evoStartX;
            cam_2d.y         = data.mem.evoStartY;
            data.mem.evoZoom = 0;
        }

        cam_2d.apply();
    }
}
