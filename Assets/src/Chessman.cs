using UnityEngine;

public enum PieceType { Light, ELight, KHeavy, BHeavy, RHeavy, Core }

public class Chessman : MonoBehaviour {
    public GameObject controller;

    public int mouse_click = 0;
    public int mouse_unclick = 0;
    public int mouse_hover = 0;
    public int selected = 0;
    public int hovered = 0;

    public int xBoard = -1;
    public int yBoard = -1;
    public int player_color;
    public int score;
    public int score_to_envo;
    public PieceType unitType;
    public int piece_type;      // 0=Pawn 1=Rook 2=Knight 3=Bishop 4=Queen 5=King
    public int evolved = 0;     // 0=normal 1=evolved
    public int evolved_type = 0;// 0=Knight 1=Bishop 2=Rook

    Game _game;
    SpriteRenderer _sr;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    void Start() { cache_components(); }

    void cache_components() {
        if (controller == null) controller = GameObject.FindGameObjectWithTag("GameController");
        _game = controller.GetComponent<Game>();
        _sr   = GetComponent<SpriteRenderer>();
    }

    public void Activate() {
        cache_components();
        SetCoords();
        ApplyPieceData();
    }

    // =========================================================================
    // SPRITE — assigned by Game via data sprites
    // =========================================================================

    public void set_sprite(Sprite s) { _sr.sprite = s; }

    // =========================================================================
    // SETUP
    // =========================================================================

    public int  GetScore()      => score;
    public void SetScore(int v) => score = v;

    public void ApplyPieceData() {
        bool w = (player_color == 0);

        if (evolved == 0) {
            switch (piece_type) {
                case 4: set_sprite(w ? data.mem.wp_queen  : data.mem.bp_queen);  score = 9; score_to_envo = 15; unitType = PieceType.Core;   break;
                case 5: set_sprite(w ? data.mem.wp_king   : data.mem.bp_king);   score = 0; score_to_envo = 7;  unitType = PieceType.Core;   break;
                case 1: set_sprite(w ? data.mem.wp_rook   : data.mem.bp_rook);   score = 5; score_to_envo = 10; unitType = PieceType.RHeavy; break;
                case 2: set_sprite(w ? data.mem.wp_knight : data.mem.bp_knight); score = 3; score_to_envo = 5;  unitType = PieceType.KHeavy; break;
                case 3: set_sprite(w ? data.mem.wp_bishop : data.mem.bp_bishop); score = 3; score_to_envo = 5;  unitType = PieceType.BHeavy; break;
                case 0: set_sprite(w ? data.mem.wp_pawn   : data.mem.bp_pawn);   score = 1; score_to_envo = 4;  unitType = PieceType.Light;  break;
            }
        } else {
            switch (piece_type) {
                case 4: set_sprite(w ? data.mem.wp_e_queen  : data.mem.bp_e_queen);  unitType = PieceType.Core;   break;
                case 5: set_sprite(w ? data.mem.wp_e_king   : data.mem.bp_e_king);   unitType = PieceType.Core;   break;
                case 1: set_sprite(w ? data.mem.wp_e_rook   : data.mem.bp_e_rook);   unitType = PieceType.RHeavy; break;
                case 2: set_sprite(w ? data.mem.wp_e_knight : data.mem.bp_e_knight); unitType = PieceType.KHeavy; break;
                case 3: set_sprite(w ? data.mem.wp_e_bishop : data.mem.bp_e_bishop); unitType = PieceType.BHeavy; break;
                case 0:
                    if      (evolved_type == 2) set_sprite(w ? data.mem.wp_e_pawn_rook   : data.mem.bp_e_pawn_rook);
                    else if (evolved_type == 0) set_sprite(w ? data.mem.wp_e_pawn_knight : data.mem.bp_e_pawn_knight);
                    else if (evolved_type == 1) set_sprite(w ? data.mem.wp_e_pawn_bishop : data.mem.bp_e_pawn_bishop);
                    unitType = PieceType.ELight;
                    break;
            }
        }
    }

    // =========================================================================
    // COORDS
    // =========================================================================

    public float BoardToWorld(int v) => v * 1.28f - 4.48f;

    public void SetCoords() {
        transform.position = new Vector3(BoardToWorld(xBoard), BoardToWorld(yBoard), -1f);
    }

    // =========================================================================
    // INPUT
    // =========================================================================

    void OnMouseEnter() { mouse_hover = 1; }
    void OnMouseExit()  { mouse_hover = 0; }
    void OnMouseDown()  { mouse_click = 1; }
    void OnMouseUp()    { mouse_unclick = 1; }

    // =========================================================================
    // CAN MOVE TO
    // =========================================================================

