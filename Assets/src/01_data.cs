using System.Collections.Generic;
using UnityEngine;

public enum CardType { Buff, Debuff, GodQueen, DemonQueen, Event, Item }
public enum PieceType { Light, ELight, KHeavy, BHeavy, RHeavy, Core }
public class data : MonoBehaviour {
    public static data mem;
    // =========================================================================
    // BOARD CELL
    // =========================================================================

    public struct board_cell {
        public int     valid;        // 0 = hole, 1 = usable
        public int     has_piece;    // 0 = empty, 1 = occupied
        public int     piece_color;
        public int     piece_index;
        public rect_2d tile;         // visual tile for this cell
        public Sprite  tile_sprite;  // ref to light or dark tile sprite
    }

    // =========================================================================
    // CHESS PIECE
    // =========================================================================

    public struct chess_piece {
        public rect_2d   rect;

        // flat sprite refs — point at global pool, no copy
        public Sprite    normal_sprite;
        public Sprite    evo_sprite0;
        public Sprite    evo_sprite1;
        public Sprite    evo_sprite2;

        public int       x;
        public int       y;
        public int       player_color;
        public int       score;
        public int       score_to_envo;
        public PieceType unitType;
        public int       piece_type;    // 0=Pawn 1=Rook 2=Knight 3=Bishop 4=Queen 5=King 6 = DQueen, 7 = King with gun
        public int       evolved;       // 0=normal 1=evolved
        public int       evolved_type;  // 0=Knight 1=Bishop 2=Rook , pawn only //why not enum? because we want to use the int value for sprite selection _nmt05
        public int       selected;
        public int       hovered;
        public float     hover_sprite_scale;
        public float     normal_sprite_scale;
        public int       shield;          // for dqueen
    }

    // =========================================================================
    // MOVE PLATE
    // =========================================================================

    public struct move_plate {
        public rect_2d rect;
        public bool    attack;
        public int     mat_x;
        public int     mat_y;
        public int     piece_index;
        public int     piece_color;
        public float   hover_sprite_scale;
        public float   normal_sprite_scale;
    }

    // =========================================================================
    // ARMY DATA — pure data
    // =========================================================================

    public class army_data {
        public int           color;
        public chess_piece[] troop_list  = new chess_piece[16];
        public int           troop_count = 0;

        public army_data(int color) { this.color = color; }
    }
    // =========================================================================
    // ARMY DATA — card
    // =========================================================================
    public class Card {
        public string cardName;
        public CardType type;
        public int value; // for buff/debuff cards, the amount to add/subtract
        public Sprite artwork;
        public string description;
    
    }

    // =========================================================================
    // BOARD
    // =========================================================================

    public int          board_w;
    public int          board_h;
    public board_cell[] board;   // flat, access: board[x + y * board_w]

    public army_data white_army;
    public army_data black_army;

    public army_data get_army(int color)  => color == 0 ? white_army : black_army;
    public army_data get_enemy(int color) => color == 0 ? black_army : white_army;

    // =========================================================================
    // GAME STATE
    // =========================================================================

    public List<move_plate> move_plate_list     = new List<move_plate>();
    public bool             gameOver            = false;
    public int              current_player_color = 0;
    public int              selected_a_piece    = 0;

    // =========================================================================
    // AI DATA
    // =========================================================================
    //random
    public struct AIMove {
        public int piece_index;
        public int targetX;
        public int targetY;
        public bool isAttack;
    }
    public bool isVsAI = true;
    public int aiColor = 1;
    public bool isAIThinking = false;

    //monte
    public class MCTSNode {
        public AIMove move;
        public MCTSNode parent;
        public List<MCTSNode> children = new List<MCTSNode>();
        public float wins = 0;
        public int visits = 0;
        public int colorToMove; 
        public List<AIMove> untriedMoves;
    }

    // --- MCTS Backup Variables ---
    public board_cell[] real_board;
    public army_data real_white;
    public army_data real_black;

    // =========================================================================
    // GLOBAL SPRITE POOL — Inspector entry point
    // =========================================================================

    // board tiles — assign your RPG tile sprites here
    public Sprite board_tile0;   // light square
    public Sprite board_tile1;    // dark square
    public Sprite board_tile2;   // light square
    public Sprite board_tile3;    // dark square
    // normal pieces
    public Sprite wp_pawn,   wp_rook,   wp_knight,   wp_bishop,   wp_queen,   wp_king;
    public Sprite bp_pawn,   bp_rook,   bp_knight,   bp_bishop,   bp_queen,   bp_king;

    // evolved pieces
    public Sprite wp_e_rook,   wp_e_knight,   wp_e_bishop,   wp_e_queen,   wp_e_king, wp_e_dqueen;
    public Sprite bp_e_rook,   bp_e_knight,   bp_e_bishop,   bp_e_queen,   bp_e_king, bp_e_dqueen;

    // evolved pawn variants
    public Sprite wp_e_pawn_knight, wp_e_pawn_bishop, wp_e_pawn_rook;
    public Sprite bp_e_pawn_knight, bp_e_pawn_bishop, bp_e_pawn_rook;

    // move plates
    public Sprite mp_normal;
    public Sprite mp_attack;

    // =========================================================================
    // AUDIO
    // =========================================================================

    public AudioSource audioSource;
    public AudioClip   moveSound;
    public AudioClip   captureSound;
    public AudioClip   checkSound;
    public AudioClip   startSound;
    public AudioClip   endSound;
    public AudioClip   timeLess;

    // =========================================================================
    // CARDS
    // =========================================================================

    public List<Card> allCards;
    public List<Card> whiteHand = new List<Card>();
    public List<Card> blackHand = new List<Card>();
    public GameObject cardPrefab;

    // =========================================================================
    // INIT
    // =========================================================================

    void Awake() {
        mem        = this;
        white_army = new army_data(0);
        black_army = new army_data(1);
    }
}