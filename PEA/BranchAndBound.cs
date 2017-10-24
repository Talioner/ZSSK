using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEA
{
	class BranchAndBound
	{
		private int _row, _column;

		private int FindMinInRow (int[][] cities, int row)
		{
			int min = cities[row][0];

			for (int i = 0; i < cities[row].Length; i++)
			{
				if (cities[row][i] < min)
					min = cities[row][i];
			}

			return min;
		}

		private int FindMinInColumn (int[][] cities, int column)
		{
			int min = cities[0][column];

			for (int i = 0; i < cities.Length; i++)
			{
				if (cities[i][column] < min)
					min = cities[i][column];
			}

			return min;
		}

		private void ReduceRow (int[][] cities)
		{
			int minRow;

			_row = 0;

			for (int i = 0; i < cities.Length; i++)
			{
				minRow = FindMinInRow(cities, i);
				if (minRow != int.MaxValue)
					_row += minRow;

				for (int j = 0; j < cities[i].Length; j++)
				{
					if (cities[i][j] != int.MaxValue)
						cities[i][j] -= minRow;
				}
			}
		}

		private void ReduceColumn (int[][] cities)
		{
			int minColumn;

			_column = 0;

			for (int j = 0; j < cities.Length; j++)
			{
				minColumn = FindMinInColumn(cities, j);
				if (minColumn != int.MaxValue)
					_column += minColumn;

				for (int i = 0; i < cities.Length; i++)
				{
					if (cities[i][j] != int.MaxValue)
						cities[i][j] -= minColumn;
				}
			}
		}
	}//class
}//namespace