    public bool CanMoveTo(int tx, int ty) {
        if (!_game.PositionOnBoard(tx, ty)) return false;

        GameObject target = data.mem.positions[tx, ty];
        if (target != null && target.GetComponent<Chessman>().player_color == player_color) return false;

        bool baseMove = false;
        switch (piece_type) {
            case 0: baseMove = IsPawnMoveValid(tx, ty);                                 break;
            case 1: baseMove = IsLineMoveValid(tx, ty);                                 break;
            case 2: baseMove = IsKnightMoveValid(tx, ty);                               break;
            case 3: baseMove = IsDiagonalMoveValid(tx, ty);                             break;
            case 4: baseMove = IsLineMoveValid(tx, ty) || IsDiagonalMoveValid(tx, ty); break;
            case 5: baseMove = IsKingMoveValid(tx, ty);                                 break;
        }

        if (evolved == 0) return baseMove;

        switch (piece_type) {
            case 2: return baseMove || AdditionalIsKnightMoveValid(tx, ty);
            case 3: return baseMove || IsKingMoveValid(tx, ty);
            case 5: return IsLineMoveValid(tx, ty) || IsDiagonalMoveValid(tx, ty);
            default: return baseMove;
        }
    }

    bool IsLineMoveValid(int tx, int ty) {
        if (tx != xBoard && ty != yBoard) return false;
        return !IsBlocked(tx, ty);
    }

    bool IsDiagonalMoveValid(int tx, int ty) {
        if (Mathf.Abs(tx - xBoard) != Mathf.Abs(ty - yBoard)) return false;
        return !IsBlocked(tx, ty);
    }

    bool IsBlocked(int tx, int ty) {
        int sx = System.Math.Sign(tx - xBoard), sy = System.Math.Sign(ty - yBoard);
        int cx = xBoard + sx, cy = yBoard + sy;
        while (cx != tx || cy != ty) {
            if (data.mem.positions[cx, cy] != null) return true;
            cx += sx; cy += sy;
        }
        return false;
    }

    bool IsKnightMoveValid(int tx, int ty) {
        int dx = Mathf.Abs(tx - xBoard), dy = Mathf.Abs(ty - yBoard);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    bool AdditionalIsKnightMoveValid(int tx, int ty) {
        int dx = Mathf.Abs(tx - xBoard), dy = Mathf.Abs(ty - yBoard);
        if ((dx == 2 && dy == 0) || (dx == 0 && dy == 2)) {
            GameObject t = data.mem.positions[tx, ty];
            return t == null || t.GetComponent<Chessman>().player_color != player_color;
        }
        return false;
    }

    bool IsPawnMoveValid(int tx, int ty) {
        int dir = (player_color == 0) ? 1 : -1;
        int dx = tx - xBoard, dy = ty - yBoard;
        if (Mathf.Abs(dx) == 1 && dy == dir) {
            GameObject t = data.mem.positions[tx, ty];
            if (t == null) return false;
            Chessman cm = t.GetComponent<Chessman>();
            return cm != null && cm.player_color != player_color;
        }
        return false;
    }

    bool IsKingMoveValid(int tx, int ty) {
        int dx = Mathf.Abs(tx - xBoard), dy = Mathf.Abs(ty - yBoard);
        if (dx <= 1 && dy <= 1 && (dx + dy > 0)) {
            GameObject t = data.mem.positions[tx, ty];
            return t == null || t.GetComponent<Chessman>().player_color != player_color;
        }
        return false;
    }

    // =========================================================================
    // EVOLUTION
    // =========================================================================

    public void AbsorbPoints(GameObject victim, int pts, Vector3 targetPos) {
        if (evolved == 1) return;

        score += pts;
        if (score < 0) score = 0;

        Chessman vs = victim.GetComponent<Chessman>();
        if (vs == null) return;

        if (unitType == PieceType.Light) {
            bool inEnemyHalf = (player_color == 0) ? yBoard >= 4 : yBoard <= 3;
            bool ateHeavy = vs.unitType == PieceType.KHeavy || vs.unitType == PieceType.BHeavy || vs.unitType == PieceType.RHeavy;
            if (inEnemyHalf && ateHeavy) { EvolveWithWeapon(vs.unitType, targetPos); return; }
        }

        if (score >= score_to_envo) Evolve(targetPos);
    }

    void Evolve(Vector3 pos) {
        if (evolved == 1) return;
        evolved = 1;
        ApplyPieceData();
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(pos, 1f);
        Debug.Log($"<color=green>{piece_type} HAS EVOLVED!</color>");
    }

    void DeEvolve() {
        if (evolved == 0) return;
        evolved = 0;
        ApplyPieceData();
        Debug.Log($"<color=red>{piece_type} de-evolved!</color>");
    }

    void EvolveWithWeapon(PieceType weapon, Vector3 pos) {
        evolved = 1;
        unitType = PieceType.ELight;
        evolved_type = weapon == PieceType.KHeavy ? 0 : weapon == PieceType.BHeavy ? 1 : 2;
        ApplyPieceData();
        Camera.main.GetComponent<CameraControl>().ZoomInTarget(pos, 1f);
    }
}