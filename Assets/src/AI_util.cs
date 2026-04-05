using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AI_util {
    // =========================================================================
    // AI MAIN FUNC
    // =========================================================================
    public static IEnumerator PlayAITurn() {
        data.mem.isAIThinking = true;
        int currentColor = data.mem.current_player_color;
        yield return new WaitForSeconds(0.1f);
        data.AIMove chosenMove = new data.AIMove { piece_index = -1 };

        switch (data.mem.ai_difficulty) {
            case AIDifficulty.Baby:
                List<data.AIMove> validMoves = GenerateAllValidMoves(currentColor);
                if (validMoves.Count > 0) chosenMove = validMoves[Random.Range(0, validMoves.Count)];
                break;
            case AIDifficulty.Easy:
                chosenMove = CalculateMCTSMove(currentColor);
                break;
            case AIDifficulty.Normal:
                chosenMove = CalculateMinimaxMove(currentColor);
                break;
            case AIDifficulty.Asean:
                chosenMove = CalculateMCTSMove(currentColor); //Ml after

                break;
        }

        if (chosenMove.piece_index != -1 && !data.mem.gameOver) {
            ExecuteAIMove(chosenMove, currentColor);
        } else {
            data.mem.gameOver = true;
            Debug.Log("<color=orange>AI Backup: AI không tìm được nước đi. Đóng băng Game!</color>");
        }
        
        data.mem.isAIThinking = false;
    }
    
    // =========================================================================
    // RANDOM / GREEDY AI
    // =========================================================================
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
                        
                        if (piece_util.IsSafeMove(i, color, tx, ty, isAttack)) {
                            moves.Add(new data.AIMove {
                                piece_index = i, targetX = tx, targetY = ty, isAttack = isAttack
                            });
                        }
                    }
                }
            }
        }
        return moves;
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

    // =========================================================================
    // STATE MANAGEMENT & CLONING
    // =========================================================================
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
        
        return 0;
    }

    // =========================================================================
    // TACTICS & HEURISTICS
    // =========================================================================
    public static float GetPieceValue(ref data.chess_piece cp) {
        if (cp.piece_type == 5 || cp.piece_type == 7) return 10000f; 
        if (cp.piece_type == 4 || cp.piece_type == 6) return 900f;   
        if (cp.piece_type == 1) return 500f;                         
        if (cp.piece_type == 2 || cp.piece_type == 3) return 300f;   
        
        if (cp.piece_type == 0) {
            if (cp.evolved == 1) {
                if (cp.evolved_type == 2) return 500f; 
                return 300f;                         
            }
            return 100f; 
        }
        return 100f; 
    }

    public static bool IsSquareAttacked(int x, int y, int defenderColor) {
        for (int c = 0; c < data.mem.total_players; c++) {
            if (c == defenderColor) continue; 
            data.army_data enemyArmy = data.mem.armies[c];
            
            for (int i = 0; i < enemyArmy.troop_count; i++) {
                ref data.chess_piece ep = ref enemyArmy.troop_list[i]; 
                if (ep.rect == null) continue;
                
                int dx = Mathf.Abs(x - ep.x);
                int dy = Mathf.Abs(y - ep.y);

                if (ep.piece_type == 7) {
                    if (dx <= 2 && dy <= 2 && (dx > 0 || dy > 0)) return true;
                    continue; 
                }
                if (ep.piece_type == 0) {
                    int dir = (ep.player_color == 0) ? 1 : -1;
                    if (dx == 1 && (y - ep.y) == dir) return true; 
                    
                    if (ep.evolved == 1) {
                        if (ep.evolved_type == 2 && piece_util.valid_line(ref ep, x, y)) return true;
                        if (ep.evolved_type == 0 && piece_util.valid_knight(ref ep, x, y)) return true;
                        if (ep.evolved_type == 1 && piece_util.valid_diag(ref ep, x, y)) return true;
                    }
                    continue; 
                }

                bool attacks = false;
                switch (ep.piece_type) {
                    case 1: attacks = piece_util.valid_line(ref ep, x, y); break;
                    case 2: attacks = piece_util.valid_knight(ref ep, x, y); break;
                    case 3: attacks = piece_util.valid_diag(ref ep, x, y); break;
                    case 4: attacks = piece_util.valid_line(ref ep, x, y) || piece_util.valid_diag(ref ep, x, y); break;
                    case 5: attacks = piece_util.valid_king(ref ep, x, y); break;
                    case 6: attacks = piece_util.valid_line(ref ep, x, y) || piece_util.valid_diag(ref ep, x, y) || piece_util.valid_knight(ref ep, x, y); break;
                }
                
                if (ep.evolved == 1 && !attacks) {
                    if (ep.piece_type == 2) attacks = piece_util.valid_evo_knight(ref ep, x, y);
                    if (ep.piece_type == 3) attacks = piece_util.valid_king(ref ep, x, y);
                    if (ep.piece_type == 6) attacks = piece_util.valid_knight(ref ep, x, y);
                }

                if (attacks) return true;
            }
        }
        return false;
    }

    public static float GetMoveHeuristic(data.AIMove move, int color) {
        float score = 0f;
        data.army_data army = data.mem.get_army(color);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];
        float attackerVal = GetPieceValue(ref attacker);

        bool wasAttacked = IsSquareAttacked(attacker.x, attacker.y, color);
        bool willBeAttacked = IsSquareAttacked(move.targetX, move.targetY, color);

        if (move.isAttack) {
            ref data.board_cell cell = ref board_util.Cell(move.targetX, move.targetY);
            data.chess_piece target = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
            float targetVal = GetPieceValue(ref target);

            score += (targetVal * 10f) - attackerVal; 
            if (Mathf.Approximately(targetVal, attackerVal)) score += 50f; 
        }

        if (willBeAttacked) {
            score -= attackerVal * 10f; 
        } else if (wasAttacked) {
            score += attackerVal * 10f; 
        }

        float centerDx = Mathf.Abs(move.targetX - (data.mem.board_w / 2.0f));
        float centerDy = Mathf.Abs(move.targetY - (data.mem.board_h / 2.0f));
        score += (10f - (centerDx + centerDy)) * 0.5f; 

        return score;
    }

    // =========================================================================
    // "VALUE HEAD"
    // =========================================================================
    
    public static float EvaluateBoardRaw(int ai_color, int colorToMove) {
        float aiScore = 0;
        float enemyScore = 0;

        for (int c = 0; c < data.mem.total_players; c++) {
            float score = 0;
            for (int p = 0; p < data.mem.armies[c].troop_count; p++) {
                ref data.chess_piece cp = ref data.mem.armies[c].troop_list[p];
                if (cp.rect != null) {
                    float pieceValue = GetPieceValue(ref cp);
                    
                    float centerDistanceX = Mathf.Abs(cp.x - (data.mem.board_w / 2.0f));
                    float centerDistanceY = Mathf.Abs(cp.y - (data.mem.board_h / 2.0f));
                    float positionalBonus = 10f - (centerDistanceX + centerDistanceY); 

                    if (cp.piece_type == 0) {
                        int forwardSteps = (cp.player_color == 0) ? cp.y : (data.mem.board_h - 1 - cp.y);
                        positionalBonus += forwardSteps * 2f;
                    }

                    if (IsSquareAttacked(cp.x, cp.y, c)) {
                        pieceValue *= (c == colorToMove) ? 0.5f : 0.1f; 
                    }

                    score += pieceValue + positionalBonus;
                }
            }
            if (c == ai_color) aiScore += score;
            else enemyScore += score;
        }
        
        return aiScore - enemyScore;
    }
    private static float EvaluateBoardStatic(int ai_color, int colorToMove) {
        float diff = EvaluateBoardRaw(ai_color, colorToMove);
        return 0.5f + Mathf.Clamp(diff * 0.0002f, -0.48f, 0.48f); 
    }

    // =========================================================================
    // MCTS (EASY)
    // =========================================================================
    public static data.AIMove CalculateMCTSMove(int ai_color) {
        BackupRealState(); 

        data.MCTSNode root = new data.MCTSNode();
        root.colorToMove = ai_color;
        root.untriedMoves = GenerateAllValidMoves(ai_color);
        root.untriedMoves.Sort((a, b) => GetMoveHeuristic(a, ai_color).CompareTo(GetMoveHeuristic(b, ai_color)));

        int maxIterations = 2500; 

        for (int i = 0; i < maxIterations; i++) {
            data.MCTSNode node = root;
            
            List<data.UndoData> undoStack = new List<data.UndoData>();

            // 1. SELECTION
            while (node.untriedMoves.Count == 0 && node.children.Count > 0) {
                data.MCTSNode bestChild = null;
                float bestUCB = -Mathf.Infinity;
                bool isAITurn = (node.colorToMove == ai_color);

                foreach (var child in node.children) {
                    float exploit = child.wins / child.visits;
                    if (!isAITurn) exploit = 1f - exploit;

                    if (isAITurn && data.lastAIMove.piece_index == child.move.piece_index) {
                         exploit -= 0.5f; 
                    }

                    float explore = 1.414f * Mathf.Sqrt(Mathf.Log(node.visits) / child.visits);
                    float ucb = exploit + explore;

                    if (ucb > bestUCB) { bestUCB = ucb; bestChild = child; }
                }
                node = bestChild;
                
                undoStack.Add(DoMoveFast(node.move, node.parent.colorToMove));
            }

            // 2. EXPANSION & 3. EVALUATION
            float nodeValue = 0.5f;

            if (node.untriedMoves.Count > 0) {
                int bestIdx = node.untriedMoves.Count - 1;
                data.AIMove move = node.untriedMoves[bestIdx];
                node.untriedMoves.RemoveAt(bestIdx);

                data.MCTSNode child = new data.MCTSNode {
                    move = move,
                    parent = node,
                    colorToMove = GetNextActiveColor(node.colorToMove) 
                };
                node.children.Add(child);
                
                data.UndoData undo = DoMoveFast(move, node.colorToMove);
                undoStack.Add(undo);
                node = child;
                
                if (undo.is_king_dead) {
                    nodeValue = (node.parent.colorToMove == ai_color) ? 1.0f : 0.0f;
                } else {
                    nodeValue = EvaluateBoardStatic(ai_color, node.colorToMove);
                }

                node.untriedMoves = GenerateAllValidMoves(node.colorToMove);
                node.untriedMoves.Sort((a, b) => GetMoveHeuristic(a, node.colorToMove).CompareTo(GetMoveHeuristic(b, node.colorToMove)));
            }

            // 4. BACKPROPAGATION
            while (node != null) {
                node.visits++;
                node.wins += nodeValue; 
                node = node.parent;
            }

            // 5. UNDO TOÀN BỘ ĐỂ TRẢ BÀN CỜ VỀ TRẠNG THÁI GỐC
            for (int j = undoStack.Count - 1; j >= 0; j--) {
                UndoMoveFast(undoStack[j]);
            }
        }

        RestoreRealState();

        data.MCTSNode bestFinalChild = null;
        int maxVisits = -1;
        foreach (var child in root.children) {
            if (child.visits > maxVisits) { 
                maxVisits = child.visits; 
                bestFinalChild = child; 
            }
        }

        if (bestFinalChild != null) {
            data.lastAIMove = bestFinalChild.move; 
            return bestFinalChild.move;
        }
        return CalculateGreedyMove(ai_color);
    }

    // =========================================================================
    // MINIMAX ALPHA-BETA (NORMAL)
    // =========================================================================
    private static float MinimaxAlphaBeta(int depth, float alpha, float beta, int colorToMove, int ai_color, bool isMaximizing) {
        if (depth == 0) return EvaluateBoardRaw(ai_color, colorToMove);

        List<data.AIMove> moves = GenerateAllValidMoves(colorToMove);
        if (moves.Count == 0) return isMaximizing ? -99999f : 99999f; 

        int moveLimit = moves.Count;
        if (depth >= 3) moveLimit = 8;       
        else if (depth >= 1) moveLimit = 15; 
        
        int movesEvaluated = 0;

        if (isMaximizing) {
            moves.Sort((a, b) => GetMoveHeuristic(b, colorToMove).CompareTo(GetMoveHeuristic(a, colorToMove)));
            float maxEval = -Mathf.Infinity;

            foreach (var move in moves) {
                if (movesEvaluated >= moveLimit) break;
                data.UndoData undo = DoMoveFast(move, colorToMove);
                float eval = undo.is_king_dead ? (99999f + depth) : MinimaxAlphaBeta(depth - 1, alpha, beta, GetNextActiveColor(colorToMove), ai_color, false);
                UndoMoveFast(undo);

                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break; 
                movesEvaluated++;
            }
            return maxEval;

        } else {
            moves.Sort((a, b) => GetMoveHeuristic(b, colorToMove).CompareTo(GetMoveHeuristic(a, colorToMove)));
            float minEval = Mathf.Infinity;

            foreach (var move in moves) {
                if (movesEvaluated >= moveLimit) break;
                data.UndoData undo = DoMoveFast(move, colorToMove);
                float eval = undo.is_king_dead ? (-99999f - depth) : MinimaxAlphaBeta(depth - 1, alpha, beta, GetNextActiveColor(colorToMove), ai_color, (GetNextActiveColor(colorToMove) == ai_color));
                UndoMoveFast(undo);

                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha) break; 
                movesEvaluated++;
            }
            return minEval;
        }
    }

    public static data.AIMove CalculateMinimaxMove(int ai_color) {
        BackupRealState(); 

        List<data.AIMove> moves = GenerateAllValidMoves(ai_color);
        moves.Sort((a, b) => GetMoveHeuristic(b, ai_color).CompareTo(GetMoveHeuristic(a, ai_color)));

        int alivePieces = 0;
        for (int c = 0; c < data.mem.total_players; c++) {
            for (int p = 0; p < data.mem.armies[c].troop_count; p++) {
                if (data.mem.armies[c].troop_list[p].rect != null) alivePieces++;
            }
        }

        // --- DYNAMIC DEPTH ---
        int maxDepth = 3; 
        if (data.mem.total_players > 2) {
            if (alivePieces > 35) maxDepth = 2;      
            else if (alivePieces > 15) maxDepth = 3;
            else maxDepth = 4;                       
        } else {
            if (alivePieces > 24) maxDepth = 3;     
            else if (alivePieces > 10) maxDepth = 4;
            else maxDepth = 5;                  
        }

        float bestScore = -Mathf.Infinity;
        data.AIMove bestMove = new data.AIMove { piece_index = -1 };
        float alpha = -Mathf.Infinity;
        float beta = Mathf.Infinity;

        foreach (var move in moves) {
            float repPenalty = (data.lastAIMove.piece_index == move.piece_index) ? 0.5f : 0f;

            data.UndoData undo = DoMoveFast(move, ai_color);
            float score;

            if (undo.is_king_dead) {
                score = 99999f; 
            } else {
                score = MinimaxAlphaBeta(maxDepth - 1, alpha, beta, GetNextActiveColor(ai_color), ai_color, false);
                score -= repPenalty;
            }

            UndoMoveFast(undo);

            if (score > bestScore) {
                bestScore = score;
                bestMove = move;
            }
            alpha = Mathf.Max(alpha, score);
        }

        RestoreRealState(); 

        if (bestMove.piece_index != -1) {
            data.lastAIMove = bestMove;
            return bestMove;
        }
        
        return CalculateGreedyMove(ai_color);
    }

    // =========================================================================
    // HELPER
    // =========================================================================
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

    public static data.UndoData DoMoveFast(data.AIMove move, int color) {
        data.UndoData undo = new data.UndoData();
        data.army_data army = data.mem.get_army(color);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];

        undo.attacker_color = color;
        undo.attacker_idx = move.piece_index;
        undo.old_x = attacker.x;
        undo.old_y = attacker.y;
        undo.old_score = attacker.score;
        undo.is_attack = move.isAttack;
        undo.is_king_dead = false;

        if (move.isAttack) {
            ref data.board_cell cell = ref board_util.Cell(move.targetX, move.targetY);
            undo.target_color = cell.piece_color;
            undo.target_idx = cell.piece_index;
            
            data.army_data enemy = data.mem.get_army(undo.target_color);
            ref data.chess_piece target = ref enemy.troop_list[undo.target_idx];
            
            undo.target_score = target.score;
            undo.target_rect = target.rect;

            attacker.score += target.score;
            target.rect = null; 
            board_util.clear_cell(move.targetX, move.targetY);

            if (target.piece_type == 5 || target.piece_type == 7) undo.is_king_dead = true;
        }

        board_util.clear_cell(attacker.x, attacker.y);
        attacker.x = move.targetX;
        attacker.y = move.targetY;
        board_util.set_cell(move.targetX, move.targetY, color, move.piece_index);

        return undo;
    }

    public static void UndoMoveFast(data.UndoData undo) {
        data.army_data army = data.mem.get_army(undo.attacker_color);
        ref data.chess_piece attacker = ref army.troop_list[undo.attacker_idx];

        board_util.clear_cell(attacker.x, attacker.y);
        attacker.x = undo.old_x;
        attacker.y = undo.old_y;
        attacker.score = undo.old_score;
        board_util.set_cell(attacker.x, attacker.y, undo.attacker_color, undo.attacker_idx);

        if (undo.is_attack) {
            data.army_data enemy = data.mem.get_army(undo.target_color);
            ref data.chess_piece target = ref enemy.troop_list[undo.target_idx];
            
            target.score = undo.target_score;
            target.rect = undo.target_rect; 
            board_util.set_cell(target.x, target.y, undo.target_color, undo.target_idx);
        }
    }
}