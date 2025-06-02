using System.Collections.Generic;

namespace TapEmpire.Utility
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T> GetRow<T>(this T[,] array, int row, int from = 0)
        {
            for (int i = from; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

        public static IEnumerable<T> GetColumn<T>(this T[,] array, int column, int from = 0)
        {
            for (int i = from; i < array.GetLength(0); i++)
            {
                yield return array[i, column];
            }
        }

        public static int FindRowIndex<T>(this T[,] array, int column, System.Func<T, bool> predicate, int from = 0)
        {
            var index = array.GetColumn(column, from).FindIndex(predicate);
            return index < 0 ? index : index + from;
        }

        public static IEnumerable<T> AsStream<T>(this T[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    yield return array[i, j];
                }
            }
        }
    }
}