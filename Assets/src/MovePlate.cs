using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    //Some functions will need reference to the controller
    public GameObject controller;

    //The Chesspiece that was tapped to create this MovePlate
    GameObject reference = null;

    //Location on the board
    public int matrixX;
    
    public int matrixY;

    //false: movement, true: attacking
    public bool attack = false;

    public void Start()
    {
        if (attack)
        {
            //Set to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game gameScript = controller.GetComponent<Game>();
        data dataScript = controller.GetComponent<data>();
        //Destroy the victim Chesspiece
        if (attack)
        {

            gameScript.PlaySound(dataScript.captureSound);
            GameObject cp = data.mem.positions[matrixX, matrixY];
            if (cp != null) 
            {
                int points = cp.GetComponent<Chessman>().GetScore();
                reference.GetComponent<Chessman>().AbsorbPoints(cp,points, this.transform.position);

            }
            string victimName = cp.name;
            if (victimName == "white_king") controller.GetComponent<Game>().Winner("black");
            if (victimName == "black_king") controller.GetComponent<Game>().Winner("white");
            string[] checkq = victimName.Split("_");
            if (checkq[0] == "e" && checkq[2] == "queen")
            {
                // checkequeen.SendToWaitingZone(); 
                Destroy(cp);
                if(checkq[1]=="white")
                {
                    // gameScript.Create("white_queen", -1, 0);
                    Debug.Log("<color=green>" + this.name + "Saved Queen");

                }
                else
                {
                    // gameScript.Create("black_queen", -1,7);
                    Debug.Log("<color=green>" + this.name + "Saved Queen");

                }

            }
            else{Destroy(cp);}
        }
        else
        {
            gameScript.PlaySound(dataScript.moveSound);
        }

        //Set the Chesspiece's original location to be empty
		data.mem.positions[reference.GetComponent<Chessman>().xBoard, reference.GetComponent<Chessman>().yBoard] = null;

        //Move reference chess piece to this position
        reference.GetComponent<Chessman>().xBoard = matrixX;
        reference.GetComponent<Chessman>().yBoard = matrixY;
        reference.GetComponent<Chessman>().SetCoords();

        //Update the matrix
        controller.GetComponent<Game>().SetPosition(reference);

        //Switch Current Player
        controller.GetComponent<Game>().NextTurn();

        //Destroy the move plates including self
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
    public int GetMatrixX() => matrixX;
    public int GetMatrixY() => matrixY;
}
