using UnityEngine;

public enum PieceKind{ Pawn,Knight,Bishop,Rook,Queen,King }
public enum PieceType{ Light,ELight,KHeavy,BHeavy,RHeavy,Core }

public struct PieceData{
    public int player;
    public PieceKind kind;
    public bool evolved;
    public int weapon;
}

public class Chessman:MonoBehaviour{

    public GameObject controller;
    public GameObject movePlate;

    public int xBoard=-1;
    public int yBoard=-1;

    public int score;
    public int score_to_envo;

    public PieceType unitType;
    public PieceData piece;

    public bool firstMove=true;

    Game game;

    public Sprite black_queen,black_knight,black_bishop,black_king,black_rook,black_pawn;
    public Sprite white_queen,white_knight,white_bishop,white_king,white_rook,white_pawn;

    public Sprite e_black_queen,e_black_knight,e_black_bishop,e_black_king,e_black_rook;
    public Sprite e_white_queen,e_white_knight,e_white_bishop,e_white_king,e_white_rook;
    public Sprite e_black_pawn_knight,e_black_pawn_bishop,e_black_pawn_rook;
    public Sprite e_white_pawn_knight,e_white_pawn_bishop,e_white_pawn_rook;

    bool selected=false;

    bool pendingEvolve=false;
    PieceType pendingWeapon;

    public int player{ get{return piece.player;} }

    void Awake(){
        controller=GameObject.FindGameObjectWithTag("GameController");
        game=controller.GetComponent<Game>();
    }

    bool IsWhite(){ return piece.player==1; }
    GameObject At(int x,int y){ return data.mem.positions[x,y]; }
    bool IsEnemy(GameObject o){ return o.GetComponent<Chessman>().piece.player!=piece.player; }
    public string PlayerString(){ return piece.player==1?"white":"black"; }

    // ================= INIT =================
    public void Activate(){
        SetCoords();
        SpriteRenderer sr=GetComponent<SpriteRenderer>();

        switch(piece.kind){
            case PieceKind.Pawn:
                sr.sprite=IsWhite()?white_pawn:black_pawn;
                score=1; score_to_envo=4; unitType=PieceType.Light;
                break;

            case PieceKind.Knight:
                sr.sprite=piece.evolved?(IsWhite()?e_white_knight:e_black_knight):(IsWhite()?white_knight:black_knight);
                score=3; score_to_envo=5; unitType=PieceType.KHeavy;
                break;

            case PieceKind.Bishop:
                sr.sprite=piece.evolved?(IsWhite()?e_white_bishop:e_black_bishop):(IsWhite()?white_bishop:black_bishop);
                score=3; score_to_envo=5; unitType=PieceType.BHeavy;
                break;

            case PieceKind.Rook:
                sr.sprite=piece.evolved?(IsWhite()?e_white_rook:e_black_rook):(IsWhite()?white_rook:black_rook);
                score=5; score_to_envo=10; unitType=PieceType.RHeavy;
                break;

            case PieceKind.Queen:
                sr.sprite=piece.evolved?(IsWhite()?e_white_queen:e_black_queen):(IsWhite()?white_queen:black_queen);
                score=9; score_to_envo=15; unitType=PieceType.Core;
                break;

            case PieceKind.King:
                sr.sprite=piece.evolved?(IsWhite()?e_white_king:e_black_king):(IsWhite()?white_king:black_king);
                score=0; score_to_envo=7; unitType=PieceType.Core;
                break;
        }

        // pawn weapon evolve sprite override
        if(piece.kind==PieceKind.Pawn && piece.evolved && piece.weapon!=-1){
            unitType=PieceType.ELight;

            if(piece.weapon==(int)PieceKind.Knight)
                sr.sprite=IsWhite()?e_white_pawn_knight:e_black_pawn_knight;

            if(piece.weapon==(int)PieceKind.Bishop)
                sr.sprite=IsWhite()?e_white_pawn_bishop:e_black_pawn_bishop;

            if(piece.weapon==(int)PieceKind.Rook)
                sr.sprite=IsWhite()?e_white_pawn_rook:e_black_pawn_rook;
        }
    }

    // ================= INPUT =================
    void OnMouseEnter(){
        if(data.mem.gameOver) return;
        if(data.mem.currentPlayer!=PlayerString()) return;
        if(selected) return;

        DestroyMovePlates();
        ShowMoves();
    }

    void OnMouseExit(){
        if(selected) return;
        DestroyMovePlates();
    }

    void OnMouseDown(){
        if(data.mem.gameOver) return;
        if(data.mem.currentPlayer!=PlayerString()) return;

        selected=!selected;

        DestroyMovePlates();
        if(selected) ShowMoves();
    }

    public void DestroyMovePlates(){
        GameObject[] m=GameObject.FindGameObjectsWithTag("MovePlate");
        for(int i=0;i<m.Length;i++) Destroy(m[i]);
    }

    // ================= MOVE =================
    public bool CanMoveTo(int x,int y){
        if(!game.PositionOnBoard(x,y)) return false;

        GameObject t=At(x,y);
        if(t!=null && !IsEnemy(t)) return false;

        switch(piece.kind){
            case PieceKind.Rook: return Line(x,y);
            case PieceKind.Bishop: return piece.evolved?(Diag(x,y)||King(x,y)):Diag(x,y);
            case PieceKind.Queen: return Line(x,y)||Diag(x,y);
            case PieceKind.Knight: return piece.evolved?EKnight(x,y):Knight(x,y);
            case PieceKind.King: return piece.evolved?(Line(x,y)||Diag(x,y)):King(x,y);
            case PieceKind.Pawn: return PawnMove(x,y)||PawnEat(x,y);
        }
        return false;
    }

