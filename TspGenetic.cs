using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEA
{
	public enum CrossoverType { OX = 0, PMX = 1, Other = 2}

	class TspGenetic
	{
		#region FieldsAndProperties
		private static int numbOfCities; //liczba miast
		private static double globalBestFitness;
		private static List<List<Int16>> population; //populacja - tablica tablic reprezentujacych osobnikow
		private static List<double> fitnessList;
		private static List<Int16> globalBestSolution;
		private static Random rand1, rand2;
		private static CrossoverType cType;
		private static Stopwatch stopwatch;
		private static int timeLimit;
		public static int Generations { get; set; }
		public static Int16[][] InputGraph { get; set; } //graf wejsciowy
		public static List<Int16> OutputList { get; private set; } //lista miast odwiedzonych w danej kolejnosci
		public static int PopulationSize { get; set; } //rozmiar populacji
		public static int ElitismSize { get; set; } //ilosc wybranych najlepszych do nowej generacji
		public static int TournamentSize { get; set; } //rozmiar puli wyboru do krzyzowania
		public static double CrossoverRate { get; set; }
		public static double MutationRate { get; set; } //szansa na mutacje
		public static int PathDistance { get; private set; } //dlugosc sciezki
		public static TimeSpan TimeMeasured { get; private set; } //zmierzony czas wykonywania obliczen
        public static short startingIndex;
        public static short endingIndex;
        public static Mutex m = new Mutex();
        public static int threadsAmount = 2;
        public static List<List<List<Int16>>> threadsPopulations;
        public static List<List<double>> threadsFitnessList;
        public static bool solutionFound = false;
        public static int taskId = 0;
        #endregion

        public static void SolveTsp(TspGraph input, int elitismSize, double crossoverRate, double mutationRate, int populationSize, short startingIndex, short endingIndex, CrossoverType cT, int limit = 300, int generations = 10000, int tournamentSize = 2)
		{
            
			if (input.Dimension > 2 && input.Name != null && input.GraphMatrix != null)
			{
				if (elitismSize % 2 == 1)
				{
					elitismSize--;
					Console.WriteLine("Ilość elit powinna być podzielna przez 2. Zmniejszono o 1.");
				}
				if (populationSize % 2 == 1)
				{
					populationSize--;
					Console.WriteLine("Wielkość populacji powinna być podzielna przez 2. Zmniejszono o 1.");
				}

				bool allright = Init(input, generations, populationSize, elitismSize, tournamentSize, mutationRate, crossoverRate, cT, limit, startingIndex, endingIndex);

				if (allright)
				{
					stopwatch = new Stopwatch();
					stopwatch.Start();
					Search();
					stopwatch.Stop();
					TimeMeasured = stopwatch.Elapsed;
				}
			}
			else Console.WriteLine("Przed uruchomieniem algorytmu wczytaj odpowiednio dane wejściowe.\n(Wciśnij dowolny klawisz aby wrócić)");
		}

		private static bool Init(TspGraph input, int generations, int populationSize, int elitismSize, int tournamentSize, double mutationRate, double crossoverRate, CrossoverType cT, int limit, short sIndex, short eIndex, int tasksAmount = 1)
		{
			try
			{
				InputGraph = input.GraphMatrix;
				numbOfCities = input.Dimension;
				Generations = generations;
				PopulationSize = populationSize;
				ElitismSize = elitismSize;
				TournamentSize = tournamentSize;
				MutationRate = mutationRate;
				CrossoverRate = crossoverRate;
				globalBestFitness = 0;
				rand1 = new Random();
				rand2 = new Random();
				cType = cT;
				timeLimit = limit;
                startingIndex = sIndex;
                endingIndex = eIndex;
                solutionFound = false;
                threadsAmount = tasksAmount;
                taskId = 0;

                threadsFitnessList = new List<List<double>>(threadsAmount);
                for(int i = 0; i < threadsAmount; i++)
                {
                    threadsFitnessList.Add(new List<double>(populationSize / threadsAmount));
                }

                threadsPopulations = new List<List<List<Int16>>>(threadsAmount);

                for (int i = 0; i < threadsAmount; i++)
                {
                    threadsPopulations.Add(new List<List<Int16>>(populationSize / threadsAmount));
                    for (int j = 0; j < threadsPopulations[i].Capacity; j++) {
                        threadsPopulations[i].Add(new List<short>(numbOfCities));
                        threadsPopulations[i][j].Add(startingIndex);
                        for (Int16 k = 0; k < threadsPopulations[i][j].Capacity; k++)
                        {
                            if (k == startingIndex || k == endingIndex) continue;
                            threadsPopulations[i][j].Add(k);
                        }
                        threadsPopulations[i][j].Add(endingIndex);
                        RandomizeSolution(threadsPopulations[i][j], j);
                        threadsFitnessList[i].Add(CalculateFitness(threadsPopulations[i][j]));
                    }
                }

				fitnessList = new List<double>(populationSize);
				population = new List<List<Int16>>(populationSize);

				for (int i = 0; i < population.Capacity; i++)
				{
					population.Add(new List<short>(numbOfCities));
                    population[i].Add(startingIndex);
                    for (Int16 j = 0; j < population[i].Capacity; j++)
                    {
                        if (j == startingIndex || j == endingIndex) continue;
                        population[i].Add(j);
                    }
					population[i].Add(endingIndex);
					RandomizeSolution(population[i], i);
					fitnessList.Add(CalculateFitness(population[i]));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Błąd. Opis błędu: " + e);
				return false;
			}

			return true;
		}


        private static void Search()
        {
            double currentRunningTime = 0;

            var populations = new List<List<List<Int16>>>();

            if (threadsAmount > 0)
            {                 
                for (int i = 0; i < threadsAmount; i++)
                {
                    populations.Add(new List<List<Int16>>());
                    for (int j = i * (PopulationSize / threadsAmount); j < (i + 1) * (PopulationSize / threadsAmount); j++)
                    {
                        populations[i].Add(population[j]);
                    }
                }
            }

            threadsPopulations = populations;
            
            List<Task> tasks = new List<Task>(threadsAmount);
            for (int i = 0; i < threadsAmount; i++)
                tasks.Add(Task.Run(() => findSolution()));
            Task.WaitAll(tasks.ToArray());
                               
			OutputList = globalBestSolution;
			PathDistance = CalculateCost(OutputList);
        }

        private static void findSolution()
        {
            int id = taskId++;
            double currentRunningTime = 0;

            for (int i = 0; i < Generations; i++)
            {
                if (solutionFound)
                    break;

                threadsPopulations[id] = NextGeneration(id);

                threadsFitnessList[id] = new List<double>(PopulationSize / threadsAmount);

                for(int j = 0; j < threadsFitnessList[id].Capacity; j++)
                    threadsFitnessList[id].Add(CalculateFitness(threadsPopulations[id][j]));

                int tempMaxIndex = FindFittest(threadsFitnessList[id]);

                
                if(threadsFitnessList[id][tempMaxIndex] > globalBestFitness)
                {
                    m.WaitOne();
                    globalBestFitness = threadsFitnessList[id][tempMaxIndex];
                    globalBestSolution = threadsPopulations[id][tempMaxIndex];
                    m.ReleaseMutex();
                }

                m.WaitOne();
                if(CalculateCost(globalBestSolution) <= TspDynamicProgramming.PathDistance * 1.05)
                {
                    solutionFound = true;
                    m.ReleaseMutex();
                    break;
                }
                m.ReleaseMutex();
                                             

                currentRunningTime = stopwatch.Elapsed.TotalSeconds;
                Debug.WriteLine(currentRunningTime);
                if (currentRunningTime > timeLimit)
                    break;
            }
        }

		private static List<List<Int16>> NextGeneration(int id)
		{
			List<List<Int16>> newPopulation = new List<List<Int16>>(PopulationSize/threadsAmount);
			ChooseElites(newPopulation, id);
			
			while (newPopulation.Count != (PopulationSize / threadsAmount))
			{
				if (rand1.NextDouble() < CrossoverRate)
				{
					List<Int16> parentA = TournamentSelection(id);
					List<Int16> parentB = TournamentSelection(id);
					Tuple<List<Int16>, List<Int16>> children;
					switch (cType)
					{
						case CrossoverType.OX:
							children = CrossoverOX(parentA, parentB);
							break;
						case CrossoverType.PMX:
							children = CrossoverPMX(parentA, parentB);
							break;
						case CrossoverType.Other:
							children = Crossover(parentA, parentB);
							break;
						default:
							children = CrossoverOX(parentA, parentB);
							break;
					}

					//Tuple<List<Int16>, List<Int16>> children = CrossoverOX(parentA, parentB);
					List<Int16> child1 = children.Item1;
					List<Int16> child2 = children.Item2;

					if (rand1.NextDouble() < MutationRate)
						MutationInvert(child1);
					if (rand2.NextDouble() < MutationRate)
						MutationInvert(child2);
					newPopulation.Add(child1);
					newPopulation.Add(child2);
				}
			}
			

			return newPopulation;
		}       

        private static List<Int16> TournamentSelection(int id)
		{
			int index = rand1.Next(PopulationSize/threadsAmount);
			int bestIndex = index;
			double bestFitness = threadsFitnessList[id][index];

			for (int i = 1; i < TournamentSize; i++)
			{
				index = rand1.Next(PopulationSize/threadsAmount);
				if (threadsFitnessList[id][index] > bestFitness)
				{
					bestFitness = threadsFitnessList[id][index];
					bestIndex = index;
				}
			}

			List<Int16> best = threadsPopulations[id][bestIndex];

			return best;
		}

		private static int FindFittest(List<double> fitnessListPar)
		{
			double max = 0;
			int maxIndex = 0;

			for (int j = 0; j < fitnessListPar.Count; j++)
			{
				if (fitnessListPar[j] > max)
				{
					max = fitnessListPar[j];
					maxIndex = j;
				}
			}

			return maxIndex;
		}

		private static void ChooseElites(List<List<Int16>> newGeneration, int id)
		{
			List<List<Int16>> oldGeneration = new List<List<Int16>>(threadsPopulations[id]);
			List<double> oldFitnessList = new List<double>(threadsFitnessList[id]);
			int maxIndex;

			for (int i = 0; i < ElitismSize; i++)
			{
				maxIndex = FindFittest(oldFitnessList);

				newGeneration.Add(oldGeneration[maxIndex]);
				oldGeneration.RemoveAt(maxIndex);
				oldFitnessList.RemoveAt(maxIndex);
			}
		}

		private static Tuple<List<Int16>, List<Int16>> CrossoverPMX(List<Int16> parentA, List<Int16> parentB)
		{
			int startIndex = rand1.Next(1, parentA.Count - 2);
			int endIndex = rand2.Next(startIndex + 1, parentA.Count - 1);
			List<Int16> child1 = new List<Int16>(parentA.Count);
			List<Int16> child2 = new List<Int16>(parentA.Count);

			for (int i = 0; i < child1.Capacity; i++)
			{
				child1.Add(0);
				child2.Add(0);
			}

			for (int i = startIndex; i <= endIndex; i++)
			{
				child1[i] = parentA[i];
				child2[i] = parentB[i];
			}

			int itA = 1, itB = 1;

			for (int i = 1; i < startIndex; i++)
			{
				for (; itB < parentB.Count - 1; itB++)
				{
					if (!child1.Contains(parentB[itB]))
					{
						child1[i] = parentB[itB];
						break;
					}
				}
				for (; itA < parentA.Count - 1; itA++)
				{
					if (!child2.Contains(parentA[itA]))
					{
						child2[i] = parentA[itA];
						break;
					}
				}
			}

			for (int i = endIndex + 1; i < child1.Capacity - 1; i++)
			{
				for (; itB < parentB.Count - 1; itB++)
				{
					if (!child1.Contains(parentB[itB]))
					{
						child1[i] = parentB[itB];
						break;
					}
				}
				for (; itA < parentA.Count - 1; itA++)
				{
					if (!child2.Contains(parentA[itA]))
					{
						child2[i] = parentA[itA];
						break;
					}
				}
			}

			Tuple<List<Int16>, List<Int16>> returnTuple = new Tuple<List<Int16>, List<Int16>>(child1, child2);

			return returnTuple;
		}

		private static Tuple<List<Int16>, List<Int16>> CrossoverOX(List<Int16> parentA, List<Int16> parentB)
		{
			int startIndex = rand1.Next(1, parentA.Count - 2);
			int endIndex = rand2.Next(startIndex + 1, parentA.Count - 1);
			List<Int16> child1 = new List<Int16>(parentA.Count);
			List<Int16> child2 = new List<Int16>(parentA.Count);

			for (int i = 0; i < child1.Capacity - 1; i++)
			{
				child1.Add(startingIndex);
				child2.Add(startingIndex);
			}
            child1.Add(endingIndex);
            child2.Add(endingIndex);

            for (int i = startIndex; i <= endIndex; i++)
			{
				child1[i] = parentA[i];
				child2[i] = parentB[i];
			}

			int itA = 1, itB = 1;

			for (int i = endIndex + 1; i < child1.Capacity - 1; i++)
			{
				for (; itB < parentB.Count - 1; itB++)
				{
					if (!child1.Contains(parentB[itB]))
					{
						child1[i] = parentB[itB];
						break;
					}
				}
				for (; itA < parentA.Count - 1; itA++)
				{
					if (!child2.Contains(parentA[itA]))
					{
						child2[i] = parentA[itA];
						break;
					}
				}
			}

			for (int i = 1; i < startIndex; i++)
			{
				for (; itB < parentB.Count - 1; itB++)
				{
					if (!child1.Contains(parentB[itB]))
					{
						child1[i] = parentB[itB];
						break;
					}
				}
				for (; itA < parentA.Count - 1; itA++)
				{
					if (!child2.Contains(parentA[itA]))
					{
						child2[i] = parentA[itA];
						break;
					}
				}
			}

			Tuple<List<Int16>, List<Int16>> returnTuple = new Tuple<List<Int16>, List<Int16>>(child1, child2);

			return returnTuple;
		}

		private static Tuple<List<Int16>, List<Int16>> Crossover(List<Int16> parentA, List<Int16> parentB)
		{
			int startIndex = rand1.Next(1, parentA.Count - 2);
			int endIndex = rand2.Next(startIndex + 1, parentA.Count - 1);
			List<Int16> child1 = new List<Int16>(parentA.Count);
			List<Int16> child2 = new List<Int16>(parentA.Count);

			child1.Add(0);
			child2.Add(0);

			for (int i = startIndex; i <= endIndex; i++)
			{
				child1.Add(parentA[i]);
				child2.Add(parentB[i]);
			}

			for (int i = 1; i < parentA.Count - 1; i++)
			{
				if (!child1.Contains(parentA[i]))
					child1.Add(parentA[i]);
			}
			for (int i = 1; i < parentB.Count - 1; i++)
			{
				if (!child2.Contains(parentB[i]))
					child2.Add(parentB[i]);
			}
			child1.Add(0);
			child2.Add(0);

			Tuple<List<Int16>, List<Int16>> returnTuple = new Tuple<List<Int16>, List<Int16>>(child1, child2);

			return returnTuple;
		}

		private static void MutationSwap(List<Int16> sol)
		{
			int point1 = rand1.Next(1, sol.Count - 1);
			int point2 = rand1.Next(1, sol.Count - 1);

			while (point1 == point2)
				point2 = rand1.Next(1, sol.Count - 1);

			Swap(point1, point2, sol);
		}

		private static void MutationInvert(List<Int16> sol)
		{
			int startIndex = rand1.Next(1, sol.Count - 2);
			int endIndex = rand1.Next(startIndex + 1, sol.Count - 1);
			int howMany = endIndex - startIndex;

			sol.Reverse(startIndex, howMany);
		}

		private static int CalculateCost(List<Int16> sol)
		{
			int temp = 0;
			Int16 i1, i2;

			i1 = sol[0];

			for (int i = 1; i < sol.Count; i++)
			{
				i2 = sol[i];
				temp += InputGraph[i1][i2];
				i1 = i2;
			}

			return temp;
		}

		private static double CalculateFitness(List<Int16> sol)
		{
			int distance = CalculateCost(sol);

			return 10000.0 / distance;
		}

		private static void Swap(int i, int j, List<Int16> sol)
		{
			Int16 temp = sol[i];
			sol[i] = sol[j];
			sol[j] = temp;
		}

		private static void RandomizeSolution(List<Int16> sol, int seed)
		{
			Random rand = new Random(seed);
			for (int i = 1; i < sol.Count - 1; i++)
			{
				int j = rand.Next(1, sol.Count - 1);
				Swap(i, j, sol);
			}
		}

		public static void ShowResults()
		{
			if (OutputList != null)
			{
				Console.WriteLine("Algorytm genetyczny - wyniki");
				Console.WriteLine("Parametry:");
				Console.WriteLine("Wielkość populacji: " + PopulationSize);
				Console.WriteLine("Ilość pokoleń: " + Generations);
				Console.WriteLine("Wielkość puli turniejowej: " + TournamentSize);
				Console.WriteLine("Ilość elit: " + ElitismSize);
				Console.WriteLine("Prawdopodobieństwo zajścia mutacji: " + MutationRate);
				Console.WriteLine("Prawdopodobieństwo krzyżowania: " + CrossoverRate);
				Console.WriteLine("Długość ścieżki: " + PathDistance);
				Console.WriteLine("Kolejność miast: ");
				foreach (var city in OutputList)
				{
					Console.Write(city + " ");
				}
				Console.Write('\n');
				Console.WriteLine("Wynik wyznaczono w czasie: " + TimeMeasured.TotalMilliseconds + " ms.");
			}
		}
	}//class
}//namespace