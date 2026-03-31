using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class data : MonoBehaviour {
    public static data mem;
	
	public struct MoveData {
    public GameObject piece; // Quân cờ sẽ đi
    public int targetX;      // Tọa độ X đến
    public int targetY;      // Tọa độ Y đến
    public int score;        // Điểm số của nước đi này
	}
	
    public GameObject chesspiece; // reserve a space for later assign of unity chesspiece obj/prefab

    //Matrices needed, positions of each of the GameObjects
    //Also separate arrays for the players in order to easily keep track of them all
    //Keep in mind that the same objects are going to be in "positions" and "playerBlack"/"playerWhite"
    public GameObject[,] positions = new GameObject[8, 8];
    public GameObject[] playerBlack = new GameObject[16];
    public GameObject[] playerWhite = new GameObject[16];

    public bool gameOver = false;
	public string currentPlayer = "white";	//current turn

    public AudioSource audioSource;
    public AudioClip moveSound;   // Âm thanh đi quân bình thường
    public AudioClip captureSound; // Âm thanh khi ăn quân
    public AudioClip checkSound;
    // public AudioClip winSound;
    public AudioClip startSound;
    public AudioClip endSound; // not yet
    public AudioClip timeLess; // not yet
    void Awake() {
        mem = this;
    }
}