using UnityEngine;

// Helpers for building and destroying menu GUI elements.
public static class gui_util {

    const float BTN_W = 1.8f;
    const float BTN_H = 0.8f;

    // Create a labelled button rect and register it for bulk destroy.
    public static rect_2d make_button(float x, float y, Color color){
        rect_2d btn = rect_2d.create(x, y, -1f);
        btn.set_sprite(data.mem.rect_2d_sprite);
        btn.set_sprite_size(BTN_W, BTN_H);
        btn.set_collider_size(BTN_W, BTN_H);
        btn.set_color(color);
        data.mem.menu_rects.Add(btn);
        return btn;
    }

    // Returns true and consumes the click if the button was unclicked.
    public static bool clicked(rect_2d btn) {
        if (btn == null) return false;
        if (btn.mouse_unclick == 1) {
            btn.mouse_unclick = 0;
            return true;
        }
        return false;
    }

    // Destroy every registered menu rect and the background.
    public static void clear_menu() {
        if (data.mem.main_screen_gui != null) {
            data.mem.main_screen_gui.self_destroy();
            data.mem.main_screen_gui = null;
        }
        foreach (rect_2d r in data.mem.menu_rects) {
            if (r != null) r.self_destroy();
        }
        data.mem.menu_rects.Clear();

        // null out named refs so HandleMenuInput skips dead objects
        data.mem.pvp_button   = null;
        data.mem.pve_button   = null;
        data.mem.btn_count1   = null;
        data.mem.btn_count2   = null;
        data.mem.btn_count3   = null;
        data.mem.back_button  = null;
    }
}