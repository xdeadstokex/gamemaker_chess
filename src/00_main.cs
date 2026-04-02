using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    GameObject controller;
    data dataScript;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    public void Awake() {
        controller = GameObject.FindGameObjectWithTag("GameController");
        dataScript = controller.GetComponent<data>();
    }

    public void Start() {
        data.mem.playerWhite = new GameObject[] {
            Create(0, 0, 1, 0), // Rook
            Create(1, 0, 2, 0), // Knight
            Create(2, 0, 3, 0), // Bishop
            Create(3, 0, 4, 0), // Queen
            Create(4, 0, 5, 0), // King
            Create(5, 0, 3, 0), // Bishop
            Create(6, 0, 2, 0), // Knight
            Create(7, 0, 1, 0), // Rook
            Create(0, 1, 0, 0), Create(1, 1, 0, 0), Create(2, 1, 0, 0), Create(3, 1, 0, 0),
            Create(4, 1, 0, 0), Create(5, 1, 0, 0), Create(6, 1, 0, 0), Create(7, 1, 0, 0)
        };

        data.mem.playerBlack = new GameObject[] {
            Create(0, 7, 1, 1), // Rook
            Create(1, 7, 2, 1), // Knight
            Create(2, 7, 3, 1), // Bishop
            Create(3, 7, 4, 1), // Queen
            Create(4, 7, 5, 1), // King
            Create(5, 7, 3, 1), // Bishop
            Create(6, 7, 2, 1), // Knight
            Create(7, 7, 1, 1), // Rook
            Create(0, 6, 0, 1), Create(1, 6, 0, 1), Create(2, 6, 0, 1), Create(3, 6, 0, 1),
            Create(4, 6, 0, 1), Create(5, 6, 0, 1), Create(6, 6, 0, 1), Create(7, 6, 0, 1)
        };

        for (int i = 0; i < data.mem.playerBlack.Length; i++) {
            SetPosition(data.mem.playerBlack[i]);
            SetPosition(data.mem.playerWhite[i]);
        }

        PlaySound(dataScript.startSound);
    }

    public void Update() {
        if (data.mem.gameOver && Input.GetMouseButtonDown(0)) {
            data.mem.gameOver = false;
            SceneManager.LoadScene("Game");
            return;
        }
        HandleChessmanInput();
        HandleMovePlateInput();
    }

    // =========================================================================
    // CHESSMAN INPUT
    // =========================================================================

    void HandleChessmanInput() {
        GameObject[][] groups = { data.mem.playerWhite, data.mem.playerBlack };
        Chessman hovered_piece = null;

        foreach (var pieces in groups) {
            foreach (GameObject obj in pieces) {
                if (obj == null) continue;
                Chessman cm = obj.GetComponent<Chessman>();
                if (cm == null) continue;

                bool isCurrent = cm.player_color == data.mem.current_player_color;

                // reset hovered state when mouse leaves
                if (cm.mouse_hover == 0 && cm.hovered == 1) {
                    cm.hovered = 0;
                }

                if (cm.mouse_hover == 1 && cm.selected == 0 && isCurrent) {
                    hovered_piece = cm;
                }
            }
        }

        // spawn plates for newly hovered piece
        if (hovered_piece != null && hovered_piece.hovered == 0) {
            hovered_piece.hovered = 1;
            ClearMovePlates();
            SpawnMovePlates(hovered_piece);
        }
    }

    // =========================================================================
    // MOVEPLATE INPUT
    // =========================================================================

    void HandleMovePlateInput() {
        foreach (GameObject mpObj in data.mem.move_plate_list) {
            if (mpObj == null) continue;
            MovePlate mp = mpObj.GetComponent<MovePlate>();
            if (mp == null) continue;

            // hover glow
            mp.ApplyHoverVisual(mp.mouse_hover == 1);

            // confirmed click
            if (mp.mouse_unclick == 1) {
                mp.mouse_unclick = 0;

                if (mp.GetReference() == null) continue;
                Chessman attacker = mp.GetReference().GetComponent<Chessman>();
                if (attacker == null) continue;

                if (mp.attack) {
                    HandleAttack(attacker, mp.GetMatrixX(), mp.GetMatrixY(), mpObj.transform.position);
                } else {
                    PlaySound(dataScript.moveSound);
                }

                MovePiece(attacker, mp.GetMatrixX(), mp.GetMatrixY());
                NextTurn();
                ClearMovePlates();
                return; // one move per frame
            }
        }
    }

    // =========================================================================
    // MOVEPLATE SPAWN — all plate logic lives here now
    // =========================================================================

    public void SpawnMovePlates(Chessman cm) {
        int pawnDir = (cm.player_color == 0) ? 1 : -1;

        switch (cm.piece_type) {
            case 0: PawnPlates(cm, cm.xBoard, cm.yBoard + pawnDir); break;
            case 1: RookPlates(cm);   break;
            case 2: KnightPlates(cm); break;
            case 3: BishopPlates(cm); break;
            case 4: QueenPlates(cm);  break;
            case 5: KingPlates(cm);   break;
        }

        if (cm.evolved == 0) return;

        switch (cm.piece_type) {
            case 0:
                if      (cm.evolved_type == 2) RookPlates(cm);
                else if (cm.evolved_type == 0) KnightPlates(cm);
                else if (cm.evolved_type == 1) BishopPlates(cm);
                break;
            case 2: EvolvedKnightAddon(cm); break;
            case 3: KingPlates(cm);         break; // evolved bishop gets king moves
            case 5: QueenPlates(cm);        break; // evolved king gets queen moves
        }
    }

    void QueenPlates(Chessman cm)  { RookPlates(cm); BishopPlates(cm); }
    void RookPlates(Chessman cm)   { LinePlates(cm, 1,0); LinePlates(cm,-1,0); LinePlates(cm,0,1); LinePlates(cm,0,-1); }
    void BishopPlates(Chessman cm) { LinePlates(cm, 1,1); LinePlates(cm,1,-1); LinePlates(cm,-1,1); LinePlates(cm,-1,-1); }

    void KnightPlates(Chessman cm) {
        PointPlate(cm, cm.xBoard+1, cm.yBoard+2); PointPlate(cm, cm.xBoard-1, cm.yBoard+2);
        PointPlate(cm, cm.xBoard+2, cm.yBoard+1); PointPlate(cm, cm.xBoard+2, cm.yBoard-1);
        PointPlate(cm, cm.xBoard+1, cm.yBoard-2); PointPlate(cm, cm.xBoard-1, cm.yBoard-2);
        PointPlate(cm, cm.xBoard-2, cm.yBoard+1); PointPlate(cm, cm.xBoard-2, cm.yBoard-1);
    }

    void EvolvedKnightAddon(Chessman cm) {
        PointPlate(cm, cm.xBoard-2, cm.yBoard); PointPlate(cm, cm.xBoard+2, cm.yBoard);
        PointPlate(cm, cm.xBoard, cm.yBoard+2); PointPlate(cm, cm.xBoard, cm.yBoard-2);
    }

    void KingPlates(Chessman cm) {
        PointPlate(cm, cm.xBoard,   cm.yBoard+1); PointPlate(cm, cm.xBoard,   cm.yBoard-1);
        PointPlate(cm, cm.xBoard-1, cm.yBoard);   PointPlate(cm, cm.xBoard+1, cm.yBoard);
        PointPlate(cm, cm.xBoard-1, cm.yBoard-1); PointPlate(cm, cm.xBoard-1, cm.yBoard+1);
        PointPlate(cm, cm.xBoard+1, cm.yBoard-1); PointPlate(cm, cm.xBoard+1, cm.yBoard+1);
    }

    void LinePlates(Chessman cm, int dx, int dy) {
        int x = cm.xBoard + dx, y = cm.yBoard + dy;
        while (PositionOnBoard(x, y) && data.mem.positions[x, y] == null) {
            SpawnPlate(cm, x, y, false);
            x += dx; y += dy;
        }
        if (PositionOnBoard(x, y) && data.mem.positions[x, y].GetComponent<Chessman>().player_color != cm.player_color) {
            SpawnPlate(cm, x, y, true);
        }
    }

    void PointPlate(Chessman cm, int x, int y) {
        if (!PositionOnBoard(x, y)) return;
        GameObject cp = data.mem.positions[x, y];
        if (cp == null) {
            SpawnPlate(cm, x, y, false);
        } else {
            if (cp.GetComponent<Chessman>().player_color != cm.player_color) SpawnPlate(cm, x, y, true);
        }
    }

    void PawnPlates(Chessman cm, int x, int y) {
        int dir = (cm.player_color == 0) ? 1 : -1;

        if (PositionOnBoard(x, y) && data.mem.positions[x, y] == null) {
            SpawnPlate(cm, x, y, false);
            int startRow = (cm.player_color == 0) ? 1 : 6;
            if (cm.yBoard == startRow && data.mem.positions[x, y + dir] == null) SpawnPlate(cm, x, y + dir, false);
        }

        // diagonal captures
        DiagCapturePlate(cm, x + 1, y);
        DiagCapturePlate(cm, x - 1, y);
    }

    void DiagCapturePlate(Chessman cm, int x, int y) {
        if (!PositionOnBoard(x, y)) return;
        GameObject cp = data.mem.positions[x, y];
        if (cp != null && cp.GetComponent<Chessman>().player_color != cm.player_color) SpawnPlate(cm, x, y, true);
    }

    void SpawnPlate(Chessman cm, int mx, int my, bool isAttack) {
        float wx = cm.BoardToWorld(mx), wy = cm.BoardToWorld(my);
        GameObject mpObj = Instantiate(data.mem.movePlatePrefab, new Vector3(wx, wy, -3f), Quaternion.identity);

        MovePlate mp = mpObj.GetComponent<MovePlate>();
        mp.attack = isAttack;
        mp.SetReference(cm.gameObject);
        mp.SetCoords(mx, my);

        data.mem.move_plate_list.Add(mpObj);
    }

    public void ClearMovePlates() {
        foreach (GameObject mp in data.mem.move_plate_list) {
            if (mp != null) Destroy(mp);
        }
        data.mem.move_plate_list.Clear();
    }

    // =========================================================================
    // ATTACK / MOVE
    // =========================================================================

    void HandleAttack(Chessman attacker, int tx, int ty, Vector3 pos) {
        GameObject targetObj = data.mem.positions[tx, ty];
        if (targetObj == null) return;

        Chessman target = targetObj.GetComponent<Chessman>();
        if (target == null) return;

        attacker.AbsorbPoints(targetObj, target.GetScore(), pos);
        PlaySound(dataScript.captureSound);

        if (target.piece_type == 5) {
            string winner = (target.player_color == 0) ? "black" : "white";
            Winner(winner);
        }

        Destroy(targetObj);
    }

    void MovePiece(Chessman cm, int tx, int ty) {
        data.mem.positions[cm.xBoard, cm.yBoard] = null;
        cm.xBoard = tx;
        cm.yBoard = ty;
        cm.SetCoords();
        SetPosition(cm.gameObject);
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

    public bool IsKingInCheck(int kingColor) {
        Chessman king = GetKing(kingColor);
        if (king == null) return false;

        GameObject[] enemies = (kingColor == 0) ? data.mem.playerBlack : data.mem.playerWhite;
        foreach (GameObject enemyObj in enemies) {
            if (enemyObj == null) continue;
            Chessman cm = enemyObj.GetComponent<Chessman>();
            if (cm != null && cm.CanMoveTo(king.xBoard, king.yBoard)) return true;
        }
        return false;
    }

    public Chessman GetKing(int color) {
        GameObject[] pieces = (color == 0) ? data.mem.playerWhite : data.mem.playerBlack;
        foreach (GameObject obj in pieces) {
            if (obj == null) continue;
            Chessman cm = obj.GetComponent<Chessman>();
            if (cm != null && cm.piece_type == 5) return cm;
        }
        Debug.LogError("King not found for color: " + color);
        return null;
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    public GameObject Create(int x, int y, int piece_type, int player_color) {
        GameObject obj = Instantiate(data.mem.chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.xBoard = x;
        cm.yBoard = y;
        cm.piece_type = piece_type;
        cm.player_color = player_color;
        cm.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj) {
        Chessman cm = obj.GetComponent<Chessman>();
        data.mem.positions[cm.xBoard, cm.yBoard] = obj;
    }

    public bool PositionOnBoard(int x, int y) {
        if (x < 0 || y < 0 || x >= data.mem.positions.GetLength(0) || y >= data.mem.positions.GetLength(1)) return false;
        return true;
    }

    public void PlaySound(AudioClip clip) {
        dataScript.audioSource.PlayOneShot(clip);
    }

    public void Winner(string playerWinner) {
        data.mem.gameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
}