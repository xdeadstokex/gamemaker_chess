using System.Collections;
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

    public GameObject chesspiece;       // chessman prefab
    public GameObject movePlatePrefab;  // moveplate prefab — moved here from Chessman

    public GameObject[,] positions = new GameObject[8, 8];
    public GameObject[] playerBlack = new GameObject[16];
    public GameObject[] playerWhite = new GameObject[16];
    public List<GameObject> move_plate_list = new List<GameObject>();

    public bool gameOver = false;
    public int current_player_color = 0;  // 0 = white, 1 = black

    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip captureSound;
    public AudioClip checkSound;
    public AudioClip startSound;
    public AudioClip endSound;
    public AudioClip timeLess;

    public List<Card> allCards;
    public List<Card> whiteHand = new List<Card>();
    public List<Card> blackHand = new List<Card>();
    public GameObject cardPrefab;

    void Awake() { mem = this; }
}