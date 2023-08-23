using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManagerSecond : MonoBehaviour
{
    [SerializeField]
    private GameObject slime, turtle, meal;
    [SerializeField]
    private int initSlimesCount, initTurtlesCount, initMealsCount;
    [SerializeField]
    private float mealNutrition = 2f;
    [SerializeField, Range(0f, 1f)]
    private float turtleForcePercentage = 0.75f, turtleLossPercentage = 1f;
    public enum ResourceShareModes
    {
        Greedy,
        Altruistic
    }
    [SerializeField]
    private ResourceShareModes resourceShareMode = ResourceShareModes.Altruistic;
    [SerializeField]
    private bool ultraFastSimulation = false;

    private int slimesCount, turtlesCount, creaturesCount, mealsCount;
    private float PHI = (Mathf.Sqrt(5) + 1) / 2;
    private List<GameObject> creatures = new List<GameObject>(), meals = new List<GameObject>();
    private Dictionary<GameObject, GameObject> mealTargets = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, List<GameObject>> mealTargetsReverse = new Dictionary<GameObject, List<GameObject>>();
    private int totalCreatures = 0, totalMeals = 0, generationCount = 0;
    private float totalSlimePercentage = 0f;
    private string step = "Init";
    // Start is called before the first frame update
    void Start()
    {
        step = "Starting";
        slimesCount = initSlimesCount;
        turtlesCount = initTurtlesCount;
        mealsCount = initMealsCount;
        StartSimulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (step == "FirstStep")
        {
            bool allArrived = true;
            for (int i = 0; i < creaturesCount && allArrived; i++)
                if (creatures[i].GetComponent<CharacterMovement>().GetAction() != "Staying")
                    allArrived = false;
            if (allArrived)
            {
                CalculateGeneration();
                ResetSimulation();
                StartSimulation();
            }
        }
    }

    void StartSimulation()
    {
        creaturesCount = slimesCount + turtlesCount;
        float radianStep = Mathf.PI * 2 / creaturesCount;
        for (int i = 0; i < creaturesCount; i++)
        {
            float currRadian = radianStep * i;
            float oppositeDegree = -currRadian * Mathf.Rad2Deg - 90;
            float x = Mathf.Cos(currRadian) * 26f;
            float y = Mathf.Sin(currRadian) * 26f;
            Vector3 slimePos = new Vector3(x, 0.1f, y);
            Quaternion slimeRot = Quaternion.identity * Quaternion.Euler(Vector3.up * oppositeDegree);
            if (i < slimesCount)
                creatures.Add(Instantiate(slime, slimePos, slimeRot, parent: this.transform.Find("Population")));
            else
                creatures.Add(Instantiate(turtle, slimePos, slimeRot, parent: this.transform.Find("Population")));
        }

        Vector3[] points = Sunflower(mealsCount, alpha: 2);
        for (int k = 0; k < mealsCount; k++)
        {
            meals.Add(Instantiate(meal, points[k], Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent: this.transform.Find("Assets")));
        }

        for (int i = 0; i < mealsCount; i++)
        {
            mealTargetsReverse.Add(meals[i], new List<GameObject>());
        }

        int[] mealArrangement = Arrangement(creaturesCount, mealsCount, rep: 2);
        for (int i = 0; i < creaturesCount; i++)
        {
            mealTargets.Add(creatures[i], meals[mealArrangement[i]]);
            mealTargetsReverse[meals[mealArrangement[i]]].Add(creatures[i]);
        }

        List<GameObject> oldMeals = new List<GameObject>();
        foreach (KeyValuePair<GameObject, GameObject> mealTarget in mealTargets)
        {
            string newAction = "GoToPosition";
            if (ultraFastSimulation)
                newAction = "TeleportToPosition";
            if (!oldMeals.Contains(mealTarget.Value))
            {
                mealTarget.Key.GetComponent<CharacterMovement>().SetAction(newAction: newAction, newObject: mealTarget.Value, newPosition: mealTarget.Value.transform.Find("FirstPlace").position);
                oldMeals.Add(mealTarget.Value);
            }
            else
                mealTarget.Key.GetComponent<CharacterMovement>().SetAction(newAction: newAction, newObject: mealTarget.Value, newPosition: mealTarget.Value.transform.Find("SecondPlace").position);
        }
        totalMeals += oldMeals.Count;


        step = "FirstStep";
    }

    void CalculateGeneration()
    {
        int newSlimesCount = 0;
        int newTurtlesCount = 0;

        foreach (GameObject meal in meals)
        {
            switch (mealTargetsReverse[meal].Count)
            {
                case 1:
                    if (mealTargetsReverse[meal][0].GetComponent<CharacterMovement>().creatureType == CharacterMovement.CreatureTypes.Slime)
                        newSlimesCount += ShareResources(mealNutrition);
                    else
                        newTurtlesCount += ShareResources(mealNutrition);
                    break;
                case 2:
                    var firstCreatureType = mealTargetsReverse[meal][0].GetComponent<CharacterMovement>().creatureType;
                    var secondCreatureType = mealTargetsReverse[meal][1].GetComponent<CharacterMovement>().creatureType;
                    if (firstCreatureType != secondCreatureType)
                    {
                        newSlimesCount += ShareResources(mealNutrition * (1f - turtleForcePercentage));
                        newTurtlesCount += ShareResources(mealNutrition * turtleForcePercentage);
                    }
                    else
                    {
                        if (firstCreatureType == CharacterMovement.CreatureTypes.Slime)
                        {
                            newSlimesCount += ShareResources(mealNutrition / 2);
                            newSlimesCount += ShareResources(mealNutrition / 2);
                        }
                        else
                        {
                            newTurtlesCount += ShareResources(mealNutrition * (1 - turtleLossPercentage) / 2);
                            newTurtlesCount += ShareResources(mealNutrition * (1 - turtleLossPercentage) / 2);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        slimesCount = newSlimesCount;
        turtlesCount = newTurtlesCount;

        generationCount += 1;
        totalCreatures += creaturesCount;
        totalSlimePercentage += ((float) slimesCount) / creaturesCount * 100;
        Debug.Log(string.Format("Current creatures: {0}, Average creatures: {1}, Average meals: {2}, Average slimes percentage: {3}", creaturesCount,
            ((float)totalCreatures) / generationCount, ((float)totalMeals) / generationCount, totalSlimePercentage / generationCount));
    }

    void ResetSimulation()
    {
        foreach (GameObject meal in meals)
        {
            mealTargetsReverse[meal].Clear();
            Destroy(meal);
        }

        foreach (GameObject creature in creatures)
        {
            Destroy(creature);
        }

        mealTargetsReverse.Clear();
        mealTargets.Clear();
        creatures.Clear();
        meals.Clear();

        step = "Reset";
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

    int ShareResources(float nutrition)
    {
        int currCount = 0;
        switch (resourceShareMode)
        {
            case ResourceShareModes.Altruistic:
                if (nutrition <= 1f)
                {
                    currCount += RandomEvent(nutrition / 2);
                    currCount += RandomEvent(nutrition / 2);
                }
                else
                {
                    int tries = Mathf.CeilToInt(nutrition);
                    for (int i = 0; i < tries; i++)
                        currCount += RandomEvent(nutrition / tries);
                }
                break;
            case ResourceShareModes.Greedy:
                if (nutrition <= 1f)
                    currCount += RandomEvent(nutrition);
                else
                {
                    currCount += 1;
                    nutrition -= 1f;
                    int tries = Mathf.CeilToInt(nutrition);
                    for (int i = 0; i < tries; i++)
                        currCount += RandomEvent(nutrition / tries);
                }
                break;
            default:
                break;
        }
        return currCount;
    }
}
