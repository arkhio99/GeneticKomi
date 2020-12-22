using System;
using System.Linq;

namespace GeneticKomi
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] weights = new int[7, 7];
            Random rand = new Random();
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    weights[i, j] = i == j ? int.MaxValue : rand.Next(2, 9);
                }
            }

            int sizeOfPopulation = 9;
            int numberOfCrossings = 4;

            var optimization = new GeneticKomiObject(weights, sizeOfPopulation, numberOfCrossings);

            var solutions = optimization.Process(out int iterations);


            Console.WriteLine("Получены следующие решения:");
            for (int i = 0; i < solutions.Count; i++)
            {
                Console.WriteLine(ArToStr(solutions[i].Vector));
            }

            Console.WriteLine($"Длина пути составит {solutions[0].Func}");
        }

        private static string ArToStr(int[] ar)
        {
            return '[' + string.Join(", ", ar.Select(i => i.ToString()).ToArray()) + ']';
        }
    }
}
