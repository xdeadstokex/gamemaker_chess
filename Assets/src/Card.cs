using System.Collections;
using System.Collections.Generic;
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
        GameObject target = data.mem.positions[x, y];
        if (target == null) return;

        Chessman cm = target.GetComponent<Chessman>();
        if (cm == null) return;

        // Only affect enemy
        if (cm.player_color == data.mem.current_player_color) return;

        cm.AbsorbPoints(target, -value, target.transform.position);

        // if (data.mem.checkSound)
        //     data.mem.audioSource.PlayOneShot(data.mem.checkSound);
    }
}

public class BuffCard : Card {
    public override void CardActivate(int x, int y) {
        GameObject target = data.mem.positions[x, y];
        if (target == null) return;

        Chessman cm = target.GetComponent<Chessman>();
        if (cm == null) return;

        // Only affect ally
        if (cm.player_color != data.mem.current_player_color) return;

        cm.AbsorbPoints(target, value, target.transform.position);
        Debug.Log($"Buff {value} points to {cm.piece_type}");
    }
}