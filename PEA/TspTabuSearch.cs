using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEA
{
	class TspTabuSearch
	{
		#region FieldsAndProperties
		private static int numbOfCities; //liczba miast
		private static int currentBestDistance;
		private static int globalBestDistance;
		private static List<Int16> currentSolution;
		private static List<Int16> globalBestSolution;
		private static Int16[][] tabuList; //lista tabu klucz - ruch, wartosc 1 - pozycja na liscie tabu, wartosc 2 - dlugoterminowa kara
		public static int TabuListCap { get; set; }
		public static int MaxIterations { get; set; } //max liczba iteracji algorytmu
		public static int MaxTriesForBranch { get; set; } //max liczba iteracji dla jednego rozwiazania poczatkowego
		public static int PityTimer { get; set; } //max liczba iteracji bez poprawy wyniku - po tym losuje nowa sekwencje miast
		public static Int16[][] InputGraph { get; set; } //graf wejsciowy
		public static List<Int16> OutputList { get; private set; } //lista miast odwiedzonych w danej kolejnosci
		public static int PathDistance { get; private set; } //dlugosc sciezki
		public static TimeSpan TimeMeasured { get; private set; } //zmierzony czas wykonywania obliczen
		#endregion

		public static void SolveTsp(TspGraph input, int maxIterations, int maxTriesForBranch, int pityTimer, int tabuListCap)
		{
			bool allright = Init(input, maxIterations, maxTriesForBranch, pityTimer, tabuListCap);

			if (allright)
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				Search();
				stopwatch.Stop();
				TimeMeasured = stopwatch.Elapsed;
			}
		}

		private static bool Init(TspGraph input, int maxIterations, int maxTriesForBranch, int pityTimer, int tabuListCap)
		{
			try
			{
				InputGraph = input.GraphMatrix;
				numbOfCities = input.Dimension;
				MaxIterations = maxIterations;
				MaxTriesForBranch = maxTriesForBranch;
				PityTimer = pityTimer;
				TabuListCap = tabuListCap;
				globalBestDistance = int.MaxValue;

				tabuList = new Int16[numbOfCities][]; //miesci wszystkie miasta ale miasta 0 nie ruszy i tak

				for (int i = 0; i < numbOfCities; i++)
					tabuList[i] = new Int16[2];
				resetTabuList();

				currentSolution = new List<Int16>(numbOfCities + 1);

				for (Int16 i = 0; i < currentSolution.Capacity - 1; i++)
					currentSolution.Add(i);
				currentSolution.Add(0);
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
			for (int x = 0; x < MaxIterations; x++)
			{
				RandomizeSolution(currentSolution);
				resetTabuList();
				int pityTimerCount = 0;
				currentBestDistance = int.MaxValue;

				for (int i = 0; i < MaxTriesForBranch; i++)
				{
					GetBestNeighbour(i, currentSolution);
					int distance = CalculateCost(currentSolution);

					if (distance < currentBestDistance)
					{
						currentBestDistance = distance;
						pityTimerCount = 0;

						if (currentBestDistance < globalBestDistance)
						{
							globalBestSolution = currentSolution;
							globalBestDistance = currentBestDistance;
						}
						else
						{
							pityTimerCount++;
							if (pityTimerCount > PityTimer)
								break;
						}
					}
				}
			}

			OutputList = globalBestSolution;
			PathDistance = globalBestDistance;
		}

		private static void GetBestNeighbour(int iteration, List<Int16> sol)
		{
			int bestPenaltyScore = int.MaxValue, city1 = 0, city2 = 0;

			for (int i = 1; i < numbOfCities - 1; i++) //startowego i koncowego nie ruszamy
			{
				for (int j = i + 1; j < numbOfCities; j++)
				{
					Swap(i, j, sol);
					int currentDistance = CalculateCost(sol);
					int penalty = currentDistance + tabuList[i][1];

					if ((penalty < bestPenaltyScore && tabuList[i][0] <= iteration) || currentDistance < currentBestDistance)
					{
						city1 = i;
						city2 = j;
						bestPenaltyScore = penalty;
						tabuList[i][0] = tabuList[j][0] = (Int16)(iteration + TabuListCap);
					}
					Swap(j, i, sol);
					if (tabuList[i][1] > 0)
						tabuList[i][1]--;
					if (tabuList[j][1] > 0)
						tabuList[j][1]--;
				}
			}

			tabuList[city1][1] += 2;
			tabuList[city2][1] += 2;
			Swap(city1, city2, sol);
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

		private static void Swap(int i, int j, List<Int16> sol)
		{
			Int16 temp = sol[i];
			sol[i] = sol[j];
			sol[j] = temp;
		}

		private static void RandomizeSolution(List<Int16> sol)
		{
			Random rand = new Random();
			for (int i = 1; i < sol.Count - 1; i++)
			{
				int j = rand.Next(1, sol.Count - 1);
				Swap(i, j, sol);
			}
		}

		private static void resetTabuList()
		{
			foreach (Int16[] subArray in tabuList)
			{
				subArray[0] = subArray[1] = 0;
			}
		}

		public static void ShowResults()
		{
			if (OutputList != null)
			{
				Console.WriteLine("Tabu Search - wyniki");
				Console.WriteLine("Parametry:");
				Console.WriteLine("Maksymalna ilość iteracji: " + MaxIterations);
				Console.WriteLine("Maksymalna ilość prób dla jednej gałęzi: " + MaxTriesForBranch);
				Console.WriteLine("Dopuszczalna ilość iteracji bez poprawy wyniku: " + PityTimer);
				Console.WriteLine("Długość listy zakazów: " + TabuListCap);
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