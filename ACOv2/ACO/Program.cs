using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AntColony
{
    internal class AntColonyProgram
    {

        private static Random random = new Random(0);
        private static string folder = AppDomain.CurrentDomain.BaseDirectory;
        private static int alpha;
        private static int beta;
        private static double rho;
        private static double Q;
        private static int numCities;
        private static int numAnts;
        private static int maxTime;
        private static int Money;
        private static int Time;
        private static int[][] dists_cost;
        private static int[][] dists_time;
        private static int[] values;
        private static string СС_file_name;
        private static string СT_file_name;
        private static string DC_file_name;
        private static string DT_file_name;
        private static string V_file_name;

        public static void Main(string[] args)
        {
            Console.WriteLine("\nChoose algorithm:");
            Console.WriteLine("1. Greedy");
            Console.WriteLine("2. Ant colony");
            bool check = false;
            while (check == false)
            {
                int choice = Int32.Parse(Console.ReadLine());
                if (choice == 1)
                {
                    Greedy();
                    check = true;
                }
                else if (choice == 2)
                {
                    AntColony();
                    check = true;
                }
                else
                    Console.WriteLine("Please input valid option: {1, 2}");
            }
        }

        private static void Greedy()
        {
            Console.Clear();
            FilesInputGreedy();
            int n = numCities;
            int[,] k = FileInputDistancesGreedy(DC_file_name);
            int[,] t = FileInputDistancesGreedy(DT_file_name);
            int[] p = FileInputCitiesGreedy(СС_file_name);
            int[] tt = FileInputCitiesGreedy(СT_file_name);
            int[,] c = FileInputValuesGreedy(V_file_name);
            int C = Money;
            int L = Time;
            Solver solv = new Solver(n, k, t, p, tt, c, C, L);
            solv.GreedySolv();
            Console.ReadKey();
        }

        private static void FilesInputGreedy()
        {
            int counter = 0;
            string[] lines = new string[14];
            string line;
            StreamReader file = new StreamReader(folder+"Graph/Global2.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] lines2 = line.Split(' ');
                lines[counter] = lines2[0];
                counter++;
            }
            numCities = Int32.Parse(lines[4]);
            Money = Int32.Parse(lines[7]);
            Time = Int32.Parse(lines[8]);
            СС_file_name = lines[9];
            СT_file_name = lines[10];
            DC_file_name = lines[11];
            DT_file_name = lines[12];
            V_file_name = lines[13];
        }

        private static int[,] FileInputDistancesGreedy(string f)
        {
            int[,] k = new int[numCities,numCities];

            int counter = 0;
            string con = folder + "Graph/" + f;
            string line;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                string[] lines = line.Split(' ');
                int counter2 = 0;
                foreach (string l in lines)
                {
                    k[counter,counter2] = Int32.Parse(l);
                    counter2++;
                }
                counter++;
            }
            return k;
        }

        private static int[] FileInputCitiesGreedy(string f)
        {
            int[] k = new int[numCities];

            int counter = 0;
            string con = folder + "Graph/" + f;
            string line;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                k[counter] = Int32.Parse(line);
                counter++;
            }
            return k;
        }

        private static int[,] FileInputValuesGreedy(string f)
        {
            int[,] k = new int[2,numCities];

            int counter = 0;
            string con = folder + "Graph/" + f;
            string line;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                k[1,counter] = Int32.Parse(line);
                counter++;
                k[0, counter - 1] = counter;
            }
            return k;
        }

        private static void AntColony()
        {
            Console.Clear();
            try
            {
                Console.WriteLine("\nChoose input type:");
                Console.WriteLine("1. File input");
                Console.WriteLine("2. Random input");
                Console.WriteLine("3. Keyboard input");
                bool check = false;
                while (check == false)
                {
                    int choice = Int32.Parse(Console.ReadLine());
                    if (choice == 1)
                    {
                        FilesInput();
                        check = true;
                    }
                    else if (choice == 2)
                    {
                        RandomInput();
                        check = true;
                    }
                    else if (choice == 3)
                    {
                        KeyboardInput();
                        check = true;
                    }
                    else
                        Console.WriteLine("Please input valid option: {1, 2, 3}");
                }

                Console.WriteLine("Number cities in problem = " + numCities);

                Console.WriteLine("\nNumber ants = " + numAnts);
                Console.WriteLine("Maximum time = " + maxTime);
                Console.WriteLine("Amount of money = " + Money);
                Console.WriteLine("Amount of time = " + Time);

                Console.WriteLine("\nAlpha (pheromone influence) = " + alpha);
                Console.WriteLine("Beta (local node influence) = " + beta);
                Console.WriteLine("Rho (pheromone evaporation coefficient) = " + rho.ToString("F2"));
                Console.WriteLine("Q (pheromone deposit factor) = " + Q.ToString("F2"));

                Console.WriteLine("Final graph of costs:");
                for (int i = 0; i < numCities; i++)
                {
                    Console.Write("{0}: ", i + 1);
                    for (int j = 0; j < numCities; j++)
                    {
                        Console.Write("| " + dists_cost[i][j] + " |");
                    }
                    Console.Write("\n");
                }

                Console.WriteLine("\nFinal graph of time:");
                for (int i = 0; i < numCities; i++)
                {
                    Console.Write("{0}: ", i + 1);
                    for (int j = 0; j < numCities; j++)
                    {
                        Console.Write("| " + dists_time[i][j] + " |");
                    }
                    Console.Write("\n");
                }

                Console.WriteLine("\nFinal graph of values:");
                for (int i = 0; i < numCities; i++)
                {
                    Console.Write("{0}: ", i + 1);
                    Console.Write("| " + values[i] + " |\n");
                }

                Console.WriteLine("\nInitialing ants\n");
                int[][] ants = InitAnts(numAnts, numCities, dists_cost);

                double[] bestLength = new double[2];
                int[] bestTrail = new int[numCities];

                double bestValue = 0;

                Console.WriteLine("\nInitializing pheromones on trails");
                double[][] pheromones = InitPheromones(numCities, dists_cost);

                int time = 0;
                Console.WriteLine("\nEntering UpdateAnts - UpdatePheromones loop\n");
                while (time < maxTime)
                {
                    UpdateAnts(ants, pheromones, dists_cost, dists_time);
                    UpdatePheromones(pheromones, ants, dists_cost, dists_time);

                    int[] currBestTrail = AntColonyProgram.BestTrail(ants, dists_cost, dists_time, values);
                    double[] currBestLength = Length(currBestTrail, dists_cost, dists_time);
                    double currBestValue = Values(currBestTrail, values);
                    if ((bestValue < currBestValue && (currBestLength[0] <= Money && currBestLength[1] <= Time)))
                    {
                        bestLength = currBestLength;
                        bestValue = currBestValue;
                        bestTrail = currBestTrail;
                        Console.WriteLine("New best length of: COST: {0}  TIME: {1}  VALUE: {2}    found at time {3}", bestLength[0].ToString("F1"), bestLength[1].ToString("F1"), bestValue, time);
                        Display(bestTrail);
                    }
                    time++;
                }

                Console.WriteLine("\nTime complete");

                Console.WriteLine("\nBest trail found:");
                Display(bestTrail);
                Console.WriteLine("\nLength of best trail found: COST: {0}  TIME: {1}  VALUE: {2} ", bestLength[0].ToString("F1"), bestLength[1].ToString("F1"), bestValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("No solution for this graph...");
                Console.ReadLine();
            }
            Console.ReadKey();
        }

        // Main

        // --------------------------------------------------------------------------------------------

        private static void FilesInput()
        {
            int counter = 0;
            string[] lines = new string[14];
            string line;
            StreamReader file = new StreamReader(folder + "Graph/Global.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] lines2 = line.Split(' ');
                lines[counter] = lines2[0];
                counter++;
            }

            alpha = Int32.Parse(lines[0]);
            beta = Int32.Parse(lines[1]);
            rho = Double.Parse(lines[2], CultureInfo.InvariantCulture);
            Q = Double.Parse(lines[3], CultureInfo.InvariantCulture);
            numCities = Int32.Parse(lines[4]);
            numAnts = Int32.Parse(lines[5]);
            maxTime = Int32.Parse(lines[6]);
            Money = Int32.Parse(lines[7]);
            Time = Int32.Parse(lines[8]);
            СС_file_name = lines[9];
            СT_file_name = lines[10];
            DC_file_name = lines[11];
            DT_file_name = lines[12];
            V_file_name = lines[13];

            dists_cost = MakeGraphDistancesCost(numCities, СС_file_name, DC_file_name);
            dists_time = MakeGraphDistancesTime(numCities, СT_file_name, DT_file_name);
            values = MakeGraphValues(numCities, V_file_name);
        }



        private static void RandomInput()
        {
            Random rnd = new Random();

            alpha = rnd.Next(0,11);
            beta = rnd.Next(0, 11);
            rho = rnd.NextDouble() * (1 - 0.01) + 0.01;
            Q = rnd.NextDouble() * (10 - 1) + 1;
            numCities = rnd.Next(3, 6);
            numAnts = rnd.Next(1, 11);
            maxTime = rnd.Next(100, 1000);

            int[] p = new int[numCities];

            for (int i = 0; i < numCities; i++) 
            {
                p[i] = rnd.Next(0, 11);
            }

            int[][] k = new int[numCities][];
            for (int i = 0; i <= k.Length - 1; i++)
            {
                k[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    int rand = rnd.Next(0, 11);
                    k[i][j] = rand;
                    k[j][i] = rand;
                }
            }
            
            int[] tt = new int[numCities];

            for (int i = 0; i < numCities; i++)
            {
                tt[i] = rnd.Next(0, 11);
            }

            int[][] t = new int[numCities][];
            for (int i = 0; i <= t.Length - 1; i++)
            {
                t[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    if (k[i][j] != 0)
                    {
                        t[i][j] = rnd.Next(0, 11);
                        t[j][i] = t[i][j];
                    }
                    else
                    {
                        t[i][j] = 0;
                        t[j][i] = t[i][j];
                    }
                }
            }

            dists_cost = new int[numCities][];
            for (int i = 0; i <= dists_cost.Length - 1; i++)
            {
                dists_cost[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (k[i][j] == 0)
                        continue;
                    else
                        dists_cost[i][j] = p[i] + k[i][j];
                }
            }

            dists_time = new int[numCities][];
            for (int i = 0; i <= dists_time.Length - 1; i++)
            {
                dists_time[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (t[i][j] == 0)
                        continue;
                    else
                        dists_time[i][j] = tt[i] + t[i][j];
                }
            }

            values = new int[numCities];
            for (int i = 0; i < numCities; i++)
            {
                values[i] = rnd.Next(1, 11);
            }

            Money = rnd.Next(20, 101);
            Time = rnd.Next(20, 101);
        }

        private static void KeyboardInput()
        {
            Console.Write("\n alpha  = ");
            alpha = Int32.Parse(Console.ReadLine());
            Console.Write("\n beta = ");
            beta = Int32.Parse(Console.ReadLine());
            Console.Write("\n rho показник вивітрення феромону (рекомендоване значення 0.9) = ");
            rho = Double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
            Console.Write("\n Q покладання феромону = ");
            Q = Double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
            Console.Write("\n кількість міст = ");
            numCities = Int32.Parse(Console.ReadLine());
            Console.Write("\n кількість мурах = ");
            numAnts = Int32.Parse(Console.ReadLine());
            Console.Write("\n максимальна к-сть ітерацій = ");
            maxTime = Int32.Parse(Console.ReadLine());

            int[] p = new int[numCities];
            Console.Write("\n введіть кількості грошей для перебування в містах = ");
            for (int i = 0; i < numCities; i++)
            {
                p[i] = Int32.Parse(Console.ReadLine());
            }

            int[][] k = new int[numCities][];
            for (int i = 0; i <= k.Length - 1; i++)
            {
                k[i] = new int[numCities];
            }

            Console.Write("\n введіть кількості грошей для переїзду між містами = ");
            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    Console.WriteLine("\n переїзд між містами {0} та {1} ", i+1, j+1);
                    int val = Int32.Parse(Console.ReadLine());
                    k[i][j] = val;
                    k[j][i] = val;
                }
            }

            int[] tt = new int[numCities];

            Console.Write("\n введіть час перебування у кожному місті = ");
            for (int i = 0; i < numCities; i++)
            {
                tt[i] = Int32.Parse(Console.ReadLine());
            }

            int[][] t = new int[numCities][];
            for (int i = 0; i <= t.Length - 1; i++)
            {
                t[i] = new int[numCities];
            }

            Console.Write("\n введіть час переїзду між містами = ");
            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    if (k[i][j] != 0)
                    {
                        Console.WriteLine("\n переїзд між містами {0} та {1} ", i + 1, j + 1);
                        t[i][j] = Int32.Parse(Console.ReadLine());
                        t[j][i] = t[i][j];
                    }
                    else
                    {
                        t[i][j] = 0;
                        t[j][i] = t[i][j];
                    }
                }
            }

            dists_cost = new int[numCities][];
            for (int i = 0; i <= dists_cost.Length - 1; i++)
            {
                dists_cost[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (k[i][j] == 0)
                        continue;
                    else
                        dists_cost[i][j] = p[i] + k[i][j];
                }
            }

            dists_time = new int[numCities][];
            for (int i = 0; i <= dists_time.Length - 1; i++)
            {
                dists_time[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (t[i][j] == 0)
                        continue;
                    else
                        dists_time[i][j] = tt[i] + t[i][j];
                }
            }

            Console.Write("\n введіть значення цінностей для кожного міста = ");
            values = new int[numCities];
            for (int i = 0; i < numCities; i++)
            {
                values[i] = Int32.Parse(Console.ReadLine());
            }

            Console.Write("\n введіть максимальну к-сть грошей = ");
            Money = Int32.Parse(Console.ReadLine());
            Console.Write("\n введіть максимальну к-сть часу = ");
            Time = Int32.Parse(Console.ReadLine());
        }

        private static int[][] MakeGraphDistancesCost(int numCities, string f1, string f2)
        {
            int[] p = new int[numCities];

            int[][] k = new int[numCities][];
            for (int i = 0; i <= k.Length - 1; i++)
            {
                k[i] = new int[numCities];
            }

            int[][] costs = new int[numCities][];
            for (int i = 0; i <= costs.Length - 1; i++)
            {
                costs[i] = new int[numCities];
            }

            int counter = 0;
            string line;
            string con = folder + "Graph/" + f1;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                p[counter] = Int32.Parse(line);
                counter++;
            }

            counter = 0;
            con = folder + "Graph/" + f2;
            file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                string[] lines = line.Split(' ');
                int counter2 = 0;
                foreach (string l in lines)
                {
                    k[counter][counter2] = Int32.Parse(l);
                    counter2++;
                }
                counter++;
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (k[i][j] == 0)
                        continue;
                    else
                        costs[i][j] = p[i] + k[i][j];
                }
            }
            return costs;
        }

        private static int[][] MakeGraphDistancesTime(int numCities, string f1, string f2)
        {
            int[] tt = new int[numCities];

            int[][] t = new int[numCities][];
            for (int i = 0; i <= t.Length - 1; i++)
            {
                t[i] = new int[numCities];
            }

            int[][] times = new int[numCities][];
            for (int i = 0; i <= times.Length - 1; i++)
            {
                times[i] = new int[numCities];
            }

            int counter = 0;
            string line;
            string con = folder + "Graph/" + f1;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                tt[counter] = Int32.Parse(line);
                counter++;
            }

            counter = 0;
            con = folder + "Graph/" + f2;
            file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                string[] lines = line.Split(' ');
                int counter2 = 0;
                foreach (string l in lines)
                {
                    t[counter][counter2] = Int32.Parse(l);
                    counter2++;
                }
                counter++;
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = 0; j <= numCities - 1; j++)
                {
                    if (t[i][j] == 0)
                        continue;
                    else
                        times[i][j] = tt[i] + t[i][j];
                }
            }
            return times;
        }

        private static int[] MakeGraphValues(int numCities, string f)
        {
            int[] v = new int[numCities];

            int counter = 0;
            string line;
            string con = folder + "Graph/" + f;
            StreamReader file = new StreamReader(con);
            while ((line = file.ReadLine()) != null)
            {
                v[counter] = Int32.Parse(line);
                counter++;
            }
            return v;
        }

        private static int[][] InitAnts(int numAnts, int numCities, int[][] dists_cost)
        {
            int[][] ants = new int[numAnts][];
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                ants[i] = new int[numAnts];
            }
            return ants;
        }
        

        private static int IndexOfTarget(int[] trail, int target)
        {
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }

        private static double[] Length(int[] trail, int[][] dists_cost, int[][] dists_time)
        {
            double[] result = new double[2];
            int counter = 0;
            for (int i = 1; i <= trail.Length - 1; i++)
                if (trail[i] == trail[0])
                    counter++;

            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if(i == trail.Length - 1 && counter==0)
                {
                    double[] res = Distance(trail[i], trail[0], dists_cost, dists_time);
                    result[0] += res[0];
                    result[1] += res[1];
                }
                else
                {
                    if (i != trail.Length - 1)
                        if (trail[i + 1] != -1)
                        {
                            double[] res = Distance(trail[i], trail[i + 1], dists_cost, dists_time);
                            result[0] += res[0];
                            result[1] += res[1];
                        }
                }
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private static int[] BestTrail(int[][] ants, int[][] dists_cost, int[][] dists_time, int[] values)
        {
            double[] bestLength = Length(ants[0], dists_cost, dists_time);
            double bestVal = Values(ants[0], values);
            int idxBestLength = 0;
            for (int k = 1; k <= ants.Length - 1; k++)
            {
                double[] len = Length(ants[k], dists_cost, dists_time);
                double val = Values(ants[k], values);
                if (  (bestVal<val && (len[0]+len[1]< bestLength[0]+bestLength[1])) || (bestVal==val && (len[0] + len[1] < bestLength[0] + bestLength[1])) )
                {
                    bestLength = len;
                    bestVal = val;
                    idxBestLength = k;
                }
            }
            int numCities = ants[0].Length;
            int[] bestTrail_Renamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }

        // -------------------------------------------------------------------------------------------- 

        private static double Values(int[] trail, int[] values)
        {
            double result = 0;
            int counter = 0;
            for (int i = 1; i <= trail.Length - 1; i++)
                if (trail[i] == trail[0])
                    counter++;

            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (i == trail.Length - 1 && counter == 0)
                {
                    result += values[trail[i]];
                }
                else
                {
                    if (i != trail.Length - 1)
                        if (trail[i + 1] != -1)
                        {
                            result += values[trail[i]];
                        }
                }
            }
            return result;
        }

        // --------------------------------------------------------------------------------------------

        private static double[][] InitPheromones(int numCities, int[][] dists_cost)
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    if (dists_cost[i][j] == 0)
                        pheromones[i][j] = 0;
                    else
                        pheromones[i][j] = 0.01;
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists_cost, int[][] dists_time)
        {
            int numCities = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                Random rnd = new Random();
                int start = rnd.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start, pheromones, dists_cost, dists_time);
                ants[k] = newTrail;
            }
        }

        private static int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists_cost, int[][] dists_time)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            int ind = 0;
            while (ind == 0)
            {
                ind = 1;
                bool[] visited = new bool[numCities];
                trail[0] = start;
                visited[start] = false;
                for (int i = 0; i <= numCities - 2; i++)
                {
                    int cityX = trail[i];
                    int next = NextCity(k, cityX, visited, pheromones, dists_cost, dists_time);

                    if (i == numCities - 2 && dists_cost[next][trail[0]] == 0)
                        ind = 0;

                    trail[i + 1] = next;
                    visited[next] = true;
                    if (next == start)
                    {
                        for (int j = i + 2; j <= numCities - 1; j++)
                            trail[j] = -1;
                        return trail;
                    }
                }
            }
            return trail;
        }

        private static int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists_cost, int[][] dists_time)
        {
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists_cost, dists_time);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
            }

            Random rnd = new Random();
            double p = rnd.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private static double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists_cost, int[][] dists_time)
        {
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            double sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (int i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else if (dists_cost[cityX][i]==0)
                {
                    taueta[i] = 0.0;
                    // prob of moving to city, withiut direct pass is 0
                }
                else
                {
                    double[] dists = Distance(cityX, i, dists_cost, dists_time);
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / (dists[0]*dists[1])), beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists_cost, int[][] dists_time)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    if (j == i || dists_cost[i][j] == 0)
                        continue;
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double[] length = AntColonyProgram.Length(ants[k], dists_cost, dists_time);
                        // length of ant k trail
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true) 
                        {
                            increase = (Q / (length[0] * length[1]));
                        }
                        
                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        //pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private static bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx != -1)
            {
                if (idx == 0 && trail[1] == cityY)
                {
                    return true;
                }
                else if (idx == 0 && trail[lastIndex] == cityY)
                {
                    return true;
                }
                else if (idx == 0)
                {
                    return false;
                }
                else if (idx == lastIndex && trail[0] == cityY)
                {
                    return true;
                }
                else if (idx == lastIndex)
                {
                    return false;
                }
                else if (idx != lastIndex && trail[idx + 1] == cityY)
                {
                    return true;
                }
                else if(idx!= lastIndex && trail[idx + 1] == -1 && trail[idx]==trail[0])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }


        // --------------------------------------------------------------------------------------------


        private static double[] Distance(int cityX, int cityY, int[][] dists_cost, int[][] dists_time)
        {
            double[] res = new double[2] { dists_cost[cityX][cityY], dists_time[cityX][cityY] };
            return res;
        }

        // --------------------------------------------------------------------------------------------

        private static void Display(int[] trail)
        {
            int ind = 0;
            for (int i = 0; i < trail.Length; i++)
            {
                if (trail[i] == -1)
                    ind++;
            }

            if (ind == 0)
            {
                for (int i = 0; i <= trail.Length; i++)
                {
                    if (i == trail.Length && trail[i - 1] != trail[0])
                        Console.Write(trail[0] + 1);
                    else
                    {
                        if (i == trail.Length - 1)
                        {
                            if (trail[i] == trail[0])
                                Console.Write(trail[0] + 1);
                            else
                                Console.Write(trail[i] + 1 + " ---> ");
                        }

                        if (i < trail.Length - 1)
                            Console.Write(trail[i] + 1 + " ---> ");
                    }
                }
            }
            else
            {
                for (int i = 0; i < trail.Length; i++)
                {
                    if (trail[i] == -1)
                        Console.Write("");
                    else
                    {
                        if (i != trail.Length - 1)
                        {
                            if (trail[i + 1] == -1)
                                Console.Write(trail[i] + 1);
                            else
                                Console.Write(trail[i] + 1 + " ---> ");
                        }
                        else
                            Console.Write(trail[i] + 1);
                    }
                }
            }
            Console.WriteLine();
        }


        private static void ShowAnts(int[][] ants, int[][] dists_cost, int[][] dists_time)
        {
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                Console.Write(i + ": [ ");

                for (int j = 0; j <= ants[i].Length; j++)
                {
                    if (j == ants[i].Length)
                        Console.Write(ants[i][0]+1);
                    else
                        Console.Write(ants[i][j]+1 + " ---> ");
                }

                Console.Write("] len = ");
                double[] len = Length(ants[i], dists_cost, dists_time);
                Console.Write("Cost: {0}  Time: {1} ", len[0].ToString("F1"), len[1].ToString("F1"));
                Console.WriteLine("");
            }
        }

    }
}