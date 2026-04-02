using UnityEngine;

public class rect_2d : MonoBehaviour {
    public GameObject obj;
    public GameObject visual;
    public SpriteRenderer sr;
    public BoxCollider2D col;
    public Sprite sprite;

    public int mouse_click = 0;
    public int mouse_unclick = 0;
    public int mouse_hover = 0;

    public static rect_2d create(float x, float y, float z = 0f) {
        GameObject o = new GameObject("rect_2d");
        o.transform.position = new Vector3(x, y, z);

        rect_2d r = o.AddComponent<rect_2d>();
        r.init();
        return r;
    }

    void init() {
        obj = gameObject;

        col = obj.AddComponent<BoxCollider2D>();

        visual = new GameObject("visual");
        visual.transform.SetParent(obj.transform);
        visual.transform.localPosition = Vector3.zero;

        sr = visual.AddComponent<SpriteRenderer>();
    }

    public void self_destroy() {
        Destroy(obj);
    }

    public void set_sprite(Sprite s) {
        sprite = s;
        sr.sprite = s;
    }

    public void set_color(Color c) {
        sr.color = c;
    }

    public void move_to(float x, float y, float z = 0f) {
        obj.transform.position = new Vector3(x, y, z);
    }

    public void move_to_board(int bx, int by, float z = 0f) {
        move_to(board_to_world(bx), board_to_world(by), z);
    }

    public void set_sprite_scale(float x_scale, float y_scale){
        visual.transform.localScale = new Vector3(x_scale, y_scale, 1f);
    }

    public void reset_visual_scale(){
        visual.transform.localScale = Vector3.one;
    }

	public void set_collider_size(float x_scale, float y_scale) {
		col.size = new Vector2(x_scale, y_scale);
	}

    public void fit_collider_to_sprite(Sprite s) {
        if (s != null) col.size = s.bounds.size;
    }

    public float board_to_world(int v) {
        return v * 1.28f - 4.48f;
    }

    void OnMouseEnter() { mouse_hover = 1; }
    void OnMouseExit()  { mouse_hover = 0; }
    void OnMouseDown()  { mouse_click = 1; }
    void OnMouseUp()    { mouse_unclick = 1; }
}