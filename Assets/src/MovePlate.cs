using UnityEngine;

public class MovePlate : MonoBehaviour{

    Chessman owner;
    int x,y;
    bool attack;

    public void Setup(Chessman o,int tx,int ty,bool atk){
        owner=o;
        x=tx;
        y=ty;
        attack=atk;

        if(attack){
            GetComponent<SpriteRenderer>().color=new Color(1f,0f,0f,1f);
        }
    }

    void OnMouseDown(){
        if(owner!=null){
            owner.ExecuteMove(x,y,attack);
        }
    }
}