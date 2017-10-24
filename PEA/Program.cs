using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEA
{
	class Program
	{
		static void Main (string[] args)
		{
			TspGraph graph = new TspGraph();
			char primaryMenuKey, secondaryMenuKey;
			do
			{
				Console.WriteLine ();
				Console.WriteLine ("PEA - problem komiwojażera");
				Console.WriteLine ("-Menu główne");
				Console.WriteLine ("--1. Wczytaj graf z pliku.");
				Console.WriteLine ("--2. Przejrzyj właściwości grafu.");
				Console.WriteLine ("--3. Rozwiąż problem za pomocą wybranego algorytmu.");

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
								case '1':
									Console.Clear ();
									TspDynamicProgramming.SolveTsp(graph);
									TspDynamicProgramming.ShowResults();
									break;
								default:
									break;
							}
						} while (secondaryMenuKey != 27); 
						break;
					default:
						Console.WriteLine ("Nieprawidłowy klawisz. Wybierz jedną z opcji.");
						break;
				}
			} while (primaryMenuKey != 27);
		}
	}
}
