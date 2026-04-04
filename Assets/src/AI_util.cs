using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
// these include are for List and IEnumerator

public static class AI_util {

    // =========================================================================
    // AI
    // =========================================================================
    //RANDOM AI
    public static List<data.AIMove> GenerateAllValidMoves(int color) {
        List<data.AIMove> moves = new List<data.AIMove>();
        data.army_data army = data.mem.get_army(color);

        for (int i = 0; i < army.troop_count; i++) {
            ref data.chess_piece cp = ref army.troop_list[i];
            
            if (cp.rect == null) continue;

            for (int tx = 0; tx < data.mem.board_w; tx++) {
                for (int ty = 0; ty < data.mem.board_h; ty++) {
                    if (piece_util.can_move_to(ref cp, tx, ty)) {
                        bool isAttack = board_util.Cell(tx, ty).has_piece == 1;
                        moves.Add(new data.AIMove {
                            piece_index = i,
                            targetX = tx,
                            targetY = ty,
                            isAttack = isAttack
                        });
                    }
                }
            }
        }
        return moves;
    }

    public static void ExecuteAIMove(data.AIMove move, int colorToMove) {
        data.army_data army = data.mem.get_army(colorToMove);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];

        if (move.isAttack) {
            Vector3 targetPos = board_util.Cell(move.targetX, move.targetY).tile.obj.transform.position;
            piece_util.piece_attack(ref attacker, move.targetX, move.targetY, targetPos);
        } else {
            sound_util.play_sound(data.mem.moveSound);
        }

        piece_util.move_piece(ref attacker, move.piece_index, colorToMove, move.targetX, move.targetY);

        data.mem.selected_a_piece = 0;
        piece_util.unselect_all_piece();
        move_plate_util.clear_move_plate();
        pvp_util.next_player_turn();
    }

    public static data.AIMove CalculateGreedyMove(int color) {
        List<data.AIMove> validMoves = GenerateAllValidMoves(color);
        
        if (validMoves.Count == 0) return new data.AIMove { piece_index = -1 };

        List<data.AIMove> attackMoves = new List<data.AIMove>();
        foreach (var move in validMoves) {
            if (move.isAttack) attackMoves.Add(move);
        }

        if (attackMoves.Count > 0) {
            int maxScore = -1;
            List<data.AIMove> bestAttacks = new List<data.AIMove>();

            foreach (var move in attackMoves) {
                ref data.board_cell cell = ref board_util.Cell(move.targetX, move.targetY);
                data.chess_piece target = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
                
                int targetScore = (target.piece_type == 5) ? 1000 : target.score;

                if (targetScore > maxScore) {
                    maxScore = targetScore;
                    bestAttacks.Clear();
                    bestAttacks.Add(move);
                } else if (targetScore == maxScore) {
                    bestAttacks.Add(move);
                }
            }
            return bestAttacks[Random.Range(0, bestAttacks.Count)];
        }

        return validMoves[Random.Range(0, validMoves.Count)];
    }

    public static IEnumerator PlayAITurn() {
        data.mem.isAIThinking = true;
        
        int currentColor = data.mem.current_player_color;

        //Monte Carlo
        yield return new WaitForSeconds(0.1f);
        data.AIMove chosenMove = CalculateMCTSMove(currentColor);

        if (chosenMove.piece_index != -1 && !data.mem.gameOver) {
            ExecuteAIMove(chosenMove, currentColor);
        } 
        data.mem.isAIThinking = false;
    }
    
    //MONTE CARLO
    public static int GetNextActiveColor(int currentColor) {
        int n = data.mem.total_players;
        int next = (currentColor + 1) % n;
        for (int i = 0; i < n; i++) {
            if (data.mem.armies[next].troop_count > 0) break;
            next = (next + 1) % n;
        }
        return next;
    }

    public static data.army_data CloneArmy(data.army_data original) {
        data.army_data clone = new data.army_data(original.color);
        clone.troop_count = original.troop_count;
        original.troop_list.CopyTo(clone.troop_list, 0); 
        return clone;
    }

    public static void BackupRealState() {
        data.mem.real_board = (data.board_cell[])data.mem.board.Clone();
        data.mem.real_armies = new data.army_data[data.mem.total_players];
        for (int i = 0; i < data.mem.total_players; i++) {
            data.mem.real_armies[i] = CloneArmy(data.mem.armies[i]);
        }
    }

    public static void RestoreRealState() {
        data.mem.board = (data.board_cell[])data.mem.real_board.Clone();
        for (int i = 0; i < data.mem.total_players; i++) {
            data.mem.armies[i] = CloneArmy(data.mem.real_armies[i]);
        }
    }

    //func that simulate move without ui/sound
    public static int SimulateMoveDataOnly(data.AIMove move, int color) {
        data.army_data army = data.mem.get_army(color);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];

        if (move.isAttack) {
            ref data.board_cell cell = ref board_util.Cell(move.targetX, move.targetY);
            data.army_data enemy = data.mem.get_army(cell.piece_color);
            ref data.chess_piece target = ref enemy.troop_list[cell.piece_index];

            attacker.score += target.score;
            
            target.rect = null; 
            board_util.clear_cell(move.targetX, move.targetY);

            if (target.piece_type == 5 || target.piece_type == 7) return 1; 
        }

        board_util.clear_cell(attacker.x, attacker.y);
        attacker.x = move.targetX;
        attacker.y = move.targetY;
        board_util.set_cell(move.targetX, move.targetY, color, move.piece_index);
        
        return 0;//no one win
    }

    public static data.AIMove CalculateMCTSMove(int ai_color) {
        BackupRealState(); //backup current state

        data.MCTSNode root = new data.MCTSNode();
        root.colorToMove = ai_color;
        root.untriedMoves = GenerateAllValidMoves(ai_color);

        int maxIterations = 200; 

        for (int i = 0; i < maxIterations; i++) {
            RestoreRealState(); 
            data.MCTSNode node = root;

            // 1. SELECTION
            while (node.untriedMoves.Count == 0 && node.children.Count > 0) {
                data.MCTSNode bestChild = null;
                float bestUCB = -Mathf.Infinity;
                foreach (var child in node.children) {
                    float ucb = (child.wins / child.visits) + 1.414f * Mathf.Sqrt(Mathf.Log(node.visits) / child.visits);
                    if (ucb > bestUCB) { bestUCB = ucb; bestChild = child; }
                }
                node = bestChild;
                SimulateMoveDataOnly(node.move, GetNextActiveColor(node.parent.colorToMove));
            }

            // 2. EXPANSION
            if (node.untriedMoves.Count > 0) {
                int rIdx = Random.Range(0, node.untriedMoves.Count);
                data.AIMove move = node.untriedMoves[rIdx];
                node.untriedMoves.RemoveAt(rIdx);

                data.MCTSNode child = new data.MCTSNode {
                    move = move,
                    parent = node,
                    colorToMove = GetNextActiveColor(node.colorToMove) 
                };
                node.children.Add(child);
                
                SimulateMoveDataOnly(move, node.colorToMove);
                node = child;
                node.untriedMoves = GenerateAllValidMoves(node.colorToMove);
            }

            // 3. SIMULATION
            int currentSimColor = node.colorToMove;
            int depth = 0;
            int result = 0; 

            while (depth < 15) { 
                List<data.AIMove> moves = GenerateAllValidMoves(currentSimColor);
                if (moves.Count == 0) break; 

                data.AIMove randomMove = moves[Random.Range(0, moves.Count)];
                int moveResult = SimulateMoveDataOnly(randomMove, currentSimColor);
                
                if (moveResult == 1) {
                    result = (currentSimColor == ai_color) ? 1 : -1;
                    break;
                }
                
                currentSimColor = GetNextActiveColor(currentSimColor);
                depth++;
            }

            // 4. BACKPROPAGATION
            while (node != null) {
                node.visits++;
                if (result == 1) node.wins += 1f;
                else if (result == 0) node.wins += 0.5f; 
                node = node.parent;
            }
        }

        RestoreRealState(); 

        data.MCTSNode bestFinalChild = null;
        int maxVisits = -1;
        foreach (var child in root.children) {
            if (child.visits > maxVisits) { maxVisits = child.visits; bestFinalChild = child; }
        }

        if (bestFinalChild != null) return bestFinalChild.move;
        
        return CalculateGreedyMove(ai_color);
    }
}