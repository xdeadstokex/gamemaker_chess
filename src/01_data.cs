using System.Collections.Generic;
using UnityEngine;

public enum CardType { Buff, Debuff, GodQueen, DemonQueen, Event, Item }

public class data : MonoBehaviour {
    public static data mem;

    public struct MoveData {
        public GameObject piece;
        public int targetX;
        public int targetY;
        public int score;
    }

    public GameObject chesspiece;
    public GameObject movePlatePrefab;

    public GameObject[,] positions = new GameObject[8, 8];
    public GameObject[] playerBlack = new GameObject[16];
    public GameObject[] playerWhite = new GameObject[16];
    public List<GameObject> move_plate_list = new List<GameObject>();

    public bool gameOver = false;
    public int current_player_color = 0;  // 0=white 1=black

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

    void Awake() { mem = this; }
}