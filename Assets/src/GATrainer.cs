using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Data;

[System.Serializable]
public class BotDNA {
    public float[] weights = new float[15];
    public float fitness = 0;
    public BotDNA() {}
    public void InitializeRandom() {
        weights[0] = Random.Range(80f, 150f);
        weights[1] = Random.Range(280f, 350f); 
        weights[2] = Random.Range(480f, 550f); 
        weights[3] = Random.Range(850f, 1000f);
        weights[4] = Random.Range(300f, 400f);  
        weights[5] = Random.Range(800f, 1000f);

        for (int i = 6; i < weights.Length; i++) {
            weights[i] = Random.Range(10f, 50f); 
        }
    }

    //Mutation
    public void Mutate(float mutationRate) {
        for (int i = 0; i < weights.Length; i++) {
            if (Random.value < mutationRate) {
                //mutation rate
                float change = weights[i] * Random.Range(-0.15f, 0.15f); 
                weights[i] += change;

                //keep in range queen and vip pawn
                if (i == 3 || i == 5) {
                    weights[i] = Mathf.Clamp(weights[i], 800f, 2000f);
                } else {
                    weights[i] = Mathf.Clamp(weights[i], 1f, 2000f); 
                }
            }
        }
    }

    //Crossover
    public BotDNA Crossover(BotDNA partner) {
        BotDNA child = new BotDNA();
        for (int i = 0; i < weights.Length; i++) {
            child.weights[i] = Random.value > 0.5f ? this.weights[i] : partner.weights[i];
        }
        return child;
    }
}


public class GATrainer : MonoBehaviour {
    public static GATrainer instance;

    [Header("Cấu hình GA")]
    public bool isTraining = false;
    public int populationSize = 10;
    public float mutationRate = 0.1f;
    public int eliteCount = 2; // Số lượng siêu elit được giữ nguyên sang đời sau
    
    [Header("Thông tin đang chạy")]
    public int currentGeneration = 1;
    public int currentMatchIndex = 0;
    
    [HideInInspector]
    public List<BotDNA> population = new List<BotDNA>();
    
    [Header("Training Mode (3 hoặc 4)")]
    public int playersPerMatch = 4;
    public BotDNA[] currentDNAs;

    string savePath;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.dataPath + "/BestBotBrain.json";
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (!isTraining)
        {
            LoadPvEBrain();
            return;
        }

        QualitySettings.vSyncCount = 0;  
        Application.targetFrameRate = -1;
        
        LoadPopulation();
        if (population.Count < populationSize) {
            population.Clear();
            for (int i = 0; i < populationSize; i++)
            {
                BotDNA newBot = new BotDNA();
                newBot.InitializeRandom();
                population.Add(newBot);
            } 
        }
        
        Time.timeScale = 20f; 
        StartNextMatch();
    }

    public void StartNextMatch() {
        playersPerMatch = Mathf.Clamp(playersPerMatch, 2, 4);

        if (currentMatchIndex >= populationSize / playersPerMatch) {
            EvolveNextGeneration();
            currentMatchIndex = 0;
        }

        currentDNAs = new BotDNA[playersPerMatch];
        for(int i = 0; i < playersPerMatch; i++) {
            currentDNAs[i] = population[currentMatchIndex * playersPerMatch + i];
        }

        SceneManager.LoadScene("main_entry");
    }

    public void ReportMatchResult(int loserColor, bool isDraw, int totalTurns, float[] materialScores) {
        
        //draw +5
        if (isDraw) {
            for (int i = 0; i < playersPerMatch; i++) {
                currentDNAs[i].fitness += 5f;
            }
        } 
        else {
            for (int i = 0; i < playersPerMatch; i++) {
                if (i == loserColor) {
                    currentDNAs[i].fitness += 2f; //lose +2
                } else {
                    currentDNAs[i].fitness += 90f; //win +90
                }
            }
        }

        //thắng nhiều quân hơn
        float totalEnemyMaterial = 0;
        for (int i = 0; i < playersPerMatch; i++) totalEnemyMaterial += materialScores[i];

        for (int i = 0; i < playersPerMatch; i++) {
            // Điểm của người hiện tại so với TỔNG lực lượng của các kẻ địch
            float myMaterial = materialScores[i];
            float enemyMaterial = totalEnemyMaterial - myMaterial;
            currentDNAs[i].fitness += (myMaterial - (enemyMaterial / (playersPerMatch - 1))) * 0.05f;
        }

        //phạt câu giờ
        float turnPenalty = totalTurns * 0.05f;
        for (int i = 0; i < playersPerMatch; i++) {
             currentDNAs[i].fitness -= turnPenalty;
        }

        currentMatchIndex++;
        StartNextMatch();
    }

    void EvolveNextGeneration() {
        population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        Debug.Log($"<color=cyan>Xong Thế hệ {currentGeneration}! Hậu Bot xịn nhất giá: {population[0].weights[3]} | Fitness: {population[0].fitness}</color>");

        SavePopulation();

        List<BotDNA> newPop = new List<BotDNA>();
        
        //2 tinh hoa ko cần lai tạp
        for (int i = 0; i < eliteCount; i++) {
            BotDNA elite = new BotDNA();
            System.Array.Copy(population[i].weights, elite.weights, 15);
            newPop.Add(elite);
        }

        //lai ghép
        for (int i = eliteCount; i < populationSize; i++) {
            BotDNA parentA = TournamentSelection();
            BotDNA parentB = TournamentSelection();
            BotDNA child = parentA.Crossover(parentB);
            child.Mutate(mutationRate);
            newPop.Add(child);
        }

        population = newPop;
        foreach (var dna in population) dna.fitness = 0; 
        currentGeneration++;
    }

    //chọn ngẫu nhiên 3 giữ lại 1
    BotDNA TournamentSelection() {
        int tournamentSize = 3;
        BotDNA best = null;
        for (int i = 0; i < tournamentSize; i++) {
            BotDNA randomBot = population[Random.Range(0, populationSize)];
            if (best == null || randomBot.fitness > best.fitness) {
                best = randomBot;
            }
        }
        return best;
    }

    void SavePopulation() {
        GAPopulationData data = new GAPopulationData { generation = currentGeneration, dnaList = population };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    void LoadPopulation() {
        if (File.Exists(savePath)) {
            string json = File.ReadAllText(savePath);
            GAPopulationData data = JsonUtility.FromJson<GAPopulationData>(json);
            population = data.dnaList;
            currentGeneration = data.generation;
            if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                Debug.Log($"<color=green>Đã load não bộ từ Thế hệ {currentGeneration}</color>");
        }
    }

    void LoadPvEBrain() {
        if (File.Exists(savePath)) {
            string json = File.ReadAllText(savePath);
            
            GAPopulationData fileData = JsonUtility.FromJson<GAPopulationData>(json);
            
            if (fileData != null && fileData.dnaList.Count > 0) {
                data.mem.pveBrains = fileData.dnaList; 
                
                if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                    Debug.Log($"<color=green>Đã nạp thành công {data.mem.pveBrains.Count} bộ não Siêu Trí Tuệ vào Bot!</color>");
            }
        } else {
            if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                Debug.LogWarning("Chưa có file BestBotBrain.json! Hãy bật Is Training để train trước.");
        }
    }
}