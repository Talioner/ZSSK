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
			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken ct = cts.Token;

			do
			{
				Console.WriteLine ();
				Console.WriteLine ("PEA - problem komiwojażera");
				Console.WriteLine ("-Menu główne");
				Console.WriteLine ("--1. Wczytaj graf z pliku.");
				Console.WriteLine ("--2. Przejrzyj właściwości grafu.");
				Console.WriteLine ("--3. Rozwiąż problem za pomocą wybranego algorytmu.");
				Console.WriteLine ("--4. Stwórz zestaw o określonej ilości miast.");
				Console.WriteLine ("--5. Przeprowadź testy czasowe dla losowych instancji.");
				Console.WriteLine ("--6. Przeprowadź testy czasowe dla danych z tsplib.");

				primaryMenuKey = Console.ReadKey().KeyChar;

				switch (primaryMenuKey)
				{
					case '1':
						Console.Clear ();
						Console.WriteLine ("Wpisz ścieżkę pliku.");
						string filename = Console.ReadLine();
						filename = filename?.Replace(@"\", @"\\");
						graph.ReadGraphFromFile(filename);
						break;
					case '2':
						Console.Clear ();
						try
						{
							Console.WriteLine("---Graf");
							Console.WriteLine("----Nazwa: " + graph.Name);
							Console.WriteLine("----Typ: " + graph.Type);
							Console.WriteLine("----Wymiary: " + graph.Dimension);
							graph.PrintGraph();
						}
						catch (NullReferenceException)
						{
							Console.WriteLine("Graf albo jego właściwości są null.");
						}
						Console.ReadKey();
						break;
					case '3':
						do
						{
							Console.Clear ();
							Console.WriteLine("---Algorytmy");
							Console.WriteLine("----1. Rozwiąż algorytmem programowania dynamicznego.");
							Console.WriteLine("----2. Rozwiąż algorytmem przeszukiwania z zakazami.");

							secondaryMenuKey = Console.ReadKey().KeyChar;

							switch (secondaryMenuKey)
							{
								case '1':
									cts = new CancellationTokenSource();
									Console.Clear ();
									Task.Run(new Action(() => {
										while (!cts.IsCancellationRequested)
										{
											Console.Write(".");
											Thread.Sleep(1000);
										}
									}), ct);
									TspDynamicProgramming.SolveTsp(graph);
									cts.Cancel();
									cts.Dispose();
									Console.WriteLine();
									TspDynamicProgramming.ShowResults();			
									TspDynamicProgramming.ClearCollections();
									Console.ReadKey();
									break;
								case '2':
									Console.Clear();
									int mi, mtfb, pt, tlc;
									Console.WriteLine("---Podaj ilość iteracji:");
									input = Console.ReadLine();

									while (!Int32.TryParse(input, out mi))
									{
										input = Console.ReadLine();
									}

									Console.WriteLine("---Podaj ilość prób dla jednej gałęzi:");
									input = Console.ReadLine();

									while (!Int32.TryParse(input, out mtfb))
									{
										input = Console.ReadLine();
									}

									Console.WriteLine("---Podaj ilość prób bez polepszenia rezultatu:");
									input = Console.ReadLine();

									while (!Int32.TryParse(input, out pt))
									{
										input = Console.ReadLine();
									}

									Console.WriteLine("---Podaj długość listy tabu:");
									input = Console.ReadLine();

									while (!Int32.TryParse(input, out tlc))
									{
										input = Console.ReadLine();
									}
									cts = new CancellationTokenSource();
									Console.Clear();
									Task.Run(new Action(() => {
										while (!cts.IsCancellationRequested)
										{
											Console.Write(".");
											Thread.Sleep(1000);
										}
									}), ct);
									TspTabuSearch.SolveTsp(graph, mi, mtfb, pt, tlc);
									cts.Cancel();
									cts.Dispose();
									Console.WriteLine();
									TspTabuSearch.ShowResults();
									Console.ReadKey();
									break;
								default:
									break;
							}
						} while (secondaryMenuKey != 27); 
						break;
					case '4':
						Console.Clear();
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
					case '6':
						Console.Clear();
						Console.WriteLine("Zostaną przprowadzone testy dla danych z tsplib. Wciśnij ESC aby anulować albo dowolny klawisz aby kontynuować.");
						secondaryMenuKey = Console.ReadKey().KeyChar;
						if (secondaryMenuKey != 27)
						{
							Console.WriteLine("Wykonywane są testy. Może to zająć dużo czasu.");
							double[,] results = DpTestsTspLib(graph);
							TimesToFile("DpTspLib.txt", GetAverageTimes(results));
							Console.WriteLine("Koniec testów. Wciśnij dowolny klawisz aby wrócić do menu.");
							Console.ReadKey();
						}
						break;
					default:
						break;
				}
			} while (primaryMenuKey != 27);
		}

		private static double[,] DpTests(TspGraph graph, bool tspOrAtsp)
		{
			double[,] timesArray = new double[6,100];
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
		//testy dla wybranych instancji z tsplib umieszczonych w folderze ./tsplib
		private static double[,] DpTestsTspLib(TspGraph graph)
		{
			double[,] timesArray = new double[4, 100];
			int i = 0;

			graph = new TspGraph();
			graph.ReadGraphFromFile(AppDomain.CurrentDomain.BaseDirectory + @"\tsplib\gr17.tsp");

			for (int j = 0; j < timesArray.GetLength(1); j++)
			{
				TspDynamicProgramming.SolveTsp(graph);
				timesArray[i, j] = TspDynamicProgramming.TimeMeasured.TotalMilliseconds;
				TspDynamicProgramming.ClearCollections();
			}
			i++;

			graph = new TspGraph();
			graph.ReadGraphFromFile(AppDomain.CurrentDomain.BaseDirectory + @"\tsplib\gr21.tsp");

			for (int j = 0; j < timesArray.GetLength(1); j++)
			{
				TspDynamicProgramming.SolveTsp(graph);
				timesArray[i, j] = TspDynamicProgramming.TimeMeasured.TotalMilliseconds;
				TspDynamicProgramming.ClearCollections();
			}
			i++;

			graph = new TspGraph();
			graph.ReadGraphFromFile(AppDomain.CurrentDomain.BaseDirectory + @"\tsplib\gr24.tsp");

			for (int j = 0; j < timesArray.GetLength(1); j++)
			{
				TspDynamicProgramming.SolveTsp(graph);
				timesArray[i, j] = TspDynamicProgramming.TimeMeasured.TotalMilliseconds;
				TspDynamicProgramming.ClearCollections();
			}
			i++;

			graph = new TspGraph();
			graph.ReadGraphFromFile(AppDomain.CurrentDomain.BaseDirectory + @"\tsplib\br17.atsp");

			for (int j = 0; j < timesArray.GetLength(1); j++)
			{
				TspDynamicProgramming.SolveTsp(graph);
				timesArray[i, j] = TspDynamicProgramming.TimeMeasured.TotalMilliseconds;
				TspDynamicProgramming.ClearCollections();
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