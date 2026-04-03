using System.Collections.Generic;
using UnityEngine;

public enum CardType { Buff, Debuff, GodQueen, DemonQueen, Event, Item }
public enum PieceType { Light, ELight, KHeavy, BHeavy, RHeavy, Core }

public class data : MonoBehaviour {
    public static data mem;

    public struct MoveData {
        public GameObject piece;
        public int targetX;
        public int targetY;
        public int score;
    }

    // =========================================================================
    // CHESS PIECE STRUCT
    // =========================================================================

    public struct chess_piece {
        public rect_2d   rect;
        public int       x;
        public int       y;
        public int       player_color;
        public int       score;
        public int       score_to_envo;
        public PieceType unitType;
        public int       piece_type;      // 0=Pawn 1=Rook 2=Knight 3=Bishop 4=Queen 5=King
        public int       evolved;         // 0=normal 1=evolved
        public int       evolved_type;    // 0=Knight 1=Bishop 2=Rook
        public int       selected;
        public int       hovered;
    }

    // =========================================================================
    // MOVE PLATE STRUCT
    // =========================================================================

    public struct move_plate {
        public rect_2d rect;
        public bool    attack;
        public int     mat_x;
        public int     mat_y;
        public int     piece_index;  // index into white_pieces or black_pieces
        public int     piece_color;  // 0=white 1=black
    }

    // =========================================================================
    // BOARD DATA
    // =========================================================================

    public GameObject chesspiece;
    public GameObject movePlatePrefab;  // kept, remove when deps clear

    // old — keep until all deps gone
    public GameObject[,] positions    = new GameObject[8, 8];
    public GameObject[]  playerBlack  = new GameObject[16];
    public GameObject[]  playerWhite  = new GameObject[16];

    // new
    public chess_piece[,] board       = new chess_piece[8, 8];
    public chess_piece[]  white_pieces = new chess_piece[16];
    public chess_piece[]  black_pieces = new chess_piece[16];

    public List<move_plate> move_plate_list = new List<move_plate>(); // switched to struct list

    public bool gameOver = false;
    public int current_player_color = 0;  // 0=white 1=black
	public int selected_a_piece = 0; // use to check whether any piece is selected

    // --- sprites normal ---
    public Sprite wp_pawn,   wp_rook,   wp_knight,   wp_bishop,   wp_queen,   wp_king;
    public Sprite bp_pawn,   bp_rook,   bp_knight,   bp_bishop,   bp_queen,   bp_king;

    // --- sprites evolved ---
    public Sprite wp_e_rook,   wp_e_knight,   wp_e_bishop,   wp_e_queen,   wp_e_king;
    public Sprite wp_e_pawn_rook, wp_e_pawn_knight, wp_e_pawn_bishop;
    public Sprite bp_e_rook,   bp_e_knight,   bp_e_bishop,   bp_e_queen,   bp_e_king;
    public Sprite bp_e_pawn_rook, bp_e_pawn_knight, bp_e_pawn_bishop;

    // --- move plate sprites ---
    public Sprite mp_normal;
    public Sprite mp_attack;  // add this in Inspector for red attack plate sprite

    // --- audio ---
    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip captureSound;
    public AudioClip checkSound;
    public AudioClip startSound;
    public AudioClip endSound;
    public AudioClip timeLess;

    // --- cards ---
    public List<Card> allCards;
    public List<Card> whiteHand = new List<Card>();
    public List<Card> blackHand = new List<Card>();
    public GameObject cardPrefab;

    //----AI data----
    public struct AIMove {
        public int piece_index;
        public int targetX;
        public int targetY;
        public bool isAttack;
    }
    void Awake() { mem = this; }
}