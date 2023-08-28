using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManagerThird : MonoBehaviour
{
    [SerializeField]
    private GameObject altruistic, deceptive, imitator, vindictive, crazy, table, cooperateSphere, preventSphere, deceiveSphere, betrayedSphere;
    [SerializeField]
    private int altruisticPairs, deceptivePairs, imitatorPairs, vindictivePairs, crazyPairs, microRounds = 10, fullRounds = 5;
    [SerializeField]
    private float microRoundBenefit = 1f;
    [SerializeField, Range(-5f, 5f)]
    private float cooperateReturn = 1f, preventReturn = 0f, deceiveReturn = 2f, betrayedReturn = -1f;
    [SerializeField]
    private bool ultraFastSimulation = false;

    private int altruisticsCount, deceptivesCount, imitatorsCount, vindictivesCount, craziesCount, tablesCount, individualsCount, currentFullRound;
    private float PHI = (Mathf.Sqrt(5) + 1) / 2;
    private List<GameObject> individuals = new List<GameObject>(), tables = new List<GameObject>();
    private Dictionary<GameObject, GameObject> tableTargets = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, List<GameObject>> tableTargetsReverse = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, List<bool>> individualsChoices = new Dictionary<GameObject, List<bool>>();
    private Dictionary<GameObject, float> individualsBenefits = new Dictionary<GameObject, float>();
    private string step = "Init";
    // Start is called before the first frame update
    void Start()
    {
        step = "Starting";
        altruisticsCount = altruisticPairs * 2;
        deceptivesCount = deceptivePairs * 2;
        imitatorsCount = imitatorPairs * 2;
        vindictivesCount = vindictivePairs * 2;
        craziesCount = crazyPairs * 2;
        tablesCount = altruisticPairs + deceptivePairs + imitatorPairs + vindictivePairs + crazyPairs;
        StartSimulation();
    }

    // Update is called once per frame
    void Update()
    {
        switch (step)
        {
            case "MoveStep":
                MoveStep();
                break;
            case "ProcessStep":
                bool allArrived = true;
                for (int i = 0; i < individualsCount && allArrived; i++)
                    if (individuals[i].GetComponent<CharacterMovement>().GetAction() != "Staying")
                        allArrived = false;
                if (allArrived)
                    ProcessStep();
                break;
            case "CalculateGeneration":
                CalculateGeneration();
                break;
            case "ResetSimulation":
                ResetSimulation();
                StartSimulation();
                break;
            default:
                break;
        }
    }

    void StartSimulation()
    {
        individualsCount = altruisticsCount + deceptivesCount + imitatorsCount + vindictivesCount + craziesCount;
        float radianStep = Mathf.PI * 2 / individualsCount;
        for (int i = 0; i < individualsCount; i++)
        {
            float currRadian = radianStep * i;
            float oppositeDegree = -currRadian * Mathf.Rad2Deg - 90;
            float x = Mathf.Cos(currRadian) * 26f;
            float y = Mathf.Sin(currRadian) * 26f;
            Vector3 individualPos = new Vector3(x, 0.1f, y);
            Quaternion individualRot = Quaternion.identity * Quaternion.Euler(Vector3.up * oppositeDegree);
            if (i < altruisticsCount)
                individuals.Add(Instantiate(altruistic, individualPos, individualRot, parent: this.transform.Find("Population")));
            else if (i < altruisticsCount + deceptivesCount)
                individuals.Add(Instantiate(deceptive, individualPos, individualRot, parent: this.transform.Find("Population")));
            else if (i < altruisticsCount + deceptivesCount + imitatorsCount)
                individuals.Add(Instantiate(imitator, individualPos, individualRot, parent: this.transform.Find("Population")));
            else if (i < altruisticsCount + deceptivesCount + imitatorsCount + vindictivesCount)
                individuals.Add(Instantiate(vindictive, individualPos, individualRot, parent: this.transform.Find("Population")));
            else
                individuals.Add(Instantiate(crazy, individualPos, individualRot, parent: this.transform.Find("Population")));
            individualsChoices.Add(individuals[i], new List<bool>());
            individualsBenefits.Add(individuals[i], 0f);
        }

        Vector3[] points = Sunflower(tablesCount, alpha: 2);
        for (int k = 0; k < tablesCount; k++)
        {
            tables.Add(Instantiate(table, points[k], Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent: this.transform.Find("Assets")));
        }

        for (int i = 0; i < tablesCount; i++)
        {
            tableTargetsReverse.Add(tables[i], new List<GameObject>());
        }

        currentFullRound = 0;

        step = "MoveStep";
        MoveStep();
    }

    void MoveStep()
    {
        int[] tableArrangement = Arrangement(individualsCount, tablesCount, rep: 2);
        for (int i = 0; i < individualsCount; i++)
        {
            tableTargets.Add(individuals[i], tables[tableArrangement[i]]);
            tableTargetsReverse[tables[tableArrangement[i]]].Add(individuals[i]);
        }

        List<GameObject> oldTables = new List<GameObject>();
        foreach (KeyValuePair<GameObject, GameObject> tableTarget in tableTargets)
        {
            string newAction = "GoToPosition";
            if (ultraFastSimulation)
                newAction = "TeleportToPosition";
            if (!oldTables.Contains(tableTarget.Value))
            {
                tableTarget.Key.GetComponent<CharacterMovement>().SetAction(newAction: newAction, newObject: tableTarget.Value, newPosition: tableTarget.Value.transform.Find("FirstPlace").position);
                oldTables.Add(tableTarget.Value);
            }
            else
                tableTarget.Key.GetComponent<CharacterMovement>().SetAction(newAction: newAction, newObject: tableTarget.Value, newPosition: tableTarget.Value.transform.Find("SecondPlace").position);
        }

        step = "ProcessStep";
    }

    void ProcessStep()
    {
        for (int microRound = 0; microRound < microRounds; microRound++)
        {
            foreach (GameObject table in tables)
            {
                GameObject firstIndividual = tableTargetsReverse[table][0], secondIndividual = tableTargetsReverse[table][1];

                individualsChoices[firstIndividual].Add(MakeChoice(firstIndividual, individualsChoices[secondIndividual], microRound));
                individualsChoices[secondIndividual].Add(MakeChoice(secondIndividual, individualsChoices[firstIndividual], microRound));
                float currRadian = -Mathf.PI / 2;
                if (microRounds > 1)
                {
                    float radianStep = -Mathf.PI / (microRounds - 1);
                    currRadian = radianStep * microRound;
                }

                if (individualsChoices[firstIndividual][microRound] && individualsChoices[secondIndividual][microRound])
                {
                    AddSphere(firstIndividual, cooperateSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[firstIndividual] += microRoundBenefit * cooperateReturn;

                    AddSphere(secondIndividual, cooperateSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[secondIndividual] += microRoundBenefit * cooperateReturn;
                }
                else if (individualsChoices[firstIndividual][microRound] && !individualsChoices[secondIndividual][microRound])
                {
                    AddSphere(firstIndividual, betrayedSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[firstIndividual] += microRoundBenefit * betrayedReturn;

                    AddSphere(secondIndividual, deceiveSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[secondIndividual] += microRoundBenefit * deceiveReturn;
                }
                else if (!individualsChoices[firstIndividual][microRound] && individualsChoices[secondIndividual][microRound])
                {
                    AddSphere(firstIndividual, deceiveSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[firstIndividual] += microRoundBenefit * deceiveReturn;

                    AddSphere(secondIndividual, betrayedSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[secondIndividual] += microRoundBenefit * betrayedReturn;
                }
                else if (!individualsChoices[firstIndividual][microRound] && !individualsChoices[secondIndividual][microRound])
                {
                    AddSphere(firstIndividual, preventSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[firstIndividual] += microRoundBenefit * preventReturn;

                    AddSphere(secondIndividual, preventSphere, currRadian, (currentFullRound + 1) * 0.15f);
                    individualsBenefits[secondIndividual] += microRoundBenefit * preventReturn;
                }
            }
        }

        foreach (GameObject table in tables)
            tableTargetsReverse[table].Clear();
        foreach (GameObject individual in individuals)
            individualsChoices[individual].Clear();
        tableTargets.Clear();

        currentFullRound += 1;
        if (currentFullRound < fullRounds)
            step = "MoveStep";
        else
            step = "CalculateGeneration";
    }

    void CalculateGeneration()
    {
        int newAltruisticsCount = 0, newDeceptivesCount = 0, newImitatorsCount = 0, newVindictivesCount = 0, newCraziesCount = 0;
        float altruisticsPercentage = 0f, deceptivesPercentage = 0f, imitatorsPercentage = 0f, vindictivesPercentage = 0f, craziesPercentage = 0f;

        float[] scoresNormalized = new float[individualsCount];
        for (int i = 0; i < individualsCount; i++)
            scoresNormalized[i] = individualsBenefits[individuals[i]];
        Normalize(scoresNormalized);
        for (int i = 0; i < individualsCount; i++)
        {
            var individual = individuals[i];
            var individualType = individual.GetComponent<CharacterMovement>().creatureType;
            switch (individualType)
            {
                case CharacterMovement.CreatureTypes.SlimePink:
                    altruisticsPercentage += individualsBenefits[individuals[i]];
                    break;
                case CharacterMovement.CreatureTypes.SlimeBlack:
                    deceptivesPercentage += individualsBenefits[individuals[i]];
                    break;
                case CharacterMovement.CreatureTypes.SlimeBlue:
                    imitatorsPercentage += individualsBenefits[individuals[i]];
                    break;
                case CharacterMovement.CreatureTypes.SlimeGreen:
                    vindictivesPercentage += individualsBenefits[individuals[i]];
                    break;
                case CharacterMovement.CreatureTypes.Slime:
                    craziesPercentage += individualsBenefits[individuals[i]];
                    break;
                default:
                    break;
            }
        }


        for (int i = 0; i < individualsCount; i++)
        {
            var individual = individuals[RandomEventFromNormalized(scoresNormalized)];
            var individualType = individual.GetComponent<CharacterMovement>().creatureType;
            switch (individualType)
            {
                case CharacterMovement.CreatureTypes.SlimePink:
                    newAltruisticsCount += 1;
                    break;
                case CharacterMovement.CreatureTypes.SlimeBlack:
                    newDeceptivesCount += 1;
                    break;
                case CharacterMovement.CreatureTypes.SlimeBlue:
                    newImitatorsCount += 1;
                    break;
                case CharacterMovement.CreatureTypes.SlimeGreen:
                    newVindictivesCount += 1;
                    break;
                case CharacterMovement.CreatureTypes.Slime:
                    newCraziesCount += 1;
                    break;
                default:
                    break;
            }
        }

        Debug.Log(string.Format("Altruistic: {0} - {5}, Deceptive: {1} - {6}, Imitator: {2} - {7}, Vindictive: {3} - {8}, Crazy: {4} - {9}", altruisticsCount, deceptivesCount,
            imitatorsCount, vindictivesCount, craziesCount, altruisticsPercentage, deceptivesPercentage, imitatorsPercentage, vindictivesPercentage, craziesPercentage));

        altruisticsCount = newAltruisticsCount;
        deceptivesCount = newDeceptivesCount;
        imitatorsCount = newImitatorsCount;
        vindictivesCount = newVindictivesCount;
        craziesCount = newCraziesCount;
        step = "ResetSimulation";
    }

    void ResetSimulation()
    {
        foreach (GameObject table in tables)
        {
            tableTargetsReverse[table].Clear();
            Destroy(table);
        }

        foreach (GameObject individual in individuals)
        {
            individualsChoices[individual].Clear();
            Destroy(individual);
        }

        individualsBenefits.Clear();
        individualsChoices.Clear();
        tableTargetsReverse.Clear();
        tableTargets.Clear();
        individuals.Clear();
        tables.Clear();

        step = "StartSimulation";
    }

    Vector3[] Sunflower(int n, int alpha = 0)
    {
        Vector3[] points = new Vector3[n];
        float angle_stride = 2 * Mathf.PI / (PHI * PHI);
        int b = Mathf.RoundToInt(alpha * Mathf.Sqrt(n));
        for (int k = 1; k <= n; k++)
        {
            float r = Radius(k, n, b);
            float theta = k * angle_stride;
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);
            points[k - 1] = new Vector3(x, 0.1f, y);
        }
        return points;
    }

    float Radius(int k, int n, int b)
    {
        if (k > n - b)
            return 23.5f;
        return Mathf.Sqrt(k - 0.5f) / Mathf.Sqrt(n - (b + 1) / 2) * 23.5f;
    }

    int[] Arrangement(int k, int n, int rep = 1)
    {
        n *= rep;
        int[] v = new int[n], ret = new int[k];
        for (int i = 0; i < n; i++)
        {
            v[i] = i;
        }

        for (int i = 0; i < k && i < n; i++)
        {
            int rand = Random.Range(0, n - i), aux = v[rand];
            v[rand] = v[n - i - 1];
            v[n - i - 1] = aux;
            ret[i] = aux / rep;
        }

        return ret;
    }

    int RandomEvent(float chance)
    {
        if (Random.value <= chance)
            return 1;
        return 0;
    }

    bool RandomBool(float chance = 0.5f)
    {
        if (Random.value <= chance)
            return true;
        return false;
    }

    bool MakeChoice(GameObject individual, List<bool> oldChoices, int microRound)
    {
        var individualType = individual.GetComponent<CharacterMovement>().creatureType;

        if (microRound == 0)
        {
            switch (individualType)
            {
                case CharacterMovement.CreatureTypes.SlimePink:
                    return true;
                case CharacterMovement.CreatureTypes.SlimeBlack:
                    return false;
                case CharacterMovement.CreatureTypes.SlimeBlue:
                    return true;
                case CharacterMovement.CreatureTypes.SlimeGreen:
                    return true;
                case CharacterMovement.CreatureTypes.Slime:
                    return RandomBool();
                default:
                    return RandomBool();
            }
        }
        else
        {
            switch (individualType)
            {
                case CharacterMovement.CreatureTypes.SlimePink:
                    return true;
                case CharacterMovement.CreatureTypes.SlimeBlack:
                    return false;
                case CharacterMovement.CreatureTypes.SlimeBlue:
                    return oldChoices[microRound - 1];
                case CharacterMovement.CreatureTypes.SlimeGreen:
                    for (int r = 0; r < microRound; r++)
                        if (!oldChoices[r])
                            return false;
                    return true;
                case CharacterMovement.CreatureTypes.Slime:
                    return RandomBool();
                default:
                    return RandomBool();
            }
        }
    }

    GameObject AddSphere(GameObject individual, GameObject sphere, float radian, float height = 0.15f)
    {
        float x = Mathf.Cos(radian) * 0.8f;
        float y = Mathf.Sin(radian) * 0.8f;
        Vector3 spherePos = new Vector3(x, height, y);
        GameObject newSphere = Instantiate(sphere, individual.transform.position, Quaternion.identity, parent: individual.transform);
        newSphere.transform.localPosition = spherePos;
        return newSphere;
    }

    void Normalize(float[] array, float tMin = 0f, float tMax = 1f, bool returnProbabilities = true)
    {
        float minScore = Mathf.Min(array);
        float maxScore = Mathf.Max(array);
        float diffScore = maxScore - minScore;
        float tDiff = tMax - tMin;
        float arraySum = 0f;
        int n = array.Length;

        for (int i = 0; i < n; i++)
        {
            if (diffScore != 0f)
                array[i] = ((array[i] - minScore) * tDiff) / diffScore + tMin;
            else
                array[i] = tMax;
            arraySum += array[i];
        }

        if (returnProbabilities)
            for (int i = 0; i < n; i++)
                array[i] /= arraySum;
    }

    int RandomEventFromNormalized(float[] percentages)
    {
        float randomValue = Random.value, cumulativePercent = 0f;
        for (int i = 0; i < percentages.Length; i++)
        {
            if (cumulativePercent + percentages[i] > randomValue)
                return i;
            cumulativePercent += percentages[i];
        }
        return percentages.Length - 1;
    }
}
