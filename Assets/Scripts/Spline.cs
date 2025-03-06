using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Spline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}



public class ParametricSpline
{
    private CubicSpline splineX;
    private CubicSpline splineY;

    public ParametricSpline(double[] xs, double[] ys)
    {
        int n = xs.Length;
        double[] t = new double[n];
        t[0] = 0.0;

        // Calculate cumulative distance for parameter t
        for (int i = 1; i < n; i++)
        {
            t[i] = t[i - 1] + Math.Sqrt((xs[i] - xs[i - 1]) * (xs[i] - xs[i - 1]) + (ys[i] - ys[i - 1]) * (ys[i] - ys[i - 1]));
        }

        // Normalize t to the range [0, 1]
        double tMax = t[n - 1];
        for (int i = 0; i < n; i++)
        {
            t[i] /= tMax;
        }

        splineX = new CubicSpline(t, xs);
        splineY = new CubicSpline(t, ys);
    }

    public (double, double) Evaluate(double t)
    {
        double x = splineX.Evaluate(t);
        double y = splineY.Evaluate(t);
        return (x, y);
    }
}

public class CubicSpline
{
    private readonly double[] a;
    private readonly double[] b;
    private readonly double[] c;
    private readonly double[] d;
    private readonly double[] x;
    private readonly int n;

    public CubicSpline(double[] xs, double[] ys)
    {
        if (xs.Length != ys.Length || xs.Length < 2)
            throw new ArgumentException("There must be at least two points, and arrays must have the same length.");

        n = xs.Length - 1;
        x = xs;
        a = new double[n + 1];
        b = new double[n];
        c = new double[n + 1];
        d = new double[n];

        double[] h = new double[n];
        double[] alpha = new double[n];
        double[] l = new double[n + 1];
        double[] mu = new double[n];
        double[] z = new double[n + 1];

        for (int i = 0; i < n; i++)
        {
            h[i] = xs[i + 1] - xs[i];
            a[i] = ys[i];
        }
        a[n] = ys[n];

        for (int i = 1; i < n; i++)
        {
            alpha[i] = (3.0 / h[i]) * (ys[i + 1] - ys[i]) - (3.0 / h[i - 1]) * (ys[i] - ys[i - 1]);
        }

        l[0] = 1.0;
        mu[0] = 0.0;
        z[0] = 0.0;

        for (int i = 1; i < n; i++)
        {
            l[i] = 2.0 * (xs[i + 1] - xs[i - 1]) - h[i - 1] * mu[i - 1];
            mu[i] = h[i] / l[i];
            z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }

        l[n] = 1.0;
        z[n] = 0.0;
        c[n] = 0.0;

        for (int j = n - 1; j >= 0; j--)
        {
            c[j] = z[j] - mu[j] * c[j + 1];
            b[j] = (ys[j + 1] - ys[j]) / h[j] - h[j] * (c[j + 1] + 2.0 * c[j]) / 3.0;
            d[j] = (c[j + 1] - c[j]) / (3.0 * h[j]);
        }
    }

    public double Evaluate(double xi)
    {
        int i = FindSegment(xi);
        double dx = xi - x[i];
        return a[i] + b[i] * dx + c[i] * dx * dx + d[i] * dx * dx * dx;
    }

    private int FindSegment(double xi)
    {
        int low = 0;
        int high = n;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (xi < x[mid])
                high = mid - 1;
            else if (xi > x[mid + 1])
                low = mid + 1;
            else
                return mid;
        }
        throw new ArgumentException("The value is out of the spline range.");
    }

}


