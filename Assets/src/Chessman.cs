using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType { Light, ELight, KHeavy, BHeavy, RHeavy, Core }
public class Chessman : MonoBehaviour
{
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    //Position for this Chesspiece on the Board
    //The correct position will be set later
    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public Sprite e_black_queen, e_black_knight, e_black_bishop, e_black_king, e_black_rook, e_black_pawn_bishop, e_black_pawn_knight, e_black_pawn_rook;
    public Sprite e_white_queen, e_white_knight, e_white_bishop, e_white_king, e_white_rook, e_white_pawn_bishop, e_white_pawn_knight, e_white_pawn_rook;
    public int score;
    public PieceType unitType;
    public int score_to_envo; // điểm tiến hóa
    public int matrixX = -1;
    public int matrixY = -1;
    public string player;
    // ... các code khác ...
    // Thêm các hàm Getter/Setter để Game.cs có thể truy cập
    public void SetScore(int Score)
    {
        score = Score;
    }

    public int GetScore()
    {
        return score;
    }
    public bool isWhite()
    {
        // Gán trực tiếp vào biến public player để Card.cs có thể đọc được
        player = name.Contains("white") ? "white" : "black";;
        return (player == "white");
    }
    void Start()
    {
        isWhite();
    }
    public void Activate()
    {

        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_queen : black_queen;
                score = 9;
                score_to_envo = 15;
                unitType = PieceType.Core;
                break;

            case "black_knight":
            case "white_knight":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_knight : black_knight;
                score = 3;
                score_to_envo = 5;
                unitType = PieceType.KHeavy;

                break;

            case "black_bishop":
            case "white_bishop":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_bishop : black_bishop;
                score = 3;
                score_to_envo = 5;
                unitType = PieceType.BHeavy;
                break;

            case "black_king":
            case "white_king":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_king : black_king;
                score = 0; 
                score_to_envo = 7;
                unitType = PieceType.Core;
                break;

            case "black_rook":
            case "white_rook":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_rook : black_rook;
                score = 5;
                score_to_envo = 10;
                unitType = PieceType.RHeavy;
                break;

            case "black_pawn":
            case "white_pawn":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? white_pawn : black_pawn;
                score = 1;
                score_to_envo = 4;
                unitType = PieceType.Light;
                break;
            case "e_black_queen":
            case "e_white_queen":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_queen : e_black_queen;
                // escore = 9;
                unitType = PieceType.Core;
                break;

            case "e_black_knight":
            case "e_white_knight":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_knight : e_black_knight;
                // escore = 3;
                unitType = PieceType.KHeavy;
                break;

            case "e_black_bishop":
            case "e_white_bishop":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_bishop : e_black_bishop;
                // score = 3;
                unitType = PieceType.BHeavy;
                break;

            case "e_black_king":
            case "e_white_king":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_king : e_black_king;
                // score = 0; 
                unitType = PieceType.Core;
                break;

            case "e_black_rook":
            case "e_white_rook":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_rook : e_black_rook;
                // score = 5;
                unitType = PieceType.RHeavy;
                break;

            case "e_black_pawn_rook":
            case "e_white_pawn_rook":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_pawn_rook : e_black_pawn_rook;
                // score = 6;
                unitType = PieceType.ELight;
                break;
            case "e_black_pawn_bishop":
            case "e_white_pawn_bishop":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_pawn_bishop : e_black_pawn_bishop;
                // score = 5;
                unitType = PieceType.ELight;
                break;
            case "e_black_pawn_knight":
            case "e_white_pawn_knight":
                GetComponent<SpriteRenderer>().sprite = isWhite() ? e_white_pawn_knight : e_black_pawn_knight;
                // score = 5;
                unitType = PieceType.ELight;
                break;
        }
    }
    public bool CanMoveTo(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (!sc.PositionOnBoard(x, y)) return false;

        // Không được đi vào ô có quân mình đang đứng
        GameObject target = sc.GetPosition(x, y);
        if (target != null && target.GetComponent<Chessman>().player == player) return false;
        string[] nameParts = this.name.Split('_');
        if (nameParts[0] != "e")
        {
            string type = nameParts[1];
            switch (type)
            {
                case "queen":  return IsLineMoveValid(x, y) || IsDiagonalMoveValid(x, y);
                case "rook":   return IsLineMoveValid(x, y);
                case "bishop": return IsDiagonalMoveValid(x, y);
                case "knight": return IsKnightMoveValid(x, y);
                case "pawn":   return IsPawnMoveValid(x, y);
            }
        } 
        else // evolution case
        {
        string type = nameParts[2];
        switch (type)
        {
            case "queen":  return IsLineMoveValid(x, y) ||IsDiagonalMoveValid(x, y);
            case "rook":   return IsLineMoveValid(x, y);
            case "bishop": return IsDiagonalMoveValid(x, y) || IsKingMoveValid(x, y);
            case "knight": return additionalIsKnightMoveValid(x, y);
            case "pawn":   return IsPawnMoveValid(x, y);

        }
        }
        return false;
    }
    public bool IsLineMoveValid(int tx, int ty)
    {
        if (tx != xBoard && ty != yBoard) return false; // Không thẳng hàng/cột

        int xStep = System.Math.Sign(tx - xBoard);
        int yStep = System.Math.Sign(ty - yBoard);

        int curX = xBoard + xStep;
        int curY = yBoard + yStep;

        while (curX != tx || curY != ty)
        {
            if (controller.GetComponent<Game>().GetPosition(curX, curY) != null) return false; // Bị chặn
            curX += xStep;
            curY += yStep;
        }
        return true;
    }
    public bool IsDiagonalMoveValid(int tx, int ty)
    {
        if (System.Math.Abs(tx - xBoard) != System.Math.Abs(ty - yBoard)) return false;

        int xStep = System.Math.Sign(tx - xBoard);
        int yStep = System.Math.Sign(ty - yBoard);

        int curX = xBoard + xStep;
        int curY = yBoard + yStep;

        while (curX != tx || curY != ty)
        {
            if (controller.GetComponent<Game>().GetPosition(curX, curY) != null) return false;
            curX += xStep;
            curY += yStep;
        }
        return true;
    }
    public bool IsKnightMoveValid(int tx, int ty)
    {
        int dx = System.Math.Abs(tx - xBoard);
        int dy = System.Math.Abs(ty - yBoard);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }
    public bool additionalIsKnightMoveValid(int tx, int ty)
    {
        Game sc = controller.GetComponent<Game>();

        int dx = System.Math.Abs(tx - xBoard);
        int dy = System.Math.Abs(ty - yBoard);

        // Logic nhảy 2x0: (Cách 2 ô ngang, 0 ô dọc) HOẶC (Cách 0 ô ngang, 2 ô dọc)
        if ((dx == 2 && dy == 0) || (dx == 0 && dy == 2))
        {
            GameObject target = sc.GetPosition(tx, ty);

            // Ô trống hoặc ô có quân địch
            if (target == null) return true;
            return target.GetComponent<Chessman>().GetPlayer() != player;
        }

        return false;
    }
    public bool IsPawnMoveValid(int tx, int ty)
    {
        Game sc = controller.GetComponent<Game>();
        int direction = (player == "white") ? 1 : -1;
        int dx = tx - xBoard;
        int dy = ty - yBoard;

        // Ăn chéo
        if (System.Math.Abs(dx) == 1 && dy == direction)
        {
            GameObject target = sc.GetPosition(tx, ty);
            return target != null && target.GetComponent<Chessman>().player != player;
        }
        return false;
    }
    public bool IsKingMoveValid(int tx, int ty)
    {
        Game sc = controller.GetComponent<Game>();
        
        // Khoảng cách di chuyển của Vua theo trục X và Y
        int dx = System.Math.Abs(tx - xBoard);
        int dy = System.Math.Abs(ty - yBoard);

        // Vua chỉ được đi tối đa 1 ô (dx <= 1 và dy <= 1)
        // Đồng thời không được đứng yên tại chỗ (dx + dy > 0)
        if (dx <= 1 && dy <= 1 && (dx + dy > 0))
        {
            GameObject target = sc.GetPosition(tx, ty);
            
            // Ô trống hoặc ô có quân địch thì mới đi được
            if (target == null) return true;
            
            // Sử dụng GetPlayer() để tránh lỗi "Inaccessible" như lúc nãy nhé Trí
            return target.GetComponent<Chessman>().GetPlayer() != player;
        }

        return false;
    }
    public string GetPlayer()
    {
        return player;
    }
    public void SetCoords()
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        //Adjust by variable offset
        x *= 1.28f;
        y *= 1.28f;

        //Add constants (pos 0,0)
        x += -4.48f;
        y += -4.48f;

        //Set actual unity values
        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    private void OnMouseUp()
    {
        if (!controller.GetComponent<Game>().IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            //Remove all moveplates relating to previously selected piece
            DestroyMovePlates();

            //Create new MovePlates
            InitiateMovePlates();
        }
    }

    public void DestroyMovePlates()
    {
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }
// =========================================================================
// MOVE RULES
// =========================================================================

    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                QueenMovePlate();
                break;
            case "black_knight":
            case "white_knight":
                KnightMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                BishopMovePlate();
                break;
            case "black_king":
            case "white_king":
                KingMovePlate();
                break;
            case "black_rook":
            case "white_rook":
                RookMovePlate();
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
            // evolution
            case "e_black_queen":
            case "e_white_queen":
                QueenMovePlate();
                break;
            case "e_black_knight":
            case "e_white_knight":
                KnightMovePlate();
                eKnightMovePlateAddon();
                break;
            case "e_black_bishop":
            case "e_white_bishop":
                BishopMovePlate();
                KingMovePlate();
                break;
            case "e_black_king":
            case "e_white_king":
                QueenMovePlate();
                break;
            case "e_black_rook":
            case "e_white_rook":
                RookMovePlate();
                break;
            case "e_black_pawn_rook":
            case "e_white_pawn_rook":
                PawnMovePlate(xBoard, yBoard + (player == "white" ? 1 : -1));
                RookMovePlate();
                break;
            case "e_black_pawn_knight":
            case "e_white_pawn_knight":
                PawnMovePlate(xBoard, yBoard + (player == "white" ? 1 : -1));
                KnightMovePlate();
                break;
            case "e_black_pawn_bishop":
            case "e_white_pawn_bishop":
                PawnMovePlate(xBoard, yBoard + (player == "white" ? 1 : -1));
                BishopMovePlate();
                break;
            
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }
    public void QueenMovePlate()
    {
        RookMovePlate();
        BishopMovePlate();
    }
    public void RookMovePlate()
    {
        LineMovePlate(1, 0);
        LineMovePlate(0, 1);
        LineMovePlate(-1, 0);
        LineMovePlate(0, -1);
    }
    public void BishopMovePlate()
    {
        LineMovePlate(1, 1);
        LineMovePlate(1, -1);
        LineMovePlate(-1, 1);
        LineMovePlate(-1, -1);
    }
    public void KnightMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }
    public void eKnightMovePlateAddon()
    {
        PointMovePlate(xBoard - 2, yBoard);
        PointMovePlate(xBoard + 2, yBoard);
        PointMovePlate(xBoard, yBoard+2);
        PointMovePlate(xBoard, yBoard-2);
    
    }

    public void KingMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        
        // 1. Kiểm tra ô ngay phía trước (Đi 1 ô)
        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);

            // 2. Kiểm tra đi 2 ô (Chỉ khi ô 1 đang trống và Tốt đang ở vị trí xuất phát)
            // Quân Trắng xuất phát ở hàng 1, muốn đi lên hàng 3 (y + 1)
            if (player == "white" && yBoard == 1 && sc.GetPosition(x, y + 1) == null)
            {
                MovePlateSpawn(x, y + 1);
            }
            // Quân Đen xuất phát ở hàng 6, muốn đi xuống hàng 4 (y - 1)
            if (player == "black" && yBoard == 6 && sc.GetPosition(x, y - 1) == null)
            {
                MovePlateSpawn(x, y - 1);
            }
        }

        // 3. Kiểm tra ăn chéo bên phải (x + 1)
        if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null 
            && sc.GetPosition(x + 1, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x + 1, y);
        }

        // 4. Kiểm tra ăn chéo bên trái (x - 1)
        if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null 
            && sc.GetPosition(x - 1, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x - 1, y);
        }
    }
    // public void PawnMovePlateBishop()
    // {
    //     // có khả năng move của bishop nhưng không toàn bản đổ 

    // }

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 1.28f;
        y *= 1.28f;

        //Add constants (pos 0,0)
        x += -4.48f;
        y += -4.48f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
    public float scalePosition(int x)
    {
    return x * 1.28f - 4.48f;
    }
    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 1.28f;
        y *= 1.28f;

        //Add constants (pos 0,0)
        x += -4.48f;
        y += -4.48f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
// =========================================================================
// EVOLUTION SYSTEM
// =========================================================================

    public void AbsorbPoints(GameObject victim, int victimScore, Vector3 targetPos)
    {
        string[] nameParts = this.name.Split('_');
        Chessman victimScript = victim.GetComponent<Chessman>();


        if (nameParts[0] == "e") 
        {
            return;
        }
        Debug.Log($"===> HAM AbsorbPoints({victimScore})");
        
        score += victimScore;

        if (this.unitType == PieceType.Light)
        {


            bool isAtOpponentHalf = isWhite() ? (yBoard >= 4) : (yBoard <= 3);

            // Check xem victim có thuộc nhóm Heavy không
            bool ateHeavy = victimScript.unitType == PieceType.KHeavy || 
                            victimScript.unitType == PieceType.BHeavy || 
                            victimScript.unitType == PieceType.RHeavy;

            if (isAtOpponentHalf && ateHeavy)
            {
                EvolveWithWeapon(victimScript.unitType, targetPos);
                return; 
            }
        }

        else if (score >= score_to_envo)
        {
            Debug.Log(name + " absorbed " + victimScore + " points. Total: " + score);
            Evolve(targetPos);
        }
    }
    private void Evolve(Vector3 targetPos)
    {
        this.name = "e_" + this.name;
        Activate();
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(targetPos, 1.0f);
        Debug.Log("<color=green>" + this.name + " HAS EVOLVED!</color>");
    }
    //pawn
    private void EvolveWithWeapon(PieceType weaponType, Vector3 targetPos)
    {
        unitType = PieceType.ELight; 
        string evolvedAbility="";
        switch (weaponType)
        {
            case PieceType.KHeavy: 
                evolvedAbility = "knight"; 
                break;
            case PieceType.BHeavy: 
                evolvedAbility = "bishop"; 
                break;
            case PieceType.RHeavy: 
                evolvedAbility = "rook"; 
                break;
        }
        this.name = "e_" + this.name + "_" + evolvedAbility;
        Activate();
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(targetPos, 1.0f);
     }
    //queen
    // public void SendToWaitingZone()
    // {
    //     // 1. Lưu lại tọa độ THẬT trước khi ghi đè bằng tọa độ ảo
    //     int oldX = GetXBoard();
    //     int oldY = GetYBoard();

    //     // 2. Báo cho Controller xóa Hậu khỏi ô thực tế trên bàn cờ TRƯỚC
    //     GameObject controller = GameObject.FindGameObjectWithTag("GameController");
    //     if (controller != null) {
    //         controller.GetComponent<Game>().SetPositionEmpty(oldX, oldY);
    //     }

    //     // 3. Bây giờ mới gán tọa độ ảo để "cất" Hậu đi
    //     this.matrixX = -1;
    //     this.matrixY = isWhite() ? 0 : 7; 
    //     float x = scalePosition(this.matrixX); // Sẽ ra -5.76f
    //     float y = scalePosition(this.matrixY);
    //     // 4. Đẩy ra khỏi tầm mắt hoàn toàn (tránh vướng víu trên màn hình)
    //     this.transform.position = new Vector3(x, y, 0);

    //     // 5. Ẩn hình ảnh
    //     // GetComponent<SpriteRenderer>().enabled = false;
    // }
}
