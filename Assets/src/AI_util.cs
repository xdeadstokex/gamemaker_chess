using System.Collections.Generic;
using System.Collections;
using UnityEngine;
<<<<<<< Updated upstream
=======
using UnityEngine.SceneManagement;
>>>>>>> Stashed changes

public static class AI_util {
    // =========================================================================
<<<<<<< Updated upstream
    // ENTRY POINT
=======
    // AI MAIN FUNC
>>>>>>> Stashed changes
    // =========================================================================
    public static IEnumerator PlayAITurn() {
        data.mem.isAIThinking = true;
        int color = data.mem.current_player_color;
        if (GATrainer.instance == null || !GATrainer.instance.isTraining) yield return new WaitForSeconds(0.1f);

<<<<<<< Updated upstream
        data.AIMove move = data.mem.ai_difficulty switch {
            AIDifficulty.Baby   => RandMove(GenerateAllValidMoves(color)),
            AIDifficulty.Easy   => CalculateGreedyMove(color), // Đổi thành Greedy
            AIDifficulty.Normal => CalculateMinimaxMove(color),
            AIDifficulty.Asean  => CalculateMinimaxMove(color), // Asean cũng dùng Minimax
            _                   => new data.AIMove { piece_index = -1 }
        };

        if (move.piece_index != -1 && !data.mem.gameOver)
            ExecuteAIMove(move, color);
        else {
            data.mem.gameOver = true;
            if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                Debug.Log("<color=orange>AI: no moves found, freezing game.</color>");
        }

=======
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
            data.mem.selected_a_piece = 0;
            piece_util.unselect_all_piece();
            move_plate_util.clear_move_plate();
            pvp_util.next_player_turn();
        }
>>>>>>> Stashed changes
        data.mem.isAIThinking = false;
    }

    static data.AIMove RandMove(List<data.AIMove> moves) =>
        moves.Count > 0 ? moves[Random.Range(0, moves.Count)] : new data.AIMove { piece_index = -1 };

    // =========================================================================
<<<<<<< Updated upstream
    // MOVE GENERATION
=======
    // RANDOM / GREEDY AI
>>>>>>> Stashed changes
    // =========================================================================
    public static List<data.AIMove> GenerateAllValidMoves(int color) {
        var moves = new List<data.AIMove>();
        var army  = data.mem.get_army(color);

        for (int i = 0; i < army.troop_count; i++) {
<<<<<<< Updated upstream
            ref var cp = ref army.troop_list[i];
            if (cp.rect == null) continue;

            for (int tx = 0; tx < data.mem.board_w; tx++)
            for (int ty = 0; ty < data.mem.board_h; ty++) {
                if (!piece_util.can_move_to(ref cp, tx, ty)) continue;
                bool atk = board_util.Cell(tx, ty).has_piece == 1;
                if (piece_util.IsSafeMove(i, color, tx, ty, atk))
                    moves.Add(new data.AIMove { piece_index = i, targetX = tx, targetY = ty, isAttack = atk });
=======
            ref data.chess_piece cp = ref army.troop_list[i];
            if (cp.rect == null) continue;

            for (int tx = 0; tx < data.mem.board_w; tx++) {
                for (int ty = 0; ty < data.mem.board_h; ty++) {
                    if (piece_util.can_move_to(ref cp, tx, ty)) {
                        bool isAttack = board_util.Cell(tx, ty).has_piece == 1;
                        moves.Add(new data.AIMove {
                            piece_index = i, targetX = tx, targetY = ty, isAttack = isAttack
                        });
                    }
                }
>>>>>>> Stashed changes
            }
        }
        return moves;
    }

    public static data.AIMove CalculateGreedyMove(int color) {
<<<<<<< Updated upstream
        var moves = GenerateAllValidMoves(color);
        if (moves.Count == 0) return new data.AIMove { piece_index = -1 };
=======
        List<data.AIMove> validMoves = GenerateAllValidMoves(color);
        if (validMoves.Count == 0) return new data.AIMove { piece_index = -1 };
>>>>>>> Stashed changes

        int   best    = -1;
        var   bestAtk = new List<data.AIMove>();

        foreach (var m in moves) {
            if (!m.isAttack) continue;
            ref var cell = ref board_util.Cell(m.targetX, m.targetY);
            var     t    = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
            int     val  = (t.piece_type == 5) ? 1000 : t.score;
            if (val > best) { best = val; bestAtk.Clear(); }
            if (val == best) bestAtk.Add(m);
        }

<<<<<<< Updated upstream
        return bestAtk.Count > 0
            ? bestAtk[Random.Range(0, bestAtk.Count)]
            : RandMove(moves);
    }

    // =========================================================================
    // TURN ORDER
=======
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
>>>>>>> Stashed changes
    // =========================================================================
    public static int GetNextActiveColor(int cur) {
        int n    = data.mem.total_players;
        int next = (cur + 1) % n;
        for (int i = 0; i < n; i++) {
            if (data.mem.armies[next].troop_count > 0) break;
            next = (next + 1) % n;
        }
        return next;
    }

    // =========================================================================
    // FAST MOVE / UNDO
    // =========================================================================
    public static data.UndoData DoMoveFast(data.AIMove move, int color) {
        var     army     = data.mem.get_army(color);
        ref var attacker = ref army.troop_list[move.piece_index];

<<<<<<< Updated upstream
        var undo = new data.UndoData {
            attacker_color = color,
            attacker_idx   = move.piece_index,
            old_x          = attacker.x,
            old_y          = attacker.y,
            old_score      = attacker.score,
            is_attack      = move.isAttack,
            is_king_dead   = false
        };
=======
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

    public static data.board_cell[] CloneBoard(data.board_cell[] original) {
        return (data.board_cell[])original.Clone();
    }

    public static data.army_data[] CloneArmies(data.army_data[] original) {
        data.army_data[] clone = new data.army_data[original.Length];
        for (int i = 0; i < original.Length; i++) {
            clone[i] = CloneArmy(original[i]);
        }
        return clone;
    }

    public static int SimulateMoveDataOnly(data.AIMove move, int color) {
        data.army_data army = data.mem.get_army(color);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];
>>>>>>> Stashed changes

        if (move.isAttack) {
            ref var cell   = ref board_util.Cell(move.targetX, move.targetY);
            var     enemy  = data.mem.get_army(cell.piece_color);
            ref var target = ref enemy.troop_list[cell.piece_index];

            undo.target_color = cell.piece_color;
            undo.target_idx   = cell.piece_index;
            undo.target_score = target.score;
            undo.target_rect  = target.rect;

            attacker.score += target.score;
<<<<<<< Updated upstream
            target.rect     = null;
=======
            target.rect = null; 
>>>>>>> Stashed changes
            board_util.clear_cell(move.targetX, move.targetY);

            if (target.piece_type == 5 || target.piece_type == 7) undo.is_king_dead = true;
        }

        board_util.clear_cell(attacker.x, attacker.y);
        attacker.x = move.targetX;
        attacker.y = move.targetY;
        board_util.set_cell(move.targetX, move.targetY, color, move.piece_index);
<<<<<<< Updated upstream

        return undo;
    }

    public static void UndoMoveFast(data.UndoData u) {
        var     army     = data.mem.get_army(u.attacker_color);
        ref var attacker = ref army.troop_list[u.attacker_idx];

        board_util.clear_cell(attacker.x, attacker.y);
        attacker.x     = u.old_x;
        attacker.y     = u.old_y;
        attacker.score = u.old_score;
        board_util.set_cell(attacker.x, attacker.y, u.attacker_color, u.attacker_idx);

        if (!u.is_attack) return;
        var     enemy  = data.mem.get_army(u.target_color);
        ref var target = ref enemy.troop_list[u.target_idx];
        target.score   = u.target_score;
        target.rect    = u.target_rect;
        board_util.set_cell(target.x, target.y, u.target_color, u.target_idx);
    }
=======
        
        return 0;
    }

    // =========================================================================
    // TACTICS & HEURISTICS
    // =========================================================================
    public static float GetPieceValue(int piece_type, int base_score) {
        if (piece_type == 5 || piece_type == 7) return 1000f; 
        if (piece_type == 4 || piece_type == 6) return 90f;   
        if (piece_type == 1) return 50f;   
        if (piece_type == 2 || piece_type == 3) return 30f;   
        return 10f; 
    }

    public static bool IsSquareAttacked(int x, int y, int defenderColor) {
        for (int c = 0; c < data.mem.total_players; c++) {
            if (c == defenderColor) continue; 
            data.army_data enemyArmy = data.mem.armies[c];    
            for (int i = 0; i < enemyArmy.troop_count; i++) {
                ref data.chess_piece enemyPiece = ref enemyArmy.troop_list[i];
                if (enemyPiece.rect == null) continue;
                
                int dx = Mathf.Abs(x - enemyPiece.x);
                int dy = Mathf.Abs(y - enemyPiece.y);

                if (enemyPiece.piece_type == 0 && enemyPiece.evolved == 0) {
                    int dir = (enemyPiece.player_color == 0) ? 1 : -1;
                    if (dx == 1 && (y - enemyPiece.y) == dir) return true;
                } 
                
                if (enemyPiece.piece_type == 7) {
                    if (dx <= 2 && dy <= 2 && (dx > 0 || dy > 0)) return true;
                }
                
                bool isKnight = (enemyPiece.piece_type == 2) || 
                                (enemyPiece.piece_type == 0 && enemyPiece.evolved == 1 && enemyPiece.evolved_type == 0) ||
                                (enemyPiece.piece_type == 6 && enemyPiece.evolved == 1);
                
                if (isKnight) {
                    if ((dx == 1 && dy == 2) || (dx == 2 && dy == 1)) return true;
                }

                if (piece_util.can_move_to(ref enemyPiece, x, y)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static float GetMoveHeuristic(data.AIMove move, int color) {
        float score = 0f;
        data.army_data army = data.mem.get_army(color);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];
        float attackerVal = GetPieceValue(attacker.piece_type, attacker.score);

        bool wasAttacked = IsSquareAttacked(attacker.x, attacker.y, color);
        bool willBeAttacked = IsSquareAttacked(move.targetX, move.targetY, color);

        if (move.isAttack) {
            ref data.board_cell cell = ref board_util.Cell(move.targetX, move.targetY);
            data.chess_piece target = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
            float targetVal = GetPieceValue(target.piece_type, target.score);

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

    private static float EvaluateBoardStatic(int ai_color, int colorToMove) {
        float aiScore = 0;
        float enemyScore = 0;

        for (int c = 0; c < data.mem.total_players; c++) {
            float score = 0;
            for (int p = 0; p < data.mem.armies[c].troop_count; p++) {
                ref data.chess_piece cp = ref data.mem.armies[c].troop_list[p];
                if (cp.rect != null) {
                    float pieceValue = GetPieceValue(cp.piece_type, cp.score);
                    
                    float centerDistanceX = Mathf.Abs(cp.x - (data.mem.board_w / 2.0f));
                    float centerDistanceY = Mathf.Abs(cp.y - (data.mem.board_h / 2.0f));
                    float positionalBonus = 1.0f - ((centerDistanceX + centerDistanceY) * 0.05f);

                    if (cp.piece_type == 0) {
                        int forwardSteps = (cp.player_color == 0) ? cp.y : (data.mem.board_h - 1 - cp.y);
                        positionalBonus += forwardSteps * 0.1f;
                    }

                    if (IsSquareAttacked(cp.x, cp.y, c)) {
                        if (c == colorToMove) pieceValue *= 0.6f; 
                        else pieceValue *= 0.1f; 
                    }

                    score += pieceValue + positionalBonus;
                }
            }
            if (c == ai_color) aiScore += score;
            else enemyScore += score;
        }
        
        float diff = aiScore - enemyScore;
        return 0.5f + Mathf.Clamp(diff * 0.002f, -0.48f, 0.48f);
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

        int maxIterations = 800; 
>>>>>>> Stashed changes

    // =========================================================================
    // HEURISTICS & EVALUATION
    // =========================================================================
    public static float GetPieceValue(ref data.chess_piece cp) {
        BotDNA dna = null;

<<<<<<< Updated upstream
        if (GATrainer.instance != null && GATrainer.instance.isTraining) {
            dna = (cp.player_color == 0) ? GATrainer.instance.currentWhiteDNA : GATrainer.instance.currentBlackDNA;
        }
        else if (data.mem != null && data.mem.ai_difficulty == AIDifficulty.Asean && data.mem.pveBrain != null) {
            dna = data.mem.pveBrain;
        }

        if (dna != null) { 
            switch (cp.piece_type) {
                case 5: case 7: return 10000f; 
                case 4: return dna.weights[3]; 
                case 6: return dna.weights[9];
                case 1: return dna.weights[2]; 
                case 2: return dna.weights[1];
                case 3: return dna.weights[1];
                case 0:
                    if (cp.evolved == 1) return (cp.evolved_type == 2) ? dna.weights[5] : dna.weights[4]; 
                    return dna.weights[0];
                default: return 100f;
=======
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
                SimulateMoveDataOnly(node.move, node.parent.colorToMove);
>>>>>>> Stashed changes
            }
        }

<<<<<<< Updated upstream
        switch (cp.piece_type) {
            case 5: case 7: return 10000f;
            case 4: case 6: return 900f;
            case 1:         return 500f;
            case 2: case 3: return 300f;
            case 0:
                if (cp.evolved == 1) return (cp.evolved_type == 2) ? 500f : 300f;
                return 100f;
            default: return 100f;
        }
    }
=======
            float nodeValue = 0.5f;

            if (node.untriedMoves.Count > 0) {
                int bestIdx = node.untriedMoves.Count - 1;
                data.AIMove move = node.untriedMoves[bestIdx];
                node.untriedMoves.RemoveAt(bestIdx);
>>>>>>> Stashed changes

    public static bool IsSquareAttacked(int x, int y, int defColor) {
        for (int c = 0; c < data.mem.total_players; c++) {
            if (c == defColor) continue;
            var army = data.mem.armies[c];

            for (int i = 0; i < army.troop_count; i++) {
                ref var ep = ref army.troop_list[i];
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
                        if (ep.evolved_type == 1 && piece_util.valid_diag(ref ep, x, y))  return true;
                    }
                    continue;
                }

                bool atk = ep.piece_type switch {
                    1 => piece_util.valid_line(ref ep, x, y),
                    2 => piece_util.valid_knight(ref ep, x, y),
                    3 => piece_util.valid_diag(ref ep, x, y),
                    4 => piece_util.valid_line(ref ep, x, y)   || piece_util.valid_diag(ref ep, x, y),
                    5 => piece_util.valid_king(ref ep, x, y),
                    6 => piece_util.valid_line(ref ep, x, y)   || piece_util.valid_diag(ref ep, x, y) || piece_util.valid_knight(ref ep, x, y),
                    _ => false
                };
<<<<<<< Updated upstream

                if (!atk && ep.evolved == 1) {
                    if (ep.piece_type == 2) atk = piece_util.valid_evo_knight(ref ep, x, y);
                    if (ep.piece_type == 3) atk = piece_util.valid_king(ref ep, x, y);
                    if (ep.piece_type == 6) atk = piece_util.valid_knight(ref ep, x, y);
                }

                if (atk) return true;
            }
        }
        return false;
    }

    public static float GetMoveHeuristic(data.AIMove move, int color) {
        var     army       = data.mem.get_army(color);
        ref var attacker   = ref army.troop_list[move.piece_index];
        float   atkVal     = GetPieceValue(ref attacker);
        float   score      = 0f;

        bool wasAttacked  = IsSquareAttacked(attacker.x,    attacker.y,    color);
        bool willAttacked = IsSquareAttacked(move.targetX,  move.targetY,  color);
=======
                node.children.Add(child);
                
                int moveResult = SimulateMoveDataOnly(move, node.colorToMove);
                node = child;
                
                if (moveResult == 1) {
                    nodeValue = (node.parent.colorToMove == ai_color) ? 1.0f : 0.0f;
                } else {
                    nodeValue = EvaluateBoardStatic(ai_color, node.colorToMove);
                }

                node.untriedMoves = GenerateAllValidMoves(node.colorToMove);
                node.untriedMoves.Sort((a, b) => GetMoveHeuristic(a, node.colorToMove).CompareTo(GetMoveHeuristic(b, node.colorToMove)));
            }

            while (node != null) {
                node.visits++;
                node.wins += nodeValue; 
                node = node.parent;
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
        if (depth == 0) return EvaluateBoardStatic(ai_color, colorToMove);

        List<data.AIMove> moves = GenerateAllValidMoves(colorToMove);
        if (moves.Count == 0) return isMaximizing ? -99999f : 99999f; 

        if (isMaximizing) {
            moves.Sort((a, b) => GetMoveHeuristic(b, colorToMove).CompareTo(GetMoveHeuristic(a, colorToMove)));
            float maxEval = -Mathf.Infinity;

            foreach (var move in moves) {
                var backupBoard = CloneBoard(data.mem.board);
                var backupArmies = CloneArmies(data.mem.armies);

                int isKingDead = SimulateMoveDataOnly(move, colorToMove);
                float eval;

                if (isKingDead == 1) {
                    eval = 99999f + depth; 
                } else {
                    eval = MinimaxAlphaBeta(depth - 1, alpha, beta, GetNextActiveColor(colorToMove), ai_color, false);
                }

                data.mem.board = backupBoard;
                data.mem.armies = backupArmies;

                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break; 
            }
            return maxEval;

        } else {
            moves.Sort((a, b) => GetMoveHeuristic(a, colorToMove).CompareTo(GetMoveHeuristic(b, colorToMove)));
            float minEval = Mathf.Infinity;

            foreach (var move in moves) {
                var backupBoard = CloneBoard(data.mem.board);
                var backupArmies = CloneArmies(data.mem.armies);

                int isKingDead = SimulateMoveDataOnly(move, colorToMove);
                float eval;

                if (isKingDead == 1) {
                    eval = -99999f - depth; 
                } else {
                    bool nextIsMaximizing = (GetNextActiveColor(colorToMove) == ai_color);
                    eval = MinimaxAlphaBeta(depth - 1, alpha, beta, GetNextActiveColor(colorToMove), ai_color, nextIsMaximizing);
                }

                data.mem.board = backupBoard;
                data.mem.armies = backupArmies;

                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha) break; 
            }
            return minEval;
        }
    }

    public static data.AIMove CalculateMinimaxMove(int ai_color) {
        BackupRealState(); 

        List<data.AIMove> moves = GenerateAllValidMoves(ai_color);
        moves.Sort((a, b) => GetMoveHeuristic(b, ai_color).CompareTo(GetMoveHeuristic(a, ai_color)));

        float bestScore = -Mathf.Infinity;
        data.AIMove bestMove = new data.AIMove { piece_index = -1 };
        float alpha = -Mathf.Infinity;
        float beta = Mathf.Infinity;

        int maxDepth = 3; 

        foreach (var move in moves) {
            float repPenalty = (data.lastAIMove.piece_index == move.piece_index) ? 0.5f : 0f;

            var backupBoard = CloneBoard(data.mem.board);
            var backupArmies = CloneArmies(data.mem.armies);

            int isKingDead = SimulateMoveDataOnly(move, ai_color);
            float score;

            if (isKingDead == 1) {
                score = 99999f; 
            } else {
                score = MinimaxAlphaBeta(maxDepth - 1, alpha, beta, GetNextActiveColor(ai_color), ai_color, false);
                score -= repPenalty;
            }

            data.mem.board = backupBoard;
            data.mem.armies = backupArmies;

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
    // EXECUTION
    // =========================================================================
    public static void ExecuteAIMove(data.AIMove move, int colorToMove) {
        data.army_data army = data.mem.get_army(colorToMove);
        ref data.chess_piece attacker = ref army.troop_list[move.piece_index];
>>>>>>> Stashed changes

        if (move.isAttack) {
            ref var cell   = ref board_util.Cell(move.targetX, move.targetY);
            var     target = data.mem.get_army(cell.piece_color).troop_list[cell.piece_index];
            float   tgtVal = GetPieceValue(ref target);
            score += tgtVal * 10f - atkVal;
            if (Mathf.Approximately(tgtVal, atkVal)) score += 50f;
        }

        if (willAttacked)      score -= atkVal * 10f;
        else if (wasAttacked)  score += atkVal * 10f;

        float cx = Mathf.Abs(move.targetX - data.mem.board_w * 0.5f);
        float cy = Mathf.Abs(move.targetY - data.mem.board_h * 0.5f);
        score += (10f - cx - cy) * 0.5f;

        return score;
    }

    public static float EvaluateBoardRaw(int aiColor, int colorToMove) {
        float ai = 0, enemy = 0;
        for (int c = 0; c < data.mem.total_players; c++) {
            float s = 0;
            for (int p = 0; p < data.mem.armies[c].troop_count; p++) {
                ref var cp = ref data.mem.armies[c].troop_list[p];
                if (cp.rect == null) continue;

                float val = GetPieceValue(ref cp);
                float cx  = Mathf.Abs(cp.x - data.mem.board_w * 0.5f);
                float cy  = Mathf.Abs(cp.y - data.mem.board_h * 0.5f);
                float pos = 10f - cx - cy;

                if (cp.piece_type == 0) {
                    int fwd = (cp.player_color == 0) ? cp.y : (data.mem.board_h - 1 - cp.y);
                    pos += fwd * 2f;
                }

                if ((cp.piece_type == 5 || cp.piece_type == 7) && IsSquareAttacked(cp.x, cp.y, c))
                    val *= (c == colorToMove) ? 0.5f : 0.1f;

                s += val + pos;
            }
            if (c == aiColor) ai += s; else enemy += s;
        }
        return ai - enemy;
    }

    static float EvaluateBoardStatic(int aiColor, int colorToMove) =>
        0.5f + Mathf.Clamp(EvaluateBoardRaw(aiColor, colorToMove) * 0.0002f, -0.48f, 0.48f);

    // =========================================================================
    // MCTS  (Easy / Asean)
    // =========================================================================
    public static data.AIMove CalculateMCTSMove(int aiColor) {
        var root = new data.MCTSNode {
            colorToMove  = aiColor,
            untriedMoves = GenerateAllValidMoves(aiColor)
        };
        root.untriedMoves.Sort((a, b) => GetMoveHeuristic(a, aiColor).CompareTo(GetMoveHeuristic(b, aiColor)));

        const int ITER = 2500;
        var undoStack  = new List<data.UndoData>();

        for (int i = 0; i < ITER; i++) {
            var node = root;
            undoStack.Clear();

            // 1. Selection
            while (node.untriedMoves.Count == 0 && node.children.Count > 0) {
                float   best = -Mathf.Infinity;
                data.MCTSNode pick = null;
                bool    isAI = (node.colorToMove == aiColor);

                foreach (var ch in node.children) {
                    float exploit = ch.wins / ch.visits;
                    if (!isAI) exploit = 1f - exploit;
                    if (isAI && data.lastAIMove.piece_index == ch.move.piece_index) exploit -= 0.5f;
                    float ucb = exploit + 1.414f * Mathf.Sqrt(Mathf.Log(node.visits) / ch.visits);
                    if (ucb > best) { best = ucb; pick = ch; }
                }
                node = pick;
                undoStack.Add(DoMoveFast(node.move, node.parent.colorToMove));
            }

            // 2. Expansion + 3. Evaluation
            float nodeVal = 0.5f;
            if (node.untriedMoves.Count > 0) {
                int last  = node.untriedMoves.Count - 1;
                var move  = node.untriedMoves[last];
                node.untriedMoves.RemoveAt(last);

                var child = new data.MCTSNode {
                    move        = move,
                    parent      = node,
                    colorToMove = GetNextActiveColor(node.colorToMove)
                };
                node.children.Add(child);

                var undo = DoMoveFast(move, node.colorToMove);
                undoStack.Add(undo);
                node = child;

                nodeVal = undo.is_king_dead
                    ? (node.parent.colorToMove == aiColor ? 1f : 0f)
                    : EvaluateBoardStatic(aiColor, node.colorToMove);

                node.untriedMoves = GenerateAllValidMoves(node.colorToMove);
                node.untriedMoves.Sort((a, b) =>
                    GetMoveHeuristic(a, node.colorToMove).CompareTo(GetMoveHeuristic(b, node.colorToMove)));
            }

            // 4. Backprop
            for (var n = node; n != null; n = n.parent) { n.visits++; n.wins += nodeVal; }

            // 5. Undo
            for (int j = undoStack.Count - 1; j >= 0; j--) UndoMoveFast(undoStack[j]);
        }

        data.MCTSNode best2 = null;
        int maxV = -1;
        foreach (var ch in root.children)
            if (ch.visits > maxV) { maxV = ch.visits; best2 = ch; }

        if (best2 != null) { data.lastAIMove = best2.move; return best2.move; }
        return CalculateGreedyMove(aiColor);
    }

    // =========================================================================
    // MINIMAX  (Normal)
    // =========================================================================
    public static data.AIMove CalculateMinimaxMove(int aiColor) {
        var moves = GenerateAllValidMoves(aiColor);
        moves.Sort((a, b) => GetMoveHeuristic(b, aiColor).CompareTo(GetMoveHeuristic(a, aiColor)));

        int alive = 0;
        for (int c = 0; c < data.mem.total_players; c++)
        for (int p = 0; p < data.mem.armies[c].troop_count; p++)
            if (data.mem.armies[c].troop_list[p].rect != null) alive++;

        int baseDepth = data.mem.total_players > 2
            ? (alive > 35 ? 2 : alive > 15 ? 3 : 4)
            : (alive > 24 ? 3 : alive > 10 ? 4 : 5);


        int depth = (data.mem.ai_difficulty == AIDifficulty.Asean) ? baseDepth : baseDepth - 1;

        float      bestScore = -Mathf.Infinity;
        float      alpha     = -Mathf.Infinity;
        float      beta      =  Mathf.Infinity;
        var        bestMove  = new data.AIMove { piece_index = -1 };

        foreach (var move in moves) {
            float penalty = (data.lastAIMove.piece_index == move.piece_index) ? 0.5f : 0f;
            var   undo    = DoMoveFast(move, aiColor);
            float score   = undo.is_king_dead
                ? 99999f
                : Minimax(depth - 1, alpha, beta, GetNextActiveColor(aiColor), aiColor, false) - penalty;
            UndoMoveFast(undo);

            if (score > bestScore) { bestScore = score; bestMove = move; }
            alpha = Mathf.Max(alpha, score);
        }

        if (bestMove.piece_index != -1) { data.lastAIMove = bestMove; return bestMove; }
        return CalculateGreedyMove(aiColor);
    }

    static float Minimax(int depth, float alpha, float beta, int colorToMove, int aiColor, bool isMax) {
        if (depth == 0) return EvaluateBoardRaw(aiColor, colorToMove);

        var moves = GenerateAllValidMoves(colorToMove);
        if (moves.Count == 0) return isMax ? -99999f : 99999f;

        int limit = (depth >= 3) ? 8 : 15;
        moves.Sort((a, b) => GetMoveHeuristic(b, colorToMove).CompareTo(GetMoveHeuristic(a, colorToMove)));

        float best = isMax ? -Mathf.Infinity : Mathf.Infinity;
        int   n    = 0;

        foreach (var move in moves) {
            if (n++ >= limit) break;
            var   undo = DoMoveFast(move, colorToMove);
            int   next = GetNextActiveColor(colorToMove);
            float eval = undo.is_king_dead
                ? (isMax ? 99999f + depth : -99999f - depth)
                : Minimax(depth - 1, alpha, beta, next, aiColor, next == aiColor);
            UndoMoveFast(undo);

            if (isMax) { best = Mathf.Max(best, eval); alpha = Mathf.Max(alpha, eval); }
            else       { best = Mathf.Min(best, eval); beta  = Mathf.Min(beta,  eval); }
            if (beta <= alpha) break;
        }
        return best;
    }

    // =========================================================================
    // EXECUTE (real board)
    // =========================================================================
    public static void ExecuteAIMove(data.AIMove move, int color) {
        var     army     = data.mem.get_army(color);
        ref var attacker = ref army.troop_list[move.piece_index];

        if (move.isAttack) {
            var pos = board_util.Cell(move.targetX, move.targetY).tile.obj.transform.position;
            piece_util.piece_attack(ref attacker, move.targetX, move.targetY, pos);
        } else {
            if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                sound_util.play_sound(data.mem.moveSound);
        }

        piece_util.move_piece(ref attacker, move.piece_index, color, move.targetX, move.targetY);
        data.mem.selected_a_piece = 0;
        piece_util.unselect_all_piece();
        move_plate_util.clear_move_plate();
        pvp_util.next_player_turn();
    }
}