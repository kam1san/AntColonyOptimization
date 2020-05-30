using System;
using System.Collections.Generic;
using System.Text;

namespace AntColony
{
    class Solver
    {
        int n { get; set; }
        int[,] k { get; set; }
        int[,] t { get; set; }
        int[] p { get; set; }
        int[] tt { get; set; }
        int[,] c { get; set; }
        int C { get; set; }
        int L { get; set; }

        public Solver(int n_, int[,]k_, int[,] t_, int[] p_, int[] tt_, int[,] c_, int C_, int L_)
        {
            n = n_; k = k_;t = t_;p = p_;tt = tt_;c = c_;C = C_;L = L_;
        }

        public void GreedySolv()
        {
            c = this.sort(c, n);
            for (int i = 0; i < n; i++)
            {
                List<int> Way = new List<int>();
                List<int> Ban = new List<int>();
                int total_cost = 0;
                int total_time = 0;
                int total_value = 0;
                int point = c[0, i] - 1;
                int nextpoint = -1;
                while (nextpoint != c[0, i] - 1)
                {
                    double sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (k[point, j] == 0 || Ban.Contains(j))
                            continue;
                        else
                            sum += 1 / (double)((p[point] + k[point, j]) * (tt[point] + t[point, j]));
                    }
                    double max = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (k[point, j] == 0 || Ban.Contains(j))
                            continue;
                        else
                        {
                            if (max < ((1 / (double)((p[point] + k[point, j]) * (tt[point] + t[point, j]))) / sum))
                            {
                                max = ((1 / (double)((p[point] + k[point, j]) * (tt[point] + t[point, j]))) / sum);
                                nextpoint = j;
                            }
                        }
                    }
                    if (point != c[0, i] - 1)
                        Ban.Add(point);
                    Way.Add(point);
                    total_cost += p[point] + k[point, nextpoint];
                    total_time += tt[point] + t[point, nextpoint];
                    for (int k = 0; k < n; k++)
                        if (c[0, k] == point + 1)
                            total_value += c[1, k];
                    point = nextpoint;
                }
                Console.WriteLine("From point {0}:", c[0, i]);
                for (int j = 0; j < Way.Count; j++)
                {
                    Console.Write(Way[j] + 1 + " ---> ");
                }
                Console.Write(Way[0]+1+"\n");
                if (total_cost < C || total_cost == C)
                    Console.WriteLine("Wasted money: {0} <= {1}", total_cost, C);
                else
                    Console.WriteLine("Wasted money: {0} > {1}", total_cost, C);
                if (total_time < L || total_time == L)
                    Console.WriteLine("Wasted time: {0} <= {1}", total_time, L);
                else
                    Console.WriteLine("Wasted time: {0} > {1}", total_time, L);
                Console.WriteLine("Total value: {0}", total_value);
                Console.WriteLine("-------------------------------------\n");
            }
        }

        public int[,] sort(int[,] c, int n)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n - 1; j++)
                    if (c[1, j] < c[1, j + 1])
                    {
                        var temp = c[1, j];
                        c[1, j] = c[1, j + 1];
                        c[1, j + 1] = temp;
                        var temp1 = c[0, j];
                        c[0, j] = c[0, j + 1];
                        c[0, j + 1] = temp1;
                    }
            return c;
        }
    }
}
