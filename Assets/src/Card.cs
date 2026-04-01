using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite artwork;
    
    public CardType type;
    public int value;
    public enum CardType { Buff, Debuff, GodQueen, DemonQueen, Event, Item }
    public abstract void CardActivate(int targetX, int targetY);
}
public class ThunderCard : Card
{
    int v = 3;
    public override void CardActivate(int x, int y)
    {
        GameObject target = data.mem.positions[x, y];
        if (target != null)
        {
            Chessman cm = target.GetComponent<Chessman>();
            // Chỉ tác động lên quân địch
            if (cm.player != data.mem.currentPlayer) 
            {
                cm.AbsorbPoints(target, -v, target.transform.position);
                
                // if (data.mem.checkSound) data.mem.audioSource.PlayOneShot(data.mem.checkSound);
            }
        }
    }
}

public class BuffCard : Card
{
    public override void CardActivate(int x, int y)
    {
        GameObject target = data.mem.positions[x, y];
        if (target != null)
        {
            Chessman cm = target.GetComponent<Chessman>();
            
            if (cm.player == data.mem.currentPlayer)
            {

                cm.AbsorbPoints(target, value, target.transform.position); 
                
                Debug.Log($"Buff {value} điểm cho {target.name}");
            }
        }
    }
}
