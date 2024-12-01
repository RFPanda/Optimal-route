using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// Khasanov Ainur | Solved 01.12.2024 for KAI | khasanov.000.pe 
class TransportTaskByKhasanovA
{
    public static void Main(string[] args)
    {
        string inputFile = "in.txt";  // Входной файл с данными
        string outputFile = "out.txt"; // Выходной файл для результатов
        // Чтение входных данных из файла
        var input = File.ReadAllLines(inputFile);
        var sizes = input[0].Split().Select(int.Parse).ToArray();
        int n = sizes[0]; // Количество поставщиков
        int m = sizes[1]; // Количество потребителей
        // Чтение запасов поставщиков и потребностей потребителей
        var supplies = input[1].Split().Select(int.Parse).ToArray();  // Чтение запасов
        var demands = input[2].Split().Select(int.Parse).ToArray(); // Чтение потребностей 
        // Чтение стоимости перевозки
        var costs = new int[n, m];
        for (int i = 0; i < n; i++)
        {
            var row = input[3 + i].Split().Select(int.Parse).ToArray();
            for (int j = 0; j < m; j++)
            {
                costs[i, j] = row[j];
            }
        }
        var DaBydetReshenye = VogelMethodbyKhasanovA(n, m, supplies, demands, costs); // Методом Фогеля
        File.WriteAllLines(outputFile, DaBydetReshenye.Select(row => string.Join(" ", row)));// Записываем результат в файл
    }

    /*
    Метод Фогеля состоит в вычислении для каждой строки транспортной таблицы разницы между двумя наименьшими тарифами. 
    Аналогичное действие выполняют для каждого столбца этой таблицы. 
    Наибольшая разница между двумя минимальными тарифами соответствует наиболее предпочтительной строке или столбцу 
    (если есть несколько строк или столбцов с одинаковой разницей, то выбор между ними произволен). 
    В пределах этой строки или столбца отыскивают ячейку с минимальным тарифом, куда пишут отгрузку. 
    Строки поставщиков или столбцы потребителей, которые полностью исчерпали свои возможности по отгрузке или потребности которых в товаре были удовлетворены, вычеркиваются из таблицы 
    (в примерах ниже они закрашиваются серым цветом), и вычисление повторяются до полного удовлетворения спроса и исчерпания отгрузок без учета вычеркнутых («серых») ячеек.
    Штраф — разница между минимальными тарифами в строке или столбце, который помогает выбрать, куда следует сделать следующую отгрузку, чтобы минимизировать общие затраты.
    */

    private static int[][] VogelMethodbyKhasanovA(int n, int m, int[] supplies, int[] demands, int[,] costs) // Методом Фогеля
    {
        int[][] solution = new int[n][]; // массив для решения задачи
        for (int i = 0; i < n; i++)
        {
            solution[i] = new int[m];
        }
        int totalCost = 0; // Суммарная стоимость перевозок totalCost
        // Параллельный расчет штрафов для строк и столбцов
        while (supplies.Sum() > 0 && demands.Sum() > 0)
        {
            var rowAndColumnPenalties = new (int Penalty, int RowOrCol, bool IsRow)[n + m]; // Массив, который будет содержать штрафы для строк и столбцов
            // Параллельный расчет штрафов для строк и столбцов
            Parallel.For(0, n + m, k =>
            {
                if (k < n && supplies[k] > 0) // Штраф для строк
                {
                    // Выбираем минимальные и вторые минимальные значения стоимости для строки
                    var rowCosts = Enumerable.Range(0, m)
                        .Where(j => demands[j] > 0) // Только для тех, где есть спрос
                        .Select(j => costs[k, j])
                        .OrderBy(x => x)
                        .ToArray();
                    rowAndColumnPenalties[k] = (rowCosts.Length > 1 ? rowCosts[1] - rowCosts[0] : rowCosts[0], k, true);// Вычисляем разницу между минимальными тарифами
                }
                else if (k >= n && demands[k - n] > 0) // Штраф для столбцов
                {
                    var colCosts = Enumerable.Range(0, n)
                        .Where(i => supplies[i] > 0) // Только для тех, где есть поставки
                        .Select(i => costs[i, k - n])
                        .OrderBy(x => x)
                        .ToArray();
                    // Вычисляем разницу
                    rowAndColumnPenalties[k] = (colCosts.Length > 1 ? colCosts[1] - colCosts[0] : colCosts[0], k - n, false);
                }
            });
            // Выбираем максимальный штраф для дальнейшего распределения
            var maxPenalty = rowAndColumnPenalties.Where(p => p.Penalty >= 0).OrderByDescending(p => p.Penalty).First();
            int row = maxPenalty.IsRow ? maxPenalty.RowOrCol : -1;
            int col = maxPenalty.IsRow ? -1 : maxPenalty.RowOrCol;
            // Выбираем минимальную стоимость для строки или столбца с максимальным штрафом
            if (maxPenalty.IsRow)
            {
                col = Enumerable.Range(0, m)
                    .Where(j => demands[j] > 0) // Только для потребителей, где есть спрос
                    .OrderBy(j => costs[row, j]) // Сортируем по стоимости
                    .First();
            }
            else
            {
                row = Enumerable.Range(0, n)
                    .Where(i => supplies[i] > 0) // Только для поставщиков, где есть поставки
                    .OrderBy(i => costs[i, col]) // Сортируем по стоимости
                    .First();
            }
            int amount = Math.Min(supplies[row], demands[col]); // Распределяем минимальное количество товара
            solution[row][col] = amount; // Обновляем решение
            totalCost += amount * costs[row, col]; // Добавляем к суммарной стоимости
            supplies[row] -= amount; // Уменьшаем запас поставщика
            demands[col] -= amount; // Уменьшаем потребность потребителя
        }
        // Результат
        var DaBydetReshenye = new int[n + 1][];
        DaBydetReshenye[0] = new[] { totalCost };
        for (int i = 0; i < n; i++)
        {
            DaBydetReshenye[i + 1] = solution[i];
        }
        return DaBydetReshenye;
    }
}