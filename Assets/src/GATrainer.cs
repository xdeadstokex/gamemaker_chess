using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Data;

[System.Serializable]
public class BotDNA {
    public float[] weights = new float[12];
    public float fitness = 0;

    public BotDNA() {}

    public void InitializeRandom() {
        for (int i = 0; i < weights.Length; i++) {
            weights[i] = Random.Range(10f, 500f); 
        }
    }

    // Lai ghép (Uniform Crossover)
    public BotDNA Crossover(BotDNA partner) {
        BotDNA child = new BotDNA();
        for (int i = 0; i < weights.Length; i++) {
            child.weights[i] = Random.value > 0.5f ? this.weights[i] : partner.weights[i];
        }
        return child;
    }

    // Đột biến (Mutate)
    public void Mutate(float mutationRate) {
        for (int i = 0; i < weights.Length; i++) {
            if (Random.value < mutationRate) {
                // Đột biến theo tỷ lệ phần trăm thay vì cộng trừ cứng
                float change = weights[i] * Random.Range(-0.2f, 0.2f); // +/- 20%
                weights[i] += change;
                if (weights[i] < 1f) weights[i] = 1f; // Không cho âm hoặc bằng 0
            }
        }
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
    
    public BotDNA currentWhiteDNA;
    public BotDNA currentBlackDNA;

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
        
        LoadPopulation(); // Thử load não cũ nếu có
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
        if (currentMatchIndex >= populationSize / 2) {
            EvolveNextGeneration();
            currentMatchIndex = 0;
        }

        currentWhiteDNA = population[currentMatchIndex * 2];
        currentBlackDNA = population[currentMatchIndex * 2 + 1];

        SceneManager.LoadScene("main_entry");
    }

    // Đã nâng cấp hàm Report để nhận nhiều thông số hơn
    public void ReportMatchResult(int loserColor, bool isDraw, int totalTurns, float whiteMaterial, float blackMaterial) {
        // 1. Điểm cơ bản (Thắng/Thua/Hòa)
        if (isDraw) {
            currentWhiteDNA.fitness += 5f;
            currentBlackDNA.fitness += 5f;
        } else {
            if (loserColor == 1) { // Trắng thắng
                currentWhiteDNA.fitness += 20f; 
                currentBlackDNA.fitness += 2f; // Điểm an ủi
            } else { // Đen thắng
                currentBlackDNA.fitness += 20f;
                currentWhiteDNA.fitness += 2f;
            }
        }

        // 2. Điểm phần thưởng: Chênh lệch lực lượng (Khuyến khích ăn quân)
        currentWhiteDNA.fitness += (whiteMaterial - blackMaterial) * 0.05f;
        currentBlackDNA.fitness += (blackMaterial - whiteMaterial) * 0.05f;

        // 3. Phạt nếu câu giờ vô nghĩa (Khuyến khích thắng nhanh)
        float turnPenalty = totalTurns * 0.02f;
        if (loserColor == 1 && !isDraw) currentWhiteDNA.fitness -= turnPenalty;
        else if (loserColor == 0 && !isDraw) currentBlackDNA.fitness -= turnPenalty;

        currentMatchIndex++;
        StartNextMatch();
    }

    void EvolveNextGeneration() {
        // Sắp xếp theo Fitness giảm dần
        population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        Debug.Log($"<color=cyan>Xong Thế hệ {currentGeneration}! Hậu Bot xịn nhất giá: {population[0].weights[3]} | Fitness: {population[0].fitness}</color>");

        SavePopulation(); // Lưu lại lứa tốt nhất

        List<BotDNA> newPop = new List<BotDNA>();
        
        // 1. Giữ lại tinh hoa (Elitism) - Chuyển thẳng sang lứa sau không lai tạp
        for (int i = 0; i < eliteCount; i++) {
            BotDNA elite = new BotDNA();
            System.Array.Copy(population[i].weights, elite.weights, 12); // Copy value, not reference
            newPop.Add(elite);
        }

        // 2. Lai ghép phần còn lại bằng Tournament Selection
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

    // Chọn ngẫu nhiên 3 con, lấy con giỏi nhất trong 3 con đó (Giữ đa dạng gen)
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

    // Lưu/Tải File JSON
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
                data.mem.pveBrain = fileData.dnaList[0]; 
                if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                    Debug.Log($"<color=green>Đã nạp thành công bộ não Siêu Trí Tuệ (Hậu giá: {data.mem.pveBrain.weights[3]}) vào Bot!</color>");
            }
        } else {
            if (GATrainer.instance == null || !GATrainer.instance.isTraining)
                Debug.LogWarning("Chưa có file BestBotBrain.json! Hãy bật Is Training để train trước.");
        }
    }
}