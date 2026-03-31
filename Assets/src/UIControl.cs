using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    public static UIControl Instance;

    public GameObject whiteQueenCard;
    public GameObject blackQueenCard;
    public Text notifyText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (whiteQueenCard) whiteQueenCard.SetActive(false);
        if (blackQueenCard) blackQueenCard.SetActive(false);
    }

    public void ActivateQueenCard(string color)
    {
        if (color == "white")
        {
            whiteQueenCard.SetActive(true);
            ShowMessage("White Queen ready.");
        }
        else
        {
            blackQueenCard.SetActive(true);
            ShowMessage("Black Queen ready.");
        }
    }

    public void ShowMessage(string msg)
    {
        if (notifyText) notifyText.text = msg;
    }

    public bool ProcessCardEffect(Card card, Chessman target)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        int cardPlayer = card.color == "white" ? 1 : 0;

        if (target.piece.player != cardPlayer)
        {
            ShowMessage("Cannot use on enemy.");
            return false;
        }

        switch (card.type)
        {
            case CardType.Resurrect:

                if (target.unitType == PieceType.KHeavy ||
                    target.unitType == PieceType.BHeavy ||
                    target.unitType == PieceType.RHeavy)
                {
                    game.Create(PieceKind.Queen, cardPlayer, target.xBoard, target.yBoard);

                    Destroy(target.gameObject);

                    ShowMessage("Queen revived.");
                    game.NextTurn();

                    return true;
                }

                ShowMessage("Need Heavy unit.");
                return false;

            default:
                return false;
        }
    }
}