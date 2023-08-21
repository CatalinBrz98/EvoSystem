using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManagerFirst : MonoBehaviour
{
    [SerializeField]
    private GameObject slime, meal;
    [SerializeField]
    private int initSlimesCount, initMealsCount;
    private int slimesCount, mealsCount;
    private float PHI = (Mathf.Sqrt(5) + 1) / 2;
    private List<GameObject> creatures = new List<GameObject>(), meals = new List<GameObject>();
    private Dictionary<GameObject, GameObject> mealTargets = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, List<GameObject>> mealTargetsReverse = new Dictionary<GameObject, List<GameObject>>();
    private string step = "Init";
    // Start is called before the first frame update
    void Start()
    {
        step = "Starting";
        slimesCount = initSlimesCount;
        mealsCount = initMealsCount;
        StartSimulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (step == "FirstStep")
        {
            bool allArrived = true;
            for (int i = 0; i < slimesCount && allArrived; i++)
                if (creatures[i].GetComponent<CharacterMovement>().GetAction() == "GoToObject")
                    allArrived = false;
            if (allArrived)
            {
                CalculateGeneration();
                ResetSimulation();
                StartSimulation();
                Debug.Log(slimesCount);
            }
        }
    }

    void StartSimulation()
    {
        float radianStep = Mathf.PI * 2 / slimesCount;
        for (int i = 0; i < slimesCount; i++)
        {
            float currRadian = radianStep * i;
            float oppositeDegree = -currRadian * Mathf.Rad2Deg - 90;
            float x = Mathf.Cos(currRadian) * 26f;
            float y = Mathf.Sin(currRadian) * 26f;
            Vector3 slimePos = new Vector3(x, 0.1f, y);
            Quaternion slimeRot = Quaternion.identity * Quaternion.Euler(Vector3.up * oppositeDegree);
            creatures.Add(Instantiate(slime, slimePos, slimeRot, parent: this.transform.Find("Population")));
        }

        Vector3[] points = Sunflower(mealsCount, alpha: 2);
        for (int k = 0; k < mealsCount; k++)
        {
            meals.Add(Instantiate(meal, points[k], Quaternion.identity, parent: this.transform.Find("Assets")));
        }

        for (int i = 0; i < mealsCount; i++)
        {
            mealTargetsReverse.Add(meals[i], new List<GameObject>());
        }

        int[] mealArrangement = Arrangement(slimesCount, mealsCount, rep: 2);
        for (int i = 0; i < slimesCount; i++)
        {
            mealTargets.Add(creatures[i], meals[mealArrangement[i]]);
            mealTargetsReverse[meals[mealArrangement[i]]].Add(creatures[i]);
        }

        foreach (KeyValuePair<GameObject, GameObject> mealTarget in mealTargets)
        {
            mealTarget.Key.GetComponent<CharacterMovement>().SetAction(newAction: "GoToObject", newObject: mealTarget.Value);
        }

        step = "FirstStep";
    }

    void CalculateGeneration()
    {
        int newSlimesCount = 0;

        foreach (GameObject meal in meals)
        {
            switch (mealTargetsReverse[meal].Count)
            {
                case 1:
                    newSlimesCount += RandomEvent(1f);
                    newSlimesCount += RandomEvent(1f);
                    break;
                case 2:
                    newSlimesCount += RandomEvent(0.5f);
                    newSlimesCount += RandomEvent(0.5f);
                    newSlimesCount += RandomEvent(0.5f);
                    newSlimesCount += RandomEvent(0.5f);
                    break;
                default:
                    break;
            }
        }

        slimesCount = newSlimesCount;
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
}
