using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class pvp_util {
    // Advance to the next living player in turn order.
    public static void next_player_turn() {
        int n = data.mem.total_players;
        int next = (data.mem.current_player_color + 1) % n;
 
        // skip eliminated armies (troop_count == 0)
        for (int i = 0; i < n; i++) {
            if (data.mem.armies[next].troop_count > 0) break;
            next = (next + 1) % n;
        }
 
        data.mem.current_player_color = next;
    }
}