using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace GeneticKomi
{
    public class Solution
    {
        public int[] Vector;
        public int Func = int.MaxValue;
    }

    public class GeneticKomiObject
    {
        private readonly int[,] _weights;
        private int _firstPopulationNumber;
        private int _numberOfCrossings;
        private List<Solution> _solutions;
        private Random rand = new Random();

        public GeneticKomiObject(int[,] weights, int sizeOfPopulation, int numberOfCrossings)
        {
            Console.WriteLine("Инициализация объекта");
            _weights = weights;
            var howNodes = _weights.GetLength(0);
            _firstPopulationNumber = sizeOfPopulation;
            _numberOfCrossings = numberOfCrossings;
            _solutions = new List<Solution>(_firstPopulationNumber);

            for (int i = 0; i < _firstPopulationNumber; i++)
            {
                var temp = Enumerable.Range(0, howNodes).ToList().Shuffle();
                temp.Add(temp.First());
                var tempSol = new Solution { Vector =  temp.ToArray() };
                var f = Function(tempSol);
                tempSol.Func = f;
                _solutions.Add(tempSol);
            }

            _solutions = _solutions.OrderBy(s => s.Func).ToList();

            Console.WriteLine("Начальная популяция:");
            ShowSolutions(_solutions, false);

            Console.WriteLine("Инициализация завершена");
        }

        public List<Solution> Process(out int iterations)
        {
            int min = 0;
            iterations = 0;
            do
            {
                List<Solution> children = new List<Solution>(_numberOfCrossings * 2);
                iterations++;

                Console.WriteLine($"Иттерация {iterations}:");
                Console.WriteLine("Популяция:");
                ShowSolutions(_solutions, true);

                for (int i = 0; i < _numberOfCrossings; i++)
                {
                    var mother = 0;

                    int father = (mother + rand.Next(1, _numberOfCrossings)) % _solutions.Count;
                    while (mother == father)
                    {
                        father = (mother + rand.Next(1, _numberOfCrossings)) % _solutions.Count;
                    }

                    var (son, daughter) = Crossing(_solutions[mother], _solutions[father]);

                    son = Mutation(son);
                    daughter = Mutation(daughter);
                    children.Add(son);
                    children.Add(daughter);
                }

                //Console.WriteLine("Популяция:");
                //ShowSolutions(_solutions, true);

                //Console.WriteLine("Потомки:");
                //ShowSolutions(children, true);
                _solutions.AddRange(children);

                Console.WriteLine($"До отбрасывания:");
                ShowSolutions(_solutions, true);

                Discarding();
                Console.WriteLine($"После отбрасывания:");
                ShowSolutions(_solutions, true);
                min = _solutions.Min(sol => sol.Func);
            } while (_solutions.Count() != 1 && _solutions.Count() != _solutions.Count(solution => solution.Func == min));

            return _solutions;
        }

        private void Discarding()
        {
            var newSolutions = _solutions.Select(sol => new Solution { Vector = sol.Vector, Func = Function(sol) }).Where(s => AreUnique(s.Vector)).ToList();
            var max = _solutions.Max(sol => sol.Func);
            
            if (newSolutions.Count() == newSolutions.Count(solution => solution.Func == max))
            {
                _solutions = _solutions.OrderBy(s => s.Func).Take(_firstPopulationNumber).ToList();
            }

            _solutions = newSolutions.Where(sol => sol.Func < max).OrderBy(s => s.Func).Take(_firstPopulationNumber).ToList();
        }

        private (Solution, Solution) Crossing(Solution mother, Solution father)
        {
            int p1 = rand.Next(1, mother.Vector.Length - 1);

            int p2 = rand.Next(1, mother.Vector.Length - 1);
            while (p2 == p1)
            {
                p2 = rand.Next(1, mother.Vector.Length - 1);
            }

            int p3 = rand.Next(1, mother.Vector.Length - 1);
            while (p3 == p2 || p3 == p1)
            {
                p3 = rand.Next(1, mother.Vector.Length - 1);
            }

            var ps = new List<int> { p1, p2, p3 };
            ps.Sort();
            p1 = ps[0];
            p2 = ps[1];
            p3 = ps[2];

            Solution son = new Solution { Vector = ArCopy(mother.Vector) };
            Solution daughter = new Solution { Vector = ArCopy(father.Vector) };

            for (int i = p1; i <= p2; i++)
            {
                int temp = son.Vector[i];
                son.Vector[i] = daughter.Vector[i];
                daughter.Vector[i] = temp;
            }

            for (int i = p3; i < mother.Vector.Length - 1; i++)
            {
                int temp = son.Vector[i];
                son.Vector[i] = daughter.Vector[i];
                daughter.Vector[i] = temp;
            }

            var fS = Function(son);
            var fD = Function(daughter);
            son.Func = fS;
            daughter.Func = fD;

            return (son, daughter);
        }

        private int Function(Solution solution)
        {
            int sum = 0;

            for (int i = 1; i < solution.Vector.Length; i++)
            {
                if (_weights[solution.Vector[i - 1], solution.Vector[i]] == int.MaxValue)
                {
                    return int.MaxValue;
                }

                sum += _weights[solution.Vector[i - 1], solution.Vector[i]];
            }

            return sum;
        }

        private Solution Mutation(Solution solution)
        {
            int p1 = rand.Next(1, solution.Vector.Length - 1);
            int p2 = rand.Next(1, solution.Vector.Length - 1);
            var newSol = new Solution { Vector = ArCopy(solution.Vector) };
            int temp = newSol.Vector[p1];
            newSol.Vector[p1] = newSol.Vector[p2];
            newSol.Vector[p2] = temp;
            var f = Function(newSol);
            newSol.Func = f;
            return newSol;
        }

        private void Swap<T>(ref T[] vector, int where1, int where2)
        {
            T temp = vector[where1];
            vector[where1] = vector[where2];
            vector[where2] = temp;
        }

        private string ArToStr(int[] ar)
        {
            return '[' + string.Join(", ", ar.Select(i => i.ToString()).ToArray()) + ']';
        }

        private void ShowSolutions(List<Solution> solutions, bool showFunc)
        {
            for(int i = 0; i < solutions.Count; i++)
            {
                Console.WriteLine($"\t{i}. " + ArToStr(solutions[i].Vector) + (showFunc ? $"Функция: {solutions[i].Func}" : ""));
            }  
        }

        private int[] ArCopy(int[] ar)
        {
            int[] res = new int[ar.Length];
            for (int i = 0; i < ar.Length; i++)
            {
                res[i] = ar[i];
            }

            return res;
        }

        private bool AreUnique(int[] ar)
        {
            var res = ar.ToList();
            res.RemoveAt(res.Count - 1);
            res.Sort();
            for (int i = 1; i < res.Count; i++)
            {
                if (res[i] == res[i - 1])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
