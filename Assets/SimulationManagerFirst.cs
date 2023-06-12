using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManagerFirst : MonoBehaviour
{
    [SerializeField]
    private GameObject slime, meal;
    [SerializeField]
    private int slimeCount;
    private float PHI = (Mathf.Sqrt(5) + 1) / 2;
    // Start is called before the first frame update
    void Start()
    {
        StartSimulation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StartSimulation()
    {
        var radianStep = Mathf.PI * 2 / slimeCount;
        for (int i = 0; i < slimeCount; i++)
        {
            var currRadian = radianStep * i;
            var oppositeDegree = -currRadian * Mathf.Rad2Deg - 90;
            var x = Mathf.Cos(currRadian) * 26f;
            var y = Mathf.Sin(currRadian) * 26f;
            var slimePos = new Vector3(x, 2f, y);
            var slimeRot = Quaternion.identity * Quaternion.Euler(Vector3.up * oppositeDegree);
            Instantiate(slime, slimePos, slimeRot, parent: this.transform);
        }

        var points = Sunflower(100, alpha: 2);

        for (int k = 0; k < 100; k++)
        {
            Instantiate(meal, points[k], Quaternion.identity, parent: this.transform);
        }
    }

    Vector3[] Sunflower(int n, int alpha = 0)
    {
        var points = new Vector3[n];
        var angle_stride = 2 * Mathf.PI / (PHI * PHI);
        var b = Mathf.RoundToInt(alpha * Mathf.Sqrt(n));
        for (int k = 1; k <= n; k++)
        {
            var r = Radius(k, n, b);
            var theta = k * angle_stride;
            var x = r * Mathf.Cos(theta);
            var y = r * Mathf.Sin(theta);
            points[k - 1] = new Vector3(x, 2f, y);
        }
        return points;
    }

    float Radius(int k, int n, int b)
    {
        if (k > n - b)
            return 20.0f;
        return Mathf.Sqrt(k - 0.5f) / Mathf.Sqrt(n - (b + 1) / 2) * 20.0f;
    }
}
