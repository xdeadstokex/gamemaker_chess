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
        ShowMainMenu();
    }

	public void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
        card_util.add_card(0, CardType.Water); 
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            card_util.add_card(1, CardType.DemonQueen); 
        }
        if (data.mem.gameOver && Input.GetMouseButtonDown(0)) {
            data.mem.gameOver = false;
            SceneManager.LoadScene("Game");
            return;
        }
		if(data.mem.game_started != 0){
		
		// Debug.Log($"mouse {mouse_util.scroll}\n");
		//cam_2d.zoom_to_point(mouse_util.scroll, mouse_util.dx, mouse_util.dy);
		cam_2d.zoom(mouse_util.scroll);
		// --- DRAG ---
		if(mouse_util.left.hold == 1){
			cam_2d.move(-mouse_util.dx * 0.1f, -mouse_util.dy * 0.1f);
		}

		// --- NORMAL ZOOM ---
		cam_2d.zoom(mouse_util.scroll);
		// Debug.Log($"mouse {mouse_util.x} {mouse_util.y}\n");
		}

		if(data.mem.zoom_cross_board == 0){
		if(data.mem.total_players > 2 || data.mem.bot_count >= 2){
		data.mem.zoom_cross_board = 1;
		cam_2d.set_pos(10.5f, 10.5f);
		cam_2d.scale(3.2f);
		}
		}

		zoom_on_evolving_piece();

		// --- GAME OVER ---
		if (data.mem.gameOver && Input.GetMouseButtonDown(0)) {
			data.mem.gameOver = false;
			SceneManager.LoadScene("Game");
			return;
		}

		HandleMenuInput();

		if (data.mem.game_started == 0) return;

		if(data.mem.current_player_color != 0 && data.mem.play_against_AI == 1){
			if (!data.mem.isAIThinking) {
				StartCoroutine(AI_util.PlayAITurn());
			}
			return;
		}

		HandlePieceInput();
		HandleMovePlateInput();
        int currentColor = data.mem.current_player_color; 

        card_util.handle_card_input(currentColor);
	}

    // =========================================================================
    // MAIN MENU
    // =========================================================================

        void ShowMainMenu() {
        gui_util.clear_menu();

        // Nền menu
        data.mem.main_screen_gui = rect_2d.create(0f, 0f, 1f);
        data.mem.main_screen_gui.set_sprite(data.mem.rect_2d_sprite);
        data.mem.main_screen_gui.set_sprite_size(20f, 12f);

        // Nút PvP
        data.mem.pvp_button = gui_util.make_button(2.5f, 0f, Color.white, "");
        data.mem.pvp_button.set_sprite(data.mem.pvp_btn_sprite); // Dùng sprite thay vì vẽ màu
        data.mem.pvp_button.set_sprite_size(4f, 2f); // Chỉnh lại size cho vừa mắt

        // Nút PvE
        data.mem.pve_button = gui_util.make_button(-2.5f, 0f, Color.white, "");
        data.mem.pve_button.set_sprite(data.mem.pve_btn_sprite);
        data.mem.pve_button.set_sprite_size(4f, 2f);

        data.mem.menu_state = data.MenuState.Main;
    }
    void ShowPlayerCountMenu(bool is_pve) {
        gui_util.clear_menu();

        // 1. Nền Menu / Label Background
        data.mem.main_screen_gui = rect_2d.create(0f, 0f);
        data.mem.main_screen_gui.set_sprite(data.mem.rect_2d_sprite);
        data.mem.main_screen_gui.set_sprite_size(20f, 12f);
        data.mem.main_screen_gui.set_collider_size(20f, 12f);

        if (is_pve) {
            // 2. Chế độ chọn số lượng Bot (1 vs 1, 1 vs 2, 1 vs 3)
            data.mem.btn_count1 = gui_util.make_button(5f, 0f, Color.white, "");
            data.mem.btn_count1.set_sprite(data.mem.count1_btn_sprite); // Gán Sprite thay vì vẽ Color
            data.mem.btn_count1.set_sprite_size(4f, 2f);
            data.mem.btn_count2 = gui_util.make_button(0f, 0f, Color.white, "");
            data.mem.btn_count2.set_sprite(data.mem.count2_btn_sprite);
            data.mem.btn_count2.set_sprite_size(4f, 2f);

            data.mem.btn_count3 = gui_util.make_button(-5f, 0f, Color.white, "");
            data.mem.btn_count3.set_sprite(data.mem.count3_btn_sprite);
            data.mem.btn_count3.set_sprite_size(4f, 2f);

            data.mem.menu_state = data.MenuState.PickBotCount;
        } else {
            // 3. Chế độ chọn số lượng người chơi (2, 3, 4 Players)
            // Bạn có thể dùng chung btn_count hoặc các Sprite riêng cho PvP
            data.mem.btn_count1 = gui_util.make_button(5f, 0f, Color.white, "");
            data.mem.btn_count1.set_sprite(data.mem.count1_btn_sprite);
            data.mem.btn_count1.set_sprite_size(4f, 2f);
            
            data.mem.btn_count2 = gui_util.make_button(0f, 0f, Color.white, "");
            data.mem.btn_count2.set_sprite(data.mem.count2_btn_sprite);
            data.mem.btn_count2.set_sprite_size(4f, 2f);

            
            data.mem.btn_count3 = gui_util.make_button(-5f, 0f, Color.white, "");
            data.mem.btn_count3.set_sprite(data.mem.count3_btn_sprite);
            data.mem.btn_count3.set_sprite_size(4f, 2f);
            
            data.mem.menu_state = data.MenuState.PickPlayerCount;
        }

        // 4. Nút Back (Dùng Sprite hình mũi tên hoặc chữ Back đã vẽ sẵn)
        data.mem.back_button = gui_util.make_button(0f, -3f, Color.white, "");
        data.mem.back_button.set_sprite(data.mem.back_btn_sprite); // Gán Sprite nút Back
        data.mem.back_button.set_sprite_size(2f, 2f); // Điều chỉnh kích thước nút Back cho cân đối
    }

    void ShowDifficultyMenu() {
        gui_util.clear_menu();
        
        // Nền
        data.mem.main_screen_gui = rect_2d.create(0f, 0f);
        data.mem.main_screen_gui.set_sprite(data.mem.rect_2d_sprite);
        data.mem.main_screen_gui.set_sprite_size(20f, 12f);

        // Các nút độ khó dùng Sprite riêng
        data.mem.btn_diff1 = gui_util.make_button(4.5f, 0f, Color.white, "");
        data.mem.btn_diff1.set_sprite(data.mem.diff1_btn_sprite);
        data.mem.btn_diff1.set_sprite_size(4f, 2f);
        
        data.mem.btn_diff2 = gui_util.make_button(1.5f, 0f, Color.white, "");
        data.mem.btn_diff2.set_sprite(data.mem.diff2_btn_sprite);
        data.mem.btn_diff2.set_sprite_size(4f, 2f);

        data.mem.btn_diff3 = gui_util.make_button(-1.5f, 0f, Color.white, "");
        data.mem.btn_diff3.set_sprite(data.mem.diff3_btn_sprite);
        data.mem.btn_diff3.set_sprite_size(4f, 2f);

        data.mem.btn_diff4 = gui_util.make_button(-4.5f, 0f, Color.white, "");
        data.mem.btn_diff4.set_sprite(data.mem.diff4_btn_sprite);
        data.mem.btn_diff4.set_sprite_size(4f, 2f);

        // Nút Back
        data.mem.back_button = gui_util.make_button(0f, -3f, Color.white, "");
        data.mem.back_button.set_sprite(data.mem.back_btn_sprite);
        data.mem.back_button.set_sprite_size(2f, 2f);

        data.mem.menu_state = data.MenuState.PickBotDifficulty;
    }
    // =========================================================================
    // MENU INPUT
    // =========================================================================

    void HandleMenuInput() {
        switch (data.mem.menu_state) {

            case data.MenuState.Main:
                if (gui_util.clicked(data.mem.pvp_button)) {
                    ShowPlayerCountMenu(false);
                }
                if (gui_util.clicked(data.mem.pve_button)) {
                    ShowPlayerCountMenu(true);
                }
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
                if (gui_util.clicked(data.mem.btn_diff1)) { data.mem.ai_difficulty = AIDifficulty.Baby; StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff2)) { data.mem.ai_difficulty = AIDifficulty.Easy; StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff3)) { data.mem.ai_difficulty = AIDifficulty.Normal; StartGame(data.mem.total_players, data.mem.bot_count); }
                if (gui_util.clicked(data.mem.btn_diff4)) { data.mem.ai_difficulty = AIDifficulty.Asean; StartGame(data.mem.total_players, data.mem.bot_count); }
                
                if (gui_util.clicked(data.mem.back_button)) ShowPlayerCountMenu(true);
                break;
        }
    }

    // =========================================================================
    // GAME START
    // =========================================================================

    void StartGame(int total_players, int bot_count) {
        gui_util.clear_menu();

        data.mem.total_players     = total_players;
        data.mem.bot_count         = bot_count;
        data.mem.current_player_color = 0;
        data.mem.game_started      = 1;
        data.mem.menu_state        = data.MenuState.None;

        data.mem.play_against_AI   = (bot_count > 0) ? 1 : 0;

        // init armies for each player
        data.mem.armies = new data.army_data[total_players];
        for (int i = 0; i < total_players; i++)
            data.mem.armies[i] = new data.army_data(i);

        // legacy shortcuts still work
        data.mem.white_army = data.mem.armies[0];
        data.mem.black_army = data.mem.armies[1];

        if (total_players == 2) {
            InitBoard_2P();
            PlacePieces_2P();
        } else {
            InitBoard_Cross(total_players);
            PlacePieces_Cross(total_players);
        }

        sound_util.play_sound(data.mem.startSound);
        card_util.init_card_table();
        card_util.refresh_card_visuals(0);
    }

    // =========================================================================
    // 2P BOARD  (standard 8×8)
    // =========================================================================

    void InitBoard_2P() {
        board_util.InitFlat(8, 8);

        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++) {
            ref data.board_cell c = ref board_util.Cell(x, y);
            bool light = (x + y) % 2 == 0;
            c.tile_sprite = light ? data.mem.board_tile0 : data.mem.board_tile1;
            c.tile = rect_2d.create(board_util.board_to_world(x), board_util.board_to_world(y), 0f);
            c.tile.set_sprite(c.tile_sprite);
            c.tile.set_sprite_scale(1f, 1f);
            c.tile.col.enabled = false;
        }
    }

    void PlacePieces_2P() {
        data.army_data w = data.mem.armies[0];
        data.army_data b = data.mem.armies[1];

        // white — bottom (y=0,1)
        PlaceBackRow(0, 0, w);
        for (int x = 0; x < 8; x++) piece_util.create_piece(x, 1, 0, w);

        // black — top (y=7,6)
        PlaceBackRowFlipped(0, 7, b);
        for (int x = 0; x < 8; x++) piece_util.create_piece(x, 6, 0, b);
    }

    // =========================================================================
    // CROSS BOARD  (24×24, corners punched out)
    //
    //  The 24×24 grid is divided into nine 8×8 blocks:
    //
    //    [0,0..7]   [1,8..15]  [2,16..23]   (block col index)
    //    [0,0..7]   [1,8..15]  [2,16..23]   (block row index)
    //
    //  Cross keeps:  top-arm (block col 1, rows 0..7)
    //                left-arm  (block row 1, cols 0..7)
    //                center    (block col 1, row 1  — cols 8..15, rows 8..15)
    //                right-arm (block row 1, cols 16..23)
    //                bottom-arm(block col 1, rows 16..23)
    //
    //  Holes (punched): 4 corner 8×8 blocks.
    //
    //  3P: top arm is left empty (no tiles, no pieces).
    //  4P: all 5 arms used.
    //
    //  Player starting positions (back row closest to arm edge):
    //    P0 = bottom arm, faces up   (rows 16..23, back=row 23)
    //    P1 = top arm,    faces down (rows  0.. 7, back=row  0)  — 4P only
    //    P2 = left arm,   faces right(cols  0.. 7, back=col  0)
    //    P3 = right arm,  faces left (cols 16..23, back=col 23)
    // =========================================================================

    const int CROSS_SIZE  = 24;
    const int ARM         = 8;   // arm width == block size

    static bool IsCrossCell(int x, int y) {
        bool in_col_mid = x >= ARM && x < ARM * 2;
        bool in_row_mid = y >= ARM && y < ARM * 2;
        return in_col_mid || in_row_mid;
    }

    void InitBoard_Cross(int total_players) {
        board_util.InitFlat(CROSS_SIZE, CROSS_SIZE);

        for (int y = 0; y < CROSS_SIZE; y++)
        for (int x = 0; x < CROSS_SIZE; x++) {
            ref data.board_cell c = ref board_util.Cell(x, y);

            bool keep = IsCrossCell(x, y);

            // 3P: disable top arm (x in mid col, y in top block)
            if (total_players == 3 && x >= ARM && x < ARM*2 && y < ARM)
                keep = false;

            if (!keep) {
                c.valid = 0;
                continue;
            }

            c.valid = 1;
            bool light = (x + y) % 2 == 0;

            // color tiles per arm for clarity
            Sprite s0, s1;
            if      (y >= ARM*2)           { s0 = data.mem.board_tile0; s1 = data.mem.board_tile1; } // bottom
            else if (y < ARM)              { s0 = data.mem.board_tile2; s1 = data.mem.board_tile3; } // top
            else if (x < ARM)             { s0 = data.mem.board_tile0; s1 = data.mem.board_tile2; } // left
            else if (x >= ARM*2)          { s0 = data.mem.board_tile1; s1 = data.mem.board_tile3; } // right
            else                           { s0 = data.mem.board_tile0; s1 = data.mem.board_tile1; } // center

            c.tile_sprite = light ? s0 : s1;
            c.tile = rect_2d.create(board_util.board_to_world(x), board_util.board_to_world(y), 0f);
            c.tile.set_sprite(c.tile_sprite);
            c.tile.set_sprite_scale(1f, 1f);
            c.tile.col.enabled = false;
        }
    }

    void PlacePieces_Cross(int total_players) {
        // P0 — bottom arm, faces up
        PlacePieces_Vertical(data.mem.armies[0], ARM, ARM*2,   ARM*3-1, ARM*3-2, +1);

        // P1 — top arm, faces down (4P only)
        if (total_players >= 4)
            PlacePieces_Vertical(data.mem.armies[1], ARM, ARM*2,   0,       1,       -1);

        // P2 — left arm, faces right
        PlacePieces_Horizontal(data.mem.armies[2], ARM, ARM*2,   0,       1,        +1);

        // P3 — right arm, faces left (4P only)
        if (total_players >= 4)
            PlacePieces_Horizontal(data.mem.armies[3], ARM, ARM*2,   ARM*3-1, ARM*3-2, -1);

        // 3P: P1 takes left arm, P2 takes right arm (re-assign)
        if (total_players == 3) {
            // re-init armies index mapping: 0=bottom, 1=left, 2=right
            data.mem.armies[1] = new data.army_data(1);
            data.mem.armies[2] = new data.army_data(2);
            PlacePieces_Horizontal(data.mem.armies[1], ARM, ARM*2,   0,       1,        +1);
            PlacePieces_Horizontal(data.mem.armies[2], ARM, ARM*2,   ARM*3-1, ARM*3-2, -1);
        }
    }

    // Place a standard back row + pawns along a vertical arm.
    // col_from..col_to  = x range of the arm (ARM wide)
    // back_row          = y of the back row
    // pawn_row          = y of the pawn row
    // pawn_dir          = +1 (pawns face up) or -1 (pawns face down) — stored but unused until move logic
    void PlacePieces_Vertical(data.army_data army, int col_from, int col_to, int back_row, int pawn_row, int pawn_dir) {
        // back row: rook knight bishop queen king bishop knight rook
        int[] back = { 1, 2, 3, 5, 4, 3, 2, 1 };
        for (int i = 0; i < ARM; i++)
            piece_util.create_piece(col_from + i, back_row, back[i], army);
        for (int i = 0; i < ARM; i++)
            piece_util.create_piece(col_from + i, pawn_row, 0, army);
    }

    // Place a standard back row + pawns along a horizontal arm.
    // row_from..row_to  = y range of the arm
    // back_col          = x of the back col
    // pawn_col          = x of the pawn col
    // pawn_dir          = +1 (pawns face right) or -1 (pawns face left)
    void PlacePieces_Horizontal(data.army_data army, int row_from, int row_to, int back_col, int pawn_col, int pawn_dir) {
        int[] back = { 1, 2, 3, 5, 4, 3, 2, 1 };
        for (int i = 0; i < ARM; i++)
            piece_util.create_piece(back_col, row_from + i, back[i], army);
        for (int i = 0; i < ARM; i++)
            piece_util.create_piece(pawn_col, row_from + i, 0, army);
    }

    // =========================================================================
    // LEGACY HELPERS  (kept for piece_util compatibility)
    // =========================================================================

    void PlaceBackRow(int x_start, int y, data.army_data army){
        int[] t = { 1, 2, 3, 4, 5, 3, 2, 1 };
        for (int i = 0; i < 8; i++)
            piece_util.create_piece(x_start + i, y, t[i], army);
    }

    void PlaceBackRowFlipped(int x_start, int y, data.army_data army){
        int[] t = { 1, 2, 3, 5, 4, 3, 2, 1 };
        for (int i = 0; i < 8; i++)
            piece_util.create_piece(x_start + i, y, t[i], army);
    }

    // =========================================================================
    // PIECE INPUT  (unchanged logic, extended to N armies)
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

        // scale pass
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
    // MOVEPLATE INPUT  (unchanged)
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

                data.army_data army = data.mem.armies[mp.piece_color];
                ref data.chess_piece attacker = ref army.troop_list[mp.piece_index];

                if (!mp.attack) {
                    sound_util.play_sound(data.mem.moveSound);
                    piece_util.move_piece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);
                } else {
                    piece_util.piece_attack(ref attacker, mp.mat_x, mp.mat_y, mp.rect.obj.transform.position);
                    if (attacker.piece_type != 7)
                        piece_util.move_piece(ref attacker, mp.piece_index, mp.piece_color, mp.mat_x, mp.mat_y);
                }

                data.mem.selected_a_piece = 0;
                piece_util.unselect_all_piece();
                pvp_util.next_player_turn();
                move_plate_util.clear_move_plate();
                return;
            }
        }
    }

    // =========================================================================
    // ZOOM ON EVO A PIECE
    // =========================================================================

	void zoom_on_evolving_piece(){

		// --- TRIGGER ---
		if (data.mem.evolving_signal == 1){
			data.mem.evolving_signal = 0;

			data.mem.evoStartX = cam_2d.x;
			data.mem.evoStartY = cam_2d.y;
			data.mem.evoStartSize = cam_2d.size;

			cam_2d.set_pos(data.mem.evolving_pos.x, data.mem.evolving_pos.y);

			data.mem.evoTargetSize = cam_2d.size * 0.7f;

			data.mem.evoTimer = 0f;
			data.mem.evoZoom = 1;
		}

		// --- UPDATE EFFECT ---
		if (data.mem.evoZoom == 1){
			data.mem.evoTimer += Time.deltaTime;

			float t = data.mem.evoTimer / 0.5f;

			if (t <= 1f){
				cam_2d.size = Mathf.Lerp(data.mem.evoStartSize, data.mem.evoTargetSize, t);
			}
			else if (t <= 2f){
				float k = t - 1f;

				cam_2d.size = Mathf.Lerp(data.mem.evoTargetSize, data.mem.evoStartSize, k);

				cam_2d.x = Mathf.Lerp(data.mem.evolving_pos.x, data.mem.evoStartX, k);
				cam_2d.y = Mathf.Lerp(data.mem.evolving_pos.y, data.mem.evoStartY, k);
			}
			else{
				cam_2d.size = data.mem.evoStartSize;
				cam_2d.x = data.mem.evoStartX;
				cam_2d.y = data.mem.evoStartY;

				data.mem.evoZoom = 0;
			}

			cam_2d.apply();
		}
	}
}