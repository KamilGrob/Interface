using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FunCounter
{
    private static int counter;
    public static void add()
    {
        counter++;
    }
    public static void reset()
    {
        counter = 0;
    }
    public static int getCounter() { return counter; }
}

public class SphereFunction
{
    public static double Evaluate(double[] x)
    {

        double sum = 0;
        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * x[i];
        }
        FunCounter.add();
        return sum;

    }
}

public class RastriginFunction
{
    public static double Evaluate(double[] x)
    {
        

        double sum = 0;
        foreach (var xi in x)
        {
            sum += (xi - 1) * (xi - 1) - 10 * Math.Cos(2 * Math.PI * (xi - 1));
        }

        FunCounter.add();
        return 10 * x.Length + sum;
    }
}


public class RosenbrockFunction
{
    public static double Evaluate(double[] x)
    {
        
        double sum = 0;
        for (int i = 0; i < x.Length - 1; i++)
        {
            sum += 100 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(1 - x[i], 2);
        }
        FunCounter.add();
        return sum;
    }
}


