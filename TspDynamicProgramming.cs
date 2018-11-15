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
		private static Int16[][] g, p; //g - tablica podproblemow, p - tablica poprzednikow
		private static int numbOfCities; //liczba miast
		private static Int64 powah; //2^(liczba miast)
		public static Int16[][] InputGraph { get; set; } //graf wejsciowy
		public static List<Int16> OutputList { get; private set; } //lista miast odwiedzonych w danej kolejnosci
		public static int PathDistance { get; private set; } //dlugosc sciezki
		public static TimeSpan TimeMeasured { get; private set; } //zmierzony czas wykonywania obliczen
		#endregion
		//rozwiazanie problemu dla podanych danych wejsciowych
		public static void SolveTsp (TspGraph input)
		{
			if (input.Dimension > 2 && input.Name != null && input.GraphMatrix != null)
			{
				//jesli udalo sie zainicjalizowac zasoby bez problemow, to mozna wykonac obliczenia
				bool allright = Init(input);
				if (allright)
				{
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					PathDistance = ComputeDistance(0, powah - 2);
					stopwatch.Stop();
					TimeMeasured = stopwatch.Elapsed;
					OutputList.Add(0);
					GetOptimalPath(0, powah - 2);
					OutputList.Add(0);
				}
				else Console.WriteLine("Nie wykonano obliczeń, ponieważ wystąpił błąd.");
			}
			else Console.WriteLine("Przed uruchomieniem algorytmu wczytaj odpowiednio dane wejściowe.\n(Wciśnij dowolny klawisz aby wrócić)");
		}
		//inicjalizacja zasobow klasy przed wykonywaniem obliczen
		private static bool Init (TspGraph input)
		{
			InputGraph = input.GraphMatrix;
			numbOfCities = input.Dimension;
			try
			{
				powah = (Int64) Math.Pow(2, numbOfCities);
				g = new Int16[numbOfCities][];
				p = new Int16[numbOfCities][];
				PathDistance = int.MaxValue;
				OutputList = new List<Int16>();

				for (int i = 0; i < numbOfCities; i++)
				{
					g[i] = new Int16[powah];
					p[i] = new Int16[powah];
				}
			}
			catch (OutOfMemoryException e)
			{
				Console.WriteLine("Brak pamięci. Opis błędu: " + e);
				return false;
			}
			catch (OverflowException e)
			{
				Console.WriteLine(e);
				return false;
			}

			for (int i = 0; i < numbOfCities; i++)
			{
				for (Int64 j = 0; j < powah; j++)
				{
					g[i][j] = Int16.MaxValue;
					p[i][j] = Int16.MaxValue;
				}
			}

			for (int i = 0; i < numbOfCities; i++)
			{
				g[i][0] = InputGraph[i][0];
			}

			return true;
		}
		//obliczanie optymalnego dystansu zapelniajac tablice podproblemow kolejnymi wynikami
		private static int ComputeDistance (int start, Int64 set)
		{
			int minDistance = Int16.MaxValue, tempMinDistance;
			Int64 maskedSet, mask;

			if (g[start][set] != Int16.MaxValue)
				return g[start][set];

			for (Int16 i = 0; i < numbOfCities; i++)
			{
				mask = powah - 1 - (1 << i);
				maskedSet = mask & set;

				if (maskedSet != set)
				{
					tempMinDistance = InputGraph[start][i] + ComputeDistance(i, maskedSet);

					if (minDistance == Int16.MaxValue || tempMinDistance < minDistance)
					{
						minDistance = tempMinDistance;
						p[start][set] = i;
					}
				}
			}

			g[start][set] = (Int16)minDistance;
			return minDistance;
		}
		//wyluskanie kolejnych miast z tablicy poprzednikow
		private static void GetOptimalPath (int start, Int64 set)
		{
			if (p[start][set] == Int16.MaxValue)
				return;

			Int64 mask = powah - 1 - (1 << p[start][set]), maskedSet = mask & set;

			OutputList.Add(p[start][set]);
			GetOptimalPath(p[start][set], maskedSet);
		}
		//wyswietlenie wynikow
		public static void ShowResults ()
		{
			if (OutputList != null)
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
			}
		}
		//czyszczenie kolekcji aby gc oznaczyl zasoby do zwolnienia
		//inputGraph nie jest oznaczony jako null aby nie zniszczyc oryginalu
		public static void ClearCollections ()
		{
			for (int i = 0; i < numbOfCities; i++)
			{
				g[i] = null;
				p[i] = null;
			}
			g = null;
			p = null;
			OutputList = null;
		}
	}//class
}//namespace