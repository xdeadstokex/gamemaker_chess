[System.Serializable]
/*
Tốt: 100
- Quân thường: Xe, mã, tượng, hậu: 0, 1, 2, 3
- Quân tiến hóa: Tốt (mã, tượng), tốt (xe), xe, mã, tượng, hậu: 4, 5, 6, 7, 8, 9
- Kiểm soát trung tâm: 10
- Phạt khi bị đe dọa: 11
*/

public class BotDNA
{
    public float[] weights = new float[12];
    public float fitness = 0;

    public BotDNA()
    {
        for(int i = 0; i < weights.Length; i++)
        {
            weights[i] = Random.Range(10f, 1000f);
        }
    }

    public BotDNA Crossover(BotDNA partner)
    {
        BotDNA child = new BotDNA();
        for(int i = 0; i < weights.Length; i++)
        {
            child.weights[i] = Random.value > 0.5 ? weights[i] : partner.weights[i]; //0-->1
        }
        return child;
    }

    public BotDNA Mutate(float mutationRate)
    {
        for(int i = 0; i < weights.Length; i++)
        {
            if(Random.value < mutationRate)
            {
                weights[i] += Random.Range(-50f, 50f);
                if(weights[i] < 0) weights[i] = 10f;
            }
        }
    }
}