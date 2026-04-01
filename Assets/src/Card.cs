using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Card : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite artwork;
    
    public CardType type;
    public int value;
    public enum CardType { Buff, Debuff, GodQueen, DemonQueen, Event, Item }
    public bool CanActivate(int x, int y) 
    {
        if (x < 0 || y < 0 || x >= 8 || y >= 8) return false;

        // 2. Lấy quân cờ tại vị trí x, y từ ma trận positions
        GameObject targetObj = data.mem.positions[x, y];
        
        // Điều kiện 1: Phải có quân cờ tại vị trí đó
        if (targetObj == null) return false;

        Chessman targetPiece = targetObj.GetComponent<Chessman>();
        string currentTurn = data.mem.currentPlayer;

        switch (type) 
        {
            case CardType.Debuff:
                Debug.Log("Checking Debuff on " + targetPiece.name);
                return targetPiece.player != currentTurn;
                
            case CardType.Buff:
                Debug.Log("Checking Buff on " + targetPiece.name);
                return targetPiece.player == currentTurn;
                        
            case CardType.GodQueen:
                Debug.Log("Checking GodQueen on " + targetPiece.name);
                return targetPiece.player == currentTurn && (targetPiece.unitType == PieceType.BHeavy || targetPiece.unitType == PieceType.KHeavy || targetPiece.unitType == PieceType.RHeavy);

            case CardType.DemonQueen:
                Debug.Log("Checking DemonQueen on " + targetPiece.name);
                return targetPiece.player == currentTurn && targetPiece.name.Contains("queen"); // get mark currentturn - get mark !currentturn >= 20
            case CardType.Event:
                Debug.Log("Checking Event on " + targetPiece.name);
                return true;
            case CardType.Item:
                Debug.Log("Checking Item on " + targetPiece.name);
                return targetPiece.player == currentTurn && targetPiece.name.Contains("king");}

        return false; 
    }
    public void ActivateEffect(int x, int y) 
    {
        GameObject targetObj = data.mem.positions[x, y];
        if (targetObj == null) return;
        Game gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        Chessman targetPiece = targetObj.GetComponent<Chessman>();
        Vector3 targetPos = targetObj.transform.position;
        PieceType pieceCheckType = targetPiece.unitType;
        switch (type) 
    {
        case CardType.Buff:
            // Cộng 2 điểm và kích hoạt kiểm tra tiến hóa
            targetPiece.AbsorbPoints(targetObj, value, targetPos);
            Debug.Log("Buff: +2 điểm cho " + targetPiece.name);
            break;

        case CardType.Debuff:
            // Trừ 2 điểm và kích hoạt kiểm tra thoái hóa (nếu có)
            targetPiece.AbsorbPoints(targetObj, -3, targetPos);
            Debug.Log("Debuff: -3 điểm cho " + targetPiece.name);
            break;
        
        case CardType.GodQueen:
            if (pieceCheckType == PieceType.BHeavy || pieceCheckType == PieceType.KHeavy || pieceCheckType == PieceType.RHeavy) 
            {
                // 1. Lưu lại thông tin cần thiết trước khi Destroy
                int oldX = targetPiece.xBoard;
                int oldY = targetPiece.yBoard;
                string teamPrefix = targetPiece.player == "white" ? "white_" : "black_";
                Destroy(targetObj); 
                GameObject newPiece = gameScript.Create(teamPrefix + "queen", oldX, oldY);
                gameScript.SetPosition(newPiece);
                Debug.Log("Queen hồi sinh: " + oldX + "," + oldY);
            }
            break;

        case CardType.DemonQueen:
            // chưa hiện thực thể tiến hóa này
            // targetPiece.name = "e_black_queen"; 
            // targetPiece.Activate(); 
            break; 

        case CardType.Event:
            // Hiệu ứng ngẫu nhiên: Ví dụ random điểm từ -3 đến 5
            // int randomScore = Random.Range(-3, 6);
            // targetPiece.score += randomScore;
            // if (targetPiece.score < 0) targetPiece.score = 0;
            // targetPiece.Activate(); // Cập nhật lại Sprite nếu cần
            break;

        case CardType.Item:
            // chưa hiện thực thể tiến hóa này
            break;
    }
    }
    
}