    bool Line(int tx,int ty){
        if(tx!=xBoard && ty!=yBoard) return false;

        int dx=(tx==xBoard)?0:(tx>xBoard?1:-1);
        int dy=(ty==yBoard)?0:(ty>yBoard?1:-1);

        int x=xBoard+dx;
        int y=yBoard+dy;

        while(x!=tx || y!=ty){
            if(At(x,y)!=null) return false;
            x+=dx; y+=dy;
        }
        return true;
    }

    bool Diag(int tx,int ty){
        if(Mathf.Abs(tx-xBoard)!=Mathf.Abs(ty-yBoard)) return false;

        int dx=(tx>xBoard)?1:-1;
        int dy=(ty>yBoard)?1:-1;

        int x=xBoard+dx;
        int y=yBoard+dy;

        while(x!=tx){
            if(At(x,y)!=null) return false;
            x+=dx; y+=dy;
        }
        return true;
    }

    bool Knight(int tx,int ty){
        int dx=Mathf.Abs(tx-xBoard);
        int dy=Mathf.Abs(ty-yBoard);
        return (dx==1&&dy==2)||(dx==2&&dy==1);
    }

    bool EKnight(int tx,int ty){
        int dx=Mathf.Abs(tx-xBoard);
        int dy=Mathf.Abs(ty-yBoard);

        if((dx==2&&dy==0)||(dx==0&&dy==2)){
            GameObject t=At(tx,ty);
            return t==null||IsEnemy(t);
        }
        return Knight(tx,ty);
    }

    bool King(int tx,int ty){
        int dx=Mathf.Abs(tx-xBoard);
        int dy=Mathf.Abs(ty-yBoard);
        return dx<=1 && dy<=1 && (dx+dy>0);
    }

    bool PawnMove(int tx,int ty){
        int dir=IsWhite()?1:-1;

        if(tx!=xBoard) return false;

        if(ty==yBoard+dir && At(tx,ty)==null) return true;

        if(firstMove && ty==yBoard+2*dir){
            if(At(xBoard,yBoard+dir)==null && At(tx,ty)==null) return true;
        }
        return false;
    }

    bool PawnEat(int tx,int ty){
        int dir=IsWhite()?1:-1;

        if(Mathf.Abs(tx-xBoard)==1 && ty==yBoard+dir){
            GameObject t=At(tx,ty);
            return t!=null && IsEnemy(t);
        }
        return false;
    }

    // ================= EXEC =================
    public void ExecuteMove(int tx,int ty,bool attack){

        GameObject target=At(tx,ty);

        if(attack && target!=null){
            game.PlaySound(data.mem.captureSound);

            Chessman v=target.GetComponent<Chessman>();
            AbsorbPoints(v.score,v.unitType,transform.position);

            if(v.piece.kind==PieceKind.King) game.Winner(PlayerString());

            Destroy(target);
        }
        else game.PlaySound(data.mem.moveSound);

        data.mem.positions[xBoard,yBoard]=null;

        xBoard=tx; yBoard=ty;
        firstMove=false;
        SetCoords();

        game.SetPosition(gameObject);

        if(pendingEvolve){
            if(piece.kind==PieceKind.Pawn && pendingWeapon!=0) EvolveWeapon(pendingWeapon);
            else Evolve();
            pendingEvolve=false;
        }

        selected=false;
        game.NextTurn();
        DestroyMovePlates();
    }

    // ================= MOVE PLATE =================
    void ShowMoves(){
        for(int x=0;x<8;x++)
        for(int y=0;y<8;y++)
            if(CanMoveTo(x,y))
                SpawnPlate(x,y,At(x,y)!=null);
    }

    void SpawnPlate(int x,int y,bool atk){
        float px=x*1.28f-4.48f;
        float py=y*1.28f-4.48f;

        GameObject mp=Instantiate(movePlate,new Vector3(px,py,-3),Quaternion.identity);
        mp.GetComponent<MovePlate>().Setup(this,x,y,atk);
    }

    // ================= EVOLVE =================
    public void AbsorbPoints(int victimScore,PieceType victimType,Vector3 pos){
        if(piece.evolved) return;

        score+=victimScore;

        if(piece.kind==PieceKind.Pawn){
            bool half=IsWhite()?(yBoard>=4):(yBoard<=3);

            bool heavy=victimType==PieceType.KHeavy||victimType==PieceType.BHeavy||victimType==PieceType.RHeavy;

            if(half && heavy){
                pendingEvolve=true;
                pendingWeapon=victimType;
                return;
            }
        }

        if(score>=score_to_envo) pendingEvolve=true;
    }

    void Evolve(){ piece.evolved=true; Activate(); }

    void EvolveWeapon(PieceType t){
        piece.evolved=true;

        if(t==PieceType.KHeavy) piece.weapon=(int)PieceKind.Knight;
        else if(t==PieceType.BHeavy) piece.weapon=(int)PieceKind.Bishop;
        else if(t==PieceType.RHeavy) piece.weapon=(int)PieceKind.Rook;

        unitType=PieceType.ELight;
        Activate();
    }

    float Scale(int v){ return v*1.28f-4.48f; }

    public void SetCoords(){
        transform.position=new Vector3(Scale(xBoard),Scale(yBoard),-1);
    }
}