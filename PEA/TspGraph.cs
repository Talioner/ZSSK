using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEA
{
	class TspGraph
	{
		private string edgeWeightFormat = "";
		public string Name { get; private set; }
		public string Type { get; private set; }
		public int Dimension { get; private set; }
		public Int16[][] GraphMatrix { get; private set; }

		public TspGraph ()
		{
			Name = null;
			Type = null;
			Dimension = 0;
			GraphMatrix = null;
		}

		public TspGraph(int dim, bool tspOrAtsp, int maxDist = 100, string name = "default")
		{
			Dimension = dim;
			Name = name;
			Type = !tspOrAtsp ? "TSP" : "ATSP";
			GraphMatrix = new Int16[Dimension][];

			for (int i = 0; i < Dimension; i++)
				GraphMatrix[i] = new Int16[Dimension];

			Random rand = new Random();

			if (!tspOrAtsp)
			{
				for (int i = 0; i < Dimension; i++)
				{
					for (int j = 0; j <= i; j++)
					{
						if (i == j)
							GraphMatrix[i][j] = 0;
						else
						{
							GraphMatrix[i][j] = (Int16)rand.Next(1, maxDist);
							GraphMatrix[j][i] = GraphMatrix[i][j];
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < Dimension; i++)
				{
					for (int j = 0; j < Dimension; j++)
						GraphMatrix[i][j] = (Int16)rand.Next(1, maxDist);
				}
			}
		}

		public void PrintGraph ()
		{
			for (int i = 0; i < Dimension; i++)
			{
				for (int j = 0; j < Dimension; j++)
				{
					Console.Write(GraphMatrix[i][j] + "\t");
				}
				Console.WriteLine();
			}
		}

		public void ReadGraphFromFile (string filename)
		{
			if (File.Exists (filename))
			{
				List<int> intsFromFile = new List<int>();
				StreamReader file = new StreamReader(filename);
				string lineToSplit;
				string[] splitLine;

				while ((lineToSplit = file.ReadLine ()) != String.IsInterned("EDGE_WEIGHT_SECTION"))
				{					
					splitLine = lineToSplit.Split(':');
					if (splitLine[0].ToUpper().Equals("NAME"))
						Name = splitLine[1].Trim(' ');
					else if (splitLine[0].ToUpper ().Equals ("TYPE"))
						Type = splitLine[1].Trim (' ');
					else if (splitLine[0].ToUpper ().Equals ("DIMENSION"))
						Dimension = int.Parse(splitLine[1].Trim (' '));
					else if (splitLine[0].ToUpper().Equals("EDGE_WEIGHT_FORMAT"))
						edgeWeightFormat = splitLine[1].Trim(' ');
				}

				while ((lineToSplit = file.ReadLine ()) != String.IsInterned ("EOF") && !file.EndOfStream)
				{
					splitLine = lineToSplit.Split(' ');
					foreach (string val in splitLine)
					{
						try
						{
							if (!String.IsNullOrWhiteSpace(val) && !String.IsNullOrEmpty(val))
								intsFromFile.Add(Int16.Parse(val));
						}
						catch (FormatException)
						{
							Console.WriteLine("Zły format danych.");
							throw;
						}
					}
				}

				GraphMatrix = new Int16[Dimension][];

				for (int i = 0; i < Dimension; i++)
					GraphMatrix[i] = new Int16[Dimension];

				if (edgeWeightFormat.ToUpper().Equals("FULL_MATRIX"))
				{
					int k = 0;

					for (int i = 0; i < Dimension; i++)
					{
						for (int j = 0; j < Dimension; j++, k++)
						{
							if (intsFromFile[k] > Int16.MaxValue) intsFromFile[k] = 0;
							GraphMatrix[i][j] = (Int16)intsFromFile[k];
						}							
					}
				}
				else if (edgeWeightFormat.ToUpper().Equals("UPPER_ROW"))
				{
					int k = 0;

					for (int i = 0; i < Dimension; i++)
					{
						for (int j = i; j < Dimension; j++)
						{
							if (i == j)
								GraphMatrix[i][j] = 0;
							else
							{
								if (intsFromFile[k] > Int16.MaxValue) intsFromFile[k] = 0;
								GraphMatrix[i][j] = (Int16)intsFromFile[k];
								GraphMatrix[j][i] = GraphMatrix[i][j];
								k++;
							}
						}
					}
				}
				else if (edgeWeightFormat.ToUpper().Equals("LOWER_ROW"))
				{
					int k = 0;

					for (int i = 0; i < Dimension; i++)
					{
						for (int j = 0; j <= i; j++)
						{
							if (i == j)
								GraphMatrix[i][j] = 0;
							else
							{
								if (intsFromFile[k] > Int16.MaxValue) intsFromFile[k] = 0;
								GraphMatrix[i][j] = (Int16)intsFromFile[k];
								GraphMatrix[j][i] = GraphMatrix[i][j];
								k++;
							}
						}
					}
				}
				else if (edgeWeightFormat.ToUpper().Equals("UPPER_DIAG_ROW"))
				{
					int k = 0;

					for (int i = 0; i < Dimension; i++)
					{
						for (int j = i; j < Dimension; j++, k++)
						{
							if (intsFromFile[k] > Int16.MaxValue) intsFromFile[k] = 0;
							GraphMatrix[i][j] = (Int16)intsFromFile[k];
							GraphMatrix[j][i] = GraphMatrix[i][j];
						}
					}
				}
				else if (edgeWeightFormat.ToUpper().Equals("LOWER_DIAG_ROW"))
				{
					int k = 0;

					for (int i = 0; i < Dimension; i++)
					{
						for (int j = 0; j <= i; j++, k++)
						{
							if (intsFromFile[k] > Int16.MaxValue) intsFromFile[k] = 0;
							GraphMatrix[i][j] = (Int16)intsFromFile[k];
							GraphMatrix[j][i] = GraphMatrix[i][j];
						}
					}
				}
				else
				{
					Console.WriteLine("Brak deklaracji formatu danych wejściowych grafu, lub typ nieobsługiwany.");
					Console.WriteLine("Obsługiwane formaty:");
					Console.Write("FULL_MATRIX\nUPPER_ROW\nLOWER_ROW\nUPPER_DIAG_ROW\nLOWER_DIAG_ROW\n");
				}

				file.Close();
			}
			else
			{
				Console.WriteLine("Nie odnaleziono pliku w ścieżce " + filename);
			}
		}
	}//class
}//namespace