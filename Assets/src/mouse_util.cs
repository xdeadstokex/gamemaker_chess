using UnityEngine;

public class mouse_util : MonoBehaviour {

    public struct button_signal {
        public int click;
        public int unclick;
        public int hold;
    }

    public static float x, y;
    public static float dx, dy;
    public static float scroll;

    public static button_signal left;
    public static button_signal right;
    public static button_signal middle;

    static Vector3 last;

    [RuntimeInitializeOnLoadMethod]
    static void init(){
        GameObject o = new GameObject("mouse_util");
        o.AddComponent<mouse_util>();
    }

    void Update(){
        Camera c = Camera.main;
        if (c == null) return;

        Vector3 cur = c.ScreenToWorldPoint(Input.mousePosition);

        x = cur.x;
        y = cur.y;

        dx = cur.x - last.x;
        dy = cur.y - last.y;

        scroll = Input.GetAxis("Mouse ScrollWheel");

        update_button(ref left, 0);
        update_button(ref right, 1);
        update_button(ref middle, 2);

        last = cur;
    }

    void update_button(ref button_signal b, int id){
        b.click   = Input.GetMouseButtonDown(id) ? 1 : 0;
        b.unclick = Input.GetMouseButtonUp(id)   ? 1 : 0;
        b.hold    = Input.GetMouseButton(id)     ? 1 : 0;
    }
}