using UnityEngine;

public abstract class Card : ScriptableObject {
    public string cardName;
    public string description;
    public Sprite artwork;

    public CardType type;
    public int value;

    public abstract void CardActivate(int targetX, int targetY);
}

public class ThunderCard : Card {
    public override void CardActivate(int x, int y) {
        ref data.chess_piece target = ref data.mem.board[x, y];
        if (target.rect == null) return;
        if (target.player_color == data.mem.current_player_color) return;

        data.chess_piece damage = default;
        damage.score = value;

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.AbsorbPoints(ref target, ref damage, target.rect.obj.transform.position);
    }
}

public class BuffCard : Card {
    public override void CardActivate(int x, int y) {
        ref data.chess_piece target = ref data.mem.board[x, y];
        if (target.rect == null) return;
        if (target.player_color != data.mem.current_player_color) return;

        data.chess_piece buff = default;
        buff.score = value;

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.AbsorbPoints(ref target, ref buff, target.rect.obj.transform.position);
        Debug.Log($"Buff {value} points to {target.piece_type}");
    }
}