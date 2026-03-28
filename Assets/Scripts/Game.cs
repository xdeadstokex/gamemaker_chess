using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    //Reference from Unity IDE
    public GameObject chesspiece;

    //Matrices needed, positions of each of the GameObjects
    //Also separate arrays for the players in order to easily keep track of them all
    //Keep in mind that the same objects are going to be in "positions" and "playerBlack"/"playerWhite"
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    //current turn
    private string currentPlayer = "white";

    //Game Ending
    private bool gameOver = false;

    public struct MoveData {
    public GameObject piece; // Quân cờ sẽ đi
    public int targetX;      // Tọa độ X đến
    public int targetY;      // Tọa độ Y đến
    public int score;        // Điểm số của nước đi này
}
    public AudioSource audioSource;
    public AudioClip moveSound;   // Âm thanh đi quân bình thường
    public AudioClip captureSound; // Âm thanh khi ăn quân
    public AudioClip checkSound;
    // public AudioClip winSound;
    public AudioClip startSound;
    public AudioClip endSound;
    public AudioClip timeLess;

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    //Unity calls this right when the game starts, there are a few built in functions
    //that Unity can call for you
    public void Start()
    {
        playerWhite = new GameObject[] { Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0), Create("white_king", 4, 0),
            Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1),
            Create("white_pawn", 3, 1), Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1) };
        playerBlack = new GameObject[] { Create("black_rook", 0, 7), Create("black_knight",1,7),
            Create("black_bishop",2,7), Create("black_queen",3,7), Create("black_king",4,7),
            Create("black_bishop",5,7), Create("black_knight",6,7), Create("black_rook",7,7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6),
            Create("black_pawn", 3, 6), Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6) };

        //Set all piece positions on the positions board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
        PlaySound(startSound);
    }
    
    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>(); //We have access to the GameObject, we need the script
        cm.name = name; //This is a built in variable that Unity has, so we did not have to declare it before
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate(); //It has everything set up so it can now Activate()
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        //Overwrites either empty space or whatever was there
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }
    // Thêm vào trong class Game trong file Game.cs

public bool IsKingInCheck(string kingColor)
    {
        Chessman king = GetKing(kingColor); // Tự viết hàm tìm King hoặc dùng mảng
        int kX = king.GetXBoard();
        int kY = king.GetYBoard();

        GameObject[] enemies = (kingColor == "white") ? playerBlack : playerWhite;

        foreach (GameObject enemyObj in enemies)
        {
            if (enemyObj == null) continue;
            if (enemyObj.GetComponent<Chessman>().CanMoveTo(kX, kY)) return true;
        }
        return false;
    }
public Chessman GetKing(string color)
    {
        // Xác định mảng quân cờ cần quét dựa trên màu sắc
        GameObject[] pieces = (color == "white") ? playerWhite : playerBlack;

        foreach (GameObject obj in pieces)
        {
            // Kiểm tra xem quân cờ còn tồn tại trên bàn không (tránh lỗi khi bị ăn)
            if (obj != null)
            {
                // Kiểm tra tên hoặc loại quân cờ
                if (obj.name.Contains("king"))
                {
                    return obj.GetComponent<Chessman>();
                }
            }
        }

        Debug.LogError("Không tìm thấy quân Vua phe " + color + "! Có thể bạn chưa kéo Prefab hoặc quân Vua đã bị xóa nhầm.");
        return null;
    }
    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
        if (!gameOver && IsKingInCheck(currentPlayer))
        {
            PlaySound(checkSound); // Phát file 'check' bạn đã kéo vào Inspector
            Debug.Log(currentPlayer + " is in CHECK!");
        }
        if (!gameOver && currentPlayer == "black")
        {
            Invoke("DoAITurn", 0.5f);
        }
    }
    void DoAITurn()
    {
        if (gameOver) return;

        List<MoveData> allPossibleMoves = new List<MoveData>();

        // 1. Tìm tất cả quân cờ đang có trên bàn
        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Chessman");

        foreach (GameObject p in allPieces)
        {
            Chessman script = p.GetComponent<Chessman>();
            
            // 2. Chỉ tính toán cho quân Đen
            if (script.GetPlayer() == "black")
            {
                // Tạo các ô di chuyển hợp lệ cho quân này (đã bao gồm chặn đường trong Chessman.cs)
                script.InitiateMovePlates();

                // Tìm các MovePlate vừa mới tạo ra
                GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");

                foreach (GameObject mp in movePlates)
                {
                    MovePlate mpScript = mp.GetComponent<MovePlate>();
                    int moveScore = 0;

                    // 3. Đánh giá nước đi (Ưu tiên ăn quân)
                    if (mpScript.attack)
                    {
                        GameObject victim = GetPosition(mpScript.GetMatrixX(), mpScript.GetMatrixY());
                        if (victim != null)
                        {
                            moveScore = GetPieceValue(victim.name);
                        }
                    }

                    allPossibleMoves.Add(new MoveData
                    {
                        piece = p,
                        targetX = mpScript.GetMatrixX(),
                        targetY = mpScript.GetMatrixY(),
                        score = moveScore
                    });
                }

                // 4. CỰC KỲ QUAN TRỌNG: Xóa sạch MovePlate ngay lập tức để quân sau không bị nhầm
                foreach (GameObject mp in movePlates)
                {
                    DestroyImmediate(mp); 
                }
            }
        }

        // 5. Chọn nước đi tốt nhất
        if (allPossibleMoves.Count > 0)
        {
            // Trộn ngẫu nhiên danh sách trước khi sắp xếp để AI không đi quá máy móc khi điểm bằng nhau
            System.Random rng = new System.Random();
            int n = allPossibleMoves.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                MoveData value = allPossibleMoves[k];
                allPossibleMoves[k] = allPossibleMoves[n];
                allPossibleMoves[n] = value;
            }

            // Sắp xếp lấy nước đi điểm cao nhất
            allPossibleMoves.Sort((a, b) => b.score.CompareTo(a.score));
            
            ExecuteAIMove(allPossibleMoves[0]);
        }
    }    
    int GetPieceValue(string name)
    {
        if (name.Contains("pawn")) return 10;
        if (name.Contains("knight")) return 30;
        if (name.Contains("bishop")) return 30;
        if (name.Contains("rook")) return 50;
        if (name.Contains("queen")) return 90;
        if (name.Contains("king")) return 900;
        return 0;
    }   

void ExecuteAIMove(MoveData move)
    {
        Chessman cm = move.piece.GetComponent<Chessman>();

        // Nếu có quân địch tại đó -> Xóa quân địch
        GameObject victim = GetPosition(move.targetX, move.targetY);
        if (victim != null) {
            PlaySound(captureSound);
            if (victim.name.Contains("king")) Winner("black");
            //PlaySound(winSound);
            Destroy(victim);
        }
        else
        {
            PlaySound(moveSound);
        }

        // Cập nhật mảng và vị trí
        SetPositionEmpty(cm.GetXBoard(), cm.GetYBoard());
        cm.SetXBoard(move.targetX);
        cm.SetYBoard(move.targetY);
        cm.SetCoords();
        SetPosition(move.piece);

        // Kết thúc lượt AI
        NextTurn();
    }

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            //Using UnityEngine.SceneManagement is needed here
            SceneManager.LoadScene("Game"); //Restarts the game by loading the scene over again
        }
    }
    
    public void Winner(string playerWinner)
    {
        gameOver = true;

        //Using UnityEngine.UI is needed here
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
    
}
