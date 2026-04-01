using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class Game : MonoBehaviour{
    GameObject controller;
    data dataScript;
    public void Awake(){
        controller = GameObject.FindGameObjectWithTag("GameController");
        dataScript = controller.GetComponent<data>();
    }

    public void PlaySound(AudioClip clip){
        dataScript.audioSource.PlayOneShot(clip);
    }
    //Unity calls this right when the game starts, there are a few built in functions
    //that Unity can call for you
    public void Start(){
        data.mem.playerWhite = new GameObject[] { Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0), Create("white_king", 4, 0),
            Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1),
            Create("white_pawn", 3, 1), Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1) };
        data.mem.playerBlack = new GameObject[] { Create("black_rook", 0, 7), Create("black_knight",1,7),
            Create("black_bishop",2,7), Create("black_queen",3,7), Create("black_king",4,7),
            Create("black_bishop",5,7), Create("black_knight",6,7), Create("black_rook",7,7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6),
            Create("black_pawn", 3, 6), Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6) };

        //Set all piece positions on the positions board
        for (int i = 0; i < data.mem.playerBlack.Length; i++){
            SetPosition(data.mem.playerBlack[i]);
            SetPosition(data.mem.playerWhite[i]);
        }
        PlaySound(dataScript.startSound);
    }
    
    public GameObject Create(string name, int x, int y){
        GameObject obj = Instantiate(data.mem.chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>(); //We have access to the GameObject, we need the script
        cm.name = name; //This is a built in variable that Unity has, so we did not have to declare it before
        cm.xBoard = x;
        cm.yBoard = y;
        cm.Activate(); //It has everything set up so it can now Activate()
        return obj;
    }
    
	public void SetPosition(GameObject obj){
        Chessman cm = obj.GetComponent<Chessman>();

        //Overwrites either empty space or whatever was there
        data.mem.positions[cm.xBoard, cm.yBoard] = obj;
    }




    public bool PositionOnBoard(int x, int y){
        if (x < 0 || y < 0 || x >= data.mem.positions.GetLength(0) || y >= data.mem.positions.GetLength(1)) return false;
        return true;
    }


public bool IsKingInCheck(string kingColor){
        Chessman king = GetKing(kingColor); // Tự viết hàm tìm King hoặc dùng mảng
        int kX = king.xBoard;
        int kY = king.yBoard;

        GameObject[] enemies = (kingColor == "white") ? data.mem.playerBlack : data.mem.playerWhite;

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
        GameObject[] pieces = (color == "white") ? data.mem.playerWhite : data.mem.playerBlack;

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

    public void NextTurn(){
        data.mem.currentPlayer = (data.mem.currentPlayer == "white") ? "black" : "white";
        if (!data.mem.gameOver && IsKingInCheck(data.mem.currentPlayer))
        {
            PlaySound(dataScript.checkSound); // Phát file 'check' bạn đã kéo vào Inspector
            Debug.Log(data.mem.currentPlayer + " is in CHECK!");
        }
        // if (!data.mem.gameOver && data.mem.currentPlayer == "black")
        // {
        //     Invoke("DoAITurn", 0.5f);
        // }
    }



    int GetPieceValue(string name){
        if (name.Contains("pawn")) return 1;
        if (name.Contains("knight")) return 3;
        if (name.Contains("bishop")) return 3;
        if (name.Contains("rook")) return 5;
        if (name.Contains("queen")) return 9;
        if (name.Contains("king")) return 90;
        return 0;
    }   

void ExecuteAIMove(data.MoveData move){
        Chessman cm = move.piece.GetComponent<Chessman>();

        // Nếu có quân địch tại đó -> Xóa quân địch
        GameObject victim = data.mem.positions[move.targetX, move.targetY];
        if (victim != null) {
            PlaySound(dataScript.captureSound);
            if (victim.name.Contains("king")) Winner("black");
            //PlaySound(winSound);
            Destroy(victim);
        }
        else{ PlaySound(dataScript.moveSound); }

        // Cập nhật mảng và vị trí
		data.mem.positions[cm.xBoard, cm.yBoard] = null; // set pos empty
        cm.xBoard = move.targetX;
        cm.yBoard = move.targetY;
        cm.SetCoords();
        SetPosition(move.piece);

        // Kết thúc lượt AI
        NextTurn();
    }

    public void Update(){
        if (data.mem.gameOver == true && Input.GetMouseButtonDown(0))
        {
            data.mem.gameOver = false;

            //Using UnityEngine.SceneManagement is needed here
            SceneManager.LoadScene("Game"); //Restarts the game by loading the scene over again
        }
        
    }
    
    public void Winner(string playerWinner)
    {
        data.mem.gameOver = true;

        //Using UnityEngine.UI is needed here
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }






/*
    void DoAITurn()
    {
        if (data.mem.gameOver) return;

        List<data.MoveData> allPossibleMoves = new List<data.MoveData>();

        // 1. Tìm tất cả quân cờ đang có trên bàn
        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Chessman");

        foreach (GameObject p in allPieces)
        {
            Chessman script = p.GetComponent<Chessman>();
            
            // 2. Chỉ tính toán cho quân Đen
            if (script.player == "black"){
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
                        GameObject victim = data.mem.positions[mpScript.GetMatrixX(), mpScript.GetMatrixY()];
                        if (victim != null)
                        {
                            moveScore = GetPieceValue(victim.name);
                        }
                    }

                    allPossibleMoves.Add(new data.MoveData
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
                data.MoveData value = allPossibleMoves[k];
                allPossibleMoves[k] = allPossibleMoves[n];
                allPossibleMoves[n] = value;
            }

            // Sắp xếp lấy nước đi điểm cao nhất
            allPossibleMoves.Sort((a, b) => b.score.CompareTo(a.score));
            
            ExecuteAIMove(allPossibleMoves[0]);
        }
    }
*/

}
