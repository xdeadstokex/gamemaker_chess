using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public void PlaySound(AudioClip clip)
    {
        if (data.mem.audioSource != null && clip != null)
            data.mem.audioSource.PlayOneShot(clip);
    }

    // ================= INIT =================

    public void Start()
    {
        data.mem.playerWhite = new GameObject[]
        {
            Create(PieceKind.Rook,1,0,0),
            Create(PieceKind.Knight,1,1,0),
            Create(PieceKind.Bishop,1,2,0),
            Create(PieceKind.Queen,1,3,0),
            Create(PieceKind.King,1,4,0),
            Create(PieceKind.Bishop,1,5,0),
            Create(PieceKind.Knight,1,6,0),
            Create(PieceKind.Rook,1,7,0),

            Create(PieceKind.Pawn,1,0,1),
            Create(PieceKind.Pawn,1,1,1),
            Create(PieceKind.Pawn,1,2,1),
            Create(PieceKind.Pawn,1,3,1),
            Create(PieceKind.Pawn,1,4,1),
            Create(PieceKind.Pawn,1,5,1),
            Create(PieceKind.Pawn,1,6,1),
            Create(PieceKind.Pawn,1,7,1)
        };

        data.mem.playerBlack = new GameObject[]
        {
            Create(PieceKind.Rook,0,0,7),
            Create(PieceKind.Knight,0,1,7),
            Create(PieceKind.Bishop,0,2,7),
            Create(PieceKind.Queen,0,3,7),
            Create(PieceKind.King,0,4,7),
            Create(PieceKind.Bishop,0,5,7),
            Create(PieceKind.Knight,0,6,7),
            Create(PieceKind.Rook,0,7,7),

            Create(PieceKind.Pawn,0,0,6),
            Create(PieceKind.Pawn,0,1,6),
            Create(PieceKind.Pawn,0,2,6),
            Create(PieceKind.Pawn,0,3,6),
            Create(PieceKind.Pawn,0,4,6),
            Create(PieceKind.Pawn,0,5,6),
            Create(PieceKind.Pawn,0,6,6),
            Create(PieceKind.Pawn,0,7,6)
        };

        for (int i = 0; i < 16; i++)
        {
            SetPosition(data.mem.playerWhite[i]);
            SetPosition(data.mem.playerBlack[i]);
        }

        PlaySound(data.mem.startSound);
    }

    // ================= CREATE =================

    public GameObject Create(PieceKind kind, int player, int x, int y)
    {
        GameObject obj = Instantiate(data.mem.chesspiece, new Vector3(0,0,-1), Quaternion.identity);

        Chessman cm = obj.GetComponent<Chessman>();

        cm.piece = new PieceData
        {
            kind = kind,
            player = player,
            evolved = false,
            weapon = -1
        };

        cm.xBoard = x;
        cm.yBoard = y;

        cm.Activate();

        return obj;
    }


    // ================= BOARD =================

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        data.mem.positions[cm.xBoard, cm.yBoard] = obj;
    }

    public bool PositionOnBoard(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= 8 || y >= 8);
    }

    // ================= GAME LOGIC =================

    public Chessman GetKing(int player)
    {
        GameObject[] pieces = player == 1 ? data.mem.playerWhite : data.mem.playerBlack;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] != null)
            {
                Chessman cm = pieces[i].GetComponent<Chessman>();
                if (cm.piece.kind == PieceKind.King)
                    return cm;
            }
        }

        return null;
    }

    public bool IsKingInCheck(int player)
    {
        Chessman king = GetKing(player);
        if (king == null) return false;

        GameObject[] enemies = player == 1 ? data.mem.playerBlack : data.mem.playerWhite;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;

            if (enemies[i].GetComponent<Chessman>().CanMoveTo(king.xBoard, king.yBoard))
                return true;
        }

        return false;
    }

    public void NextTurn()
    {
        data.mem.currentPlayer = data.mem.currentPlayer == "white" ? "black" : "white";

        int p = data.mem.currentPlayer == "white" ? 1 : 0;

        if (!data.mem.gameOver && IsKingInCheck(p))
        {
            PlaySound(data.mem.checkSound);
            Debug.Log(data.mem.currentPlayer + " CHECK");
        }
    }

    // ================= UPDATE =================

    public void Update()
    {
        if (data.mem.gameOver && Input.GetMouseButtonDown(0))
        {
            data.mem.gameOver = false;
            SceneManager.LoadScene("Game");
        }
    }

    public void Winner(string playerWinner)
    {
        data.mem.gameOver = true;

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " wins";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
}