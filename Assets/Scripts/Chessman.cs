using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    //Position for this Chesspiece on the Board
    //The correct position will be set later
    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    private string player;

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();

        //Choose correct sprite based on piece's name
        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
        }
    }
    public bool CanMoveTo(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (!sc.PositionOnBoard(x, y)) return false;

        // Không được đi vào ô có quân mình đang đứng
        GameObject target = sc.GetPosition(x, y);
        if (target != null && target.GetComponent<Chessman>().player == player) return false;

        string type = this.name.Split('_')[1]; // Lấy "rook", "knight",... từ "black_rook"

        switch (type)
        {
            case "queen":  return IsLineMoveValid(x, y) || IsDiagonalMoveValid(x, y);
            case "rook":   return IsLineMoveValid(x, y);
            case "bishop": return IsDiagonalMoveValid(x, y);
            case "knight": return IsKnightMoveValid(x, y);
            case "pawn":   return IsPawnMoveValid(x, y);
        }
        return false;
    }
    private bool IsLineMoveValid(int tx, int ty)
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
    private bool IsDiagonalMoveValid(int tx, int ty)
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
    private bool IsKnightMoveValid(int tx, int ty)
    {
        int dx = System.Math.Abs(tx - xBoard);
        int dy = System.Math.Abs(ty - yBoard);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

private bool IsPawnMoveValid(int tx, int ty)
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

    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
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

    public void LMovePlate()
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

    public void SurroundMovePlate()
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
}
