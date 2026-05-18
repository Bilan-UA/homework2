using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace TRSPO2
{
    class Program
    {
        static void Main()
        {
            // Набори даних згідно з вашим завданням
            long[] iterations = { 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000, 100_000_000_000 };
            int[] threadCounts = { 1, 2, 4, 8, 16, 32, 64, 128 };

            string filePath = "pi_monte_carlo_parallel_results.csv";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Формуємо шапку CSV: N/M,1,2,4,8...
                writer.WriteLine("N/M," + string.Join(",", threadCounts));

                foreach (long n in iterations)
                {
                    List<string> rowResults = new List<string> { n.ToString() };
                    Console.WriteLine($"\n>>> Обробка N = {n:N0}");

                    foreach (int m in threadCounts)
                    {
                        double timeTaken = RunMonteCarlo(n, m);
                        rowResults.Add(timeTaken.ToString("F4", CultureInfo.InvariantCulture));
                        Console.WriteLine($"    Threads: {m} | Time: {timeTaken:F4}s");
                    }

                    writer.WriteLine(string.Join(",", rowResults));
                    writer.Flush(); // Зберігаємо після кожного N
                }
            }

            Console.WriteLine($"\nГотово! Результати збережено у файл: {Path.GetFullPath(filePath)}");
        }

        static double RunMonteCarlo(long totalPoints, int threadCount)
        {
            long pointsPerThread = totalPoints / threadCount;
            Stopwatch sw = Stopwatch.StartNew();

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                // Важливо: свіжий Random для кожного потоку
                Random rand = new Random(Guid.NewGuid().GetHashCode() + i);
                long hits = 0;

                for (long j = 0; j < pointsPerThread; j++)
                {
                    double x = rand.NextDouble();
                    double y = rand.NextDouble();
                    if (x * x + y * y <= 1.0) hits++;
                }
            });

            sw.Stop();
            return sw.Elapsed.TotalSeconds;
        }
    }
}//dev 