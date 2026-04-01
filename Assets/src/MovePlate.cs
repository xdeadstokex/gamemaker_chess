using UnityEngine;

public class MovePlate : MonoBehaviour {
    public bool attack = false;

    public int mouse_click = 0;    // 0 = no, 1 = pressed
    public int mouse_unclick = 0;  // 0 = no, 1 = released
    public int mouse_hover = 0;    // 0 = no, 1 = hovering

    private GameObject reference = null;
    private int matrixX;
    private int matrixY;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Color baseColor;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = attack ? Color.red : Color.white;
        sr.color = baseColor;
    }

    // =========================================================================
    // INPUT — flags only, Game.Update() reads them
    // =========================================================================

    void OnMouseEnter() { mouse_hover = 1; }
    void OnMouseExit()  { mouse_hover = 0; }
    void OnMouseDown()  { mouse_click = 1; }
    void OnMouseUp()    { mouse_unclick = 1; }

    // =========================================================================
    // VISUAL — called by Game after reading hover flag
    // =========================================================================

    public void ApplyHoverVisual(bool hovered) {
        if (sr == null) return;
        if (hovered) {
            sr.color = baseColor + new Color(0.4f, 0.4f, 0.4f, 0f);
            transform.localScale = baseScale * 1.15f;
        } else {
            sr.color = baseColor;
            transform.localScale = baseScale;
        }
    }

    // =========================================================================
    // DATA
    // =========================================================================

    public void SetCoords(int x, int y)      { matrixX = x; matrixY = y; }
    public void SetReference(GameObject obj) { reference = obj; }
    public GameObject GetReference()         => reference;
    public int GetMatrixX()                  => matrixX;
    public int GetMatrixY()                  => matrixY;
}