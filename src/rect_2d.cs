using UnityEngine;

public class rect_2d : MonoBehaviour {
    public GameObject obj;
    public SpriteRenderer sr;
    public BoxCollider2D col;
    public Sprite sprite;

    // input flags
    public int mouse_click = 0;
    public int mouse_unclick = 0;
    public int mouse_hover = 0;

    // =========================================================================
    // INIT / DESTROY
    // =========================================================================

    public static rect_2d create(float x, float y, float z = 0f) {
        GameObject o = new GameObject("rect_2d");
        o.transform.position = new Vector3(x, y, z);
        o.AddComponent<SpriteRenderer>();
        o.AddComponent<BoxCollider2D>();
        rect_2d r = o.AddComponent<rect_2d>();
        r.init();
        return r;
    }

    void init() {
        obj    = gameObject;
        sr     = GetComponent<SpriteRenderer>();
        col    = GetComponent<BoxCollider2D>();
    }

    public void self_destroy() { Destroy(obj); }

    // =========================================================================
    // VISUAL
    // =========================================================================

    public void set_sprite(Sprite s) { sprite = s; sr.sprite = s; }
    public void set_color(Color c)   { sr.color = c; }

    // =========================================================================
    // TRANSFORM
    // =========================================================================

    public void move_to(float x, float y, float z = 0f)    { obj.transform.position = new Vector3(x, y, z); }
    public void move_to_board(int bx, int by, float z = 0f) { move_to(board_to_world(bx), board_to_world(by), z); }
    public void resize(float w, float h)                    { obj.transform.localScale = new Vector3(w, h, 1f); }

    public float board_to_world(int v) => v * 1.28f - 4.48f;

    // =========================================================================
    // INPUT
    // =========================================================================

    void OnMouseEnter() { mouse_hover = 1; }
    void OnMouseExit()  { mouse_hover = 0; }
    void OnMouseDown()  { mouse_click = 1; }
    void OnMouseUp()    { mouse_unclick = 1; }
}