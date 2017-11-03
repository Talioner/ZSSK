using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PEA
{
	class Program
	{
		static void Main (string[] args)
		{
			TspGraph graph = new TspGraph();
			char primaryMenuKey, secondaryMenuKey;
			string input;

			do
			{
				Console.WriteLine ();
				Console.WriteLine ("PEA - problem komiwojażera");
				Console.WriteLine ("-Menu główne");
				Console.WriteLine ("--1. Wczytaj graf z pliku.");
				Console.WriteLine ("--2. Przejrzyj właściwości grafu.");
				Console.WriteLine ("--3. Rozwiąż problem za pomocą wybranego algorytmu.");
				Console.WriteLine ("--4. Stwórz zestaw o określonej ilości miast.");
				Console.WriteLine ("--5. Przeprowadź testy czasowe.");

				primaryMenuKey = Console.ReadKey().KeyChar;

				switch (primaryMenuKey)
				{
					case '1':
						Console.Clear ();
						Console.WriteLine ("Wpisz ścieżkę pliku.");
						string filename = Console.ReadLine();
						graph.ReadGraphFromFile(filename);
						break;
					case '2':
						Console.Clear ();
						Console.WriteLine ("---Graf");
						Console.WriteLine ("----Nazwa: " + graph.Name);
						Console.WriteLine ("----Typ: " + graph.Type);
						Console.WriteLine ("----Wymiary: " + graph.Dimension);
						graph.PrintGraph();
						Console.ReadKey();
						break;
					case '3':
						do
						{
							Console.Clear ();
							Console.WriteLine("---Algorytmy");
							Console.WriteLine("----1. Rozwiąż algorytmem programowania dynamicznego.");

							secondaryMenuKey = Console.ReadKey().KeyChar;

							switch (secondaryMenuKey)
							{
								//ten case i tak nie sluzy do testow, wiec pozwolilem sobie
								//na taka drobnostke jak wypisywanie kropek podczas czekania na wyniki
								case '1':
									Console.Clear ();
									CancellationTokenSource cts = new CancellationTokenSource();
									CancellationToken ct = cts.Token;
									Task.Run(new Action(() => {
										while (!cts.IsCancellationRequested)
										{
											Console.Write(".");
											Thread.Sleep(1000);
										}
									}), ct);
									TspDynamicProgramming.SolveTsp(graph);
									cts.Cancel();
									Console.WriteLine();
									TspDynamicProgramming.ShowResults();
									TspDynamicProgramming.ClearCollections();
									Console.ReadKey();
									break;
								default:
									break;
							}
						} while (secondaryMenuKey != 27); 
						break;
					case '4':
						Console.Clear();
						//string input;
						int dim, maxDist;
						Console.WriteLine("---Podaj ilość miast:");
						input = Console.ReadLine();

						while (!Int32.TryParse(input, out dim))
						{
							input = Console.ReadLine();
						}

						Console.WriteLine("---Równe odległości A-B i B-A? (t - tak, n - nie)");
						
						do
						{
							input = Console.ReadLine() ?? "";
						} while (!(input.Equals("t") || input.Equals("n")));

						bool tspOrAtsp = false;

						if (input.Equals("t")) tspOrAtsp = false;
						else if (input.Equals("n")) tspOrAtsp = true;

						Console.WriteLine("---Podaj maksymalną odległość między miastami:");
						input = Console.ReadLine();

						while (!Int32.TryParse(input, out maxDist))
						{
							input = Console.ReadLine();
						}

						graph = new TspGraph(dim, tspOrAtsp, maxDist);
						Console.WriteLine("---Utworzono graf o następujących właściwościach:");
						Console.WriteLine("----Nazwa: " + graph.Name);
						Console.WriteLine("----Typ: " + graph.Type);
						Console.WriteLine("----Wymiary: " + graph.Dimension);
						Console.ReadKey();
						break;
					case '5':
						Console.Clear();
						Console.WriteLine("Zostaną przprowadzone testy. Wciśnij ESC aby anulować albo dowolny klawisz aby kontynuować.");
						secondaryMenuKey = Console.ReadKey().KeyChar;
						if (secondaryMenuKey != 27)
						{
							Console.WriteLine("Wykonywane są testy dla tsp. Może to zająć dużo czasu.");
							double[,] results = DpTests(graph, false);
							TimesToFile("DpTsp.txt", GetAverageTimes(results));
							Console.WriteLine("Wykonywane są testy dla atsp. Może to zająć dużo czasu.");
							results = DpTests(graph, true);
							TimesToFile("DpAtsp.txt", GetAverageTimes(results));
							Console.WriteLine("Koniec testów. Wciśnij dowolny klawisz aby wrócić do menu.");
							Console.ReadKey();
						}
						break;
					default:
						//Console.WriteLine ("Nieprawidłowy klawisz. Wybierz jedną z opcji.");
						break;
				}
			} while (primaryMenuKey != 27);
		}

		private static double[,] DpTests(TspGraph graph, bool tspOrAtsp)
		{
			double[,] timesArray = new double[6,10];
			int numbOfCities = 14;

			for (int i = 0; i < timesArray.GetLength(0); i++, numbOfCities += 2)
			{
				for (int j = 0; j < timesArray.GetLength(1); j++)
				{
					graph = new TspGraph(numbOfCities, tspOrAtsp);
					TspDynamicProgramming.SolveTsp(graph);
					timesArray[i, j] = TspDynamicProgramming.TimeMeasured.TotalMilliseconds;
					TspDynamicProgramming.ClearCollections();
				}
			}

			return timesArray;
		}

		private static double[] GetAverageTimes(double[,] times)
		{
			double[] avgTimesArray = new double[times.GetLength(0)];
			double temp = 0;

			for (int i = 0; i < times.GetLength(0); i++)
			{
				for (int j = 0; j < times.GetLength(1); j++)
				{
					temp += times[i, j];
				}

				temp = temp / times.GetLength(1);
				avgTimesArray[i] = temp;
				temp = 0;
			}

			return avgTimesArray;
		}

		private static void TimesToFile(string filename, double[] times)
		{
			string dir = AppDomain.CurrentDomain.BaseDirectory + @"\Times\";
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			StreamWriter file = new StreamWriter(dir + filename);

			file.WriteLine("Czasy [ms]:");

			foreach (double time in times)
			{
				file.WriteLine(time.ToString("F3"));
			}

			file.Close();
		}
	}//class
}//namespace