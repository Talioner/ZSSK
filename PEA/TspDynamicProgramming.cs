using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PEA
{
	static class TspDynamicProgramming
	{
		#region FieldsAndProperties
		private static int[][] g, p; //g - subproplems 2D array, p - predecessors 2D array
		//private static List<LinkedList<int>> g, p;
		private static int numbOfCities;
		private static Int64 powah;
		public static int[][] InputGraph { get; set; }
		public static List<int> OutputList { get; private set; }
		public static int PathDistance { get; private set; }
		public static TimeSpan TimeMeasured { get; private set; }
		#endregion

		public static void SolveTsp (TspGraph input)
		{
			if (input.Dimension > 2 && input.Name != null && input.GraphMatrix != null)
			{
				Init(input);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				PathDistance = ComputeDistance(0, powah - 2);
				stopwatch.Stop();
				TimeMeasured = stopwatch.Elapsed;
				OutputList.Add(0);
				GetOptimalPath(0, powah - 2);
			}
			else
			{
				Console.WriteLine("Przed uruchomieniem algorytmu wczytaj odpowiednio dane wejściowe.\n(Wciśnij dowolny klawisz aby wrócić)");
				Console.ReadKey();
			}
		}

		private static void Init (TspGraph input) //przekazanie grafu moze byc inne w przyszlosci
		{
			InputGraph = input.GraphMatrix;
			numbOfCities = input.Dimension;
			try
			{
				powah = (Int64) Math.Pow(2, numbOfCities);
				g = new int[numbOfCities][];
				p = new int[numbOfCities][];
				PathDistance = int.MaxValue;
				OutputList = new List<int>();

				for (int i = 0; i < numbOfCities; i++)
				{
					g[i] = new int[powah];
					p[i] = new int[powah];
				}
			}
			catch (OutOfMemoryException e)
			{
				Console.WriteLine("Brak pamięci. Opis błędu: " + e);
			}
			catch (OverflowException e)
			{
				Console.WriteLine(e);
			}

			for (int i = 0; i < numbOfCities; i++)
			{
				for (Int64 j = 0; j < powah; j++)
				{
					g[i][j] = int.MaxValue;
					p[i][j] = int.MaxValue;
				}
			}

			for (int i = 0; i < numbOfCities; i++)
			{
				g[i][0] = InputGraph[i][0];
			}
		}

		private static int ComputeDistance (int start, Int64 set)
		{
			int minDistance = int.MaxValue, tempMinDistance;
			Int64 maskedSet, mask;

			if (g[start][set] != int.MaxValue)
				return g[start][set];

			for (int i = 0; i < numbOfCities; i++)
			{
				mask = powah - 1 - (1 << i);
				maskedSet = mask & set;

				if (maskedSet != set)
				{
					tempMinDistance = InputGraph[start][i] + ComputeDistance(i, maskedSet);

					if (tempMinDistance == int.MaxValue || tempMinDistance < minDistance)
					{
						minDistance = tempMinDistance;
						p[start][set] = i;
					}
				}
			}

			g[start][set] = minDistance;
			return minDistance;
		}

		private static void GetOptimalPath (int start, Int64 set)
		{
			if (p[start][set] == int.MaxValue)
				return;

			Int64 mask = powah - 1 - (1 << p[start][set]), maskedSet = mask & set;

			OutputList.Add(p[start][set]);
			GetOptimalPath(p[start][set], maskedSet);
		}

		public static void ShowResults ()
		{
			Console.WriteLine("Programowanie dynamiczne - wyniki");
			Console.WriteLine("Długość ścieżki: " + PathDistance);
			Console.WriteLine("Kolejność miast: ");
			foreach (var city in OutputList)
			{
				Console.Write(city + " ");
			}
			Console.Write('\n');
			Console.WriteLine("Wynik wyznaczono w czasie: " + TimeMeasured.TotalMilliseconds + " ms.");
			Console.ReadKey();
		}
	}//class
}//namespace