using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Modeling6
{
    class Transition
    {
        public int To { get; set; }
        public double Rate { get; set; }
    }

    class Markov
    {
        private Form1 form; // Ссылка на основную форму
        private const int N = 5; // Количество состояний
        private List<List<Transition>> graph; // Граф переходов

        public Markov(Form1 form)
        {
            this.form = form;
            graph = new List<List<Transition>>(N);
            for (int i = 0; i < N; i++)
            {
                graph.Add(new List<Transition>());
            }
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            // Очищаем граф перед инициализацией
            graph.Clear();
            for (int i = 0; i < N; i++)
            {
                graph.Add(new List<Transition>());
            }

            // Считывание значений из таблицы
            double[,] rates = new double[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    // Получаем значение из соответствующего Label
                    var labelText = form.labels[i, j].Text;
                    if (double.TryParse(labelText, out double rate))
                    {
                        rates[i, j] = rate;
                    }
                }
            }

            // Инициализация переходов на основе считанных значений
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (i != j && rates[i, j] > 0) // Делаем переход только если ставка > 0
                    {
                        graph[i].Add(new Transition { To = j, Rate = rates[i, j] });
                    }
                }
            }
        }

        // Функция для построения матрицы переходных интенсивностей
        private double[,] BuildTransitionMatrix()
        {
            double[,] L = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                foreach (var trans in graph[i])
                {
                    L[i, trans.To] = trans.Rate;
                }
            }
            return L;
        }

        // Функция для построения матрицы коэффициентов системы dP/dt = L P
        private double[,] BuildKolmogorovSystem(double[,] L)
        {
            double[,] A = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                A[i, i] = 0.0;
                for (int j = 0; j < N; j++)
                {
                    if (i != j)
                    {
                        A[i, j] = L[j, i]; // Входящие переходы
                        A[i, i] -= L[i, j]; // Выходящие переходы
                    }
                }
            }
            return A;
        }

        // Метод Эйлера для численного решения системы ОДУ
        public List<double[]> EulerMethod(double[,] L, double[] P0, double dt, double t_max)
        {
            int steps = (int)(t_max / dt);
            var solutions = new List<double[]>();
            double[] P = (double[])P0.Clone();
            solutions.Add((double[])P.Clone());

            for (int step = 0; step <= steps; step++) //!!!
            {
                double[] dP = new double[N];

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (i != j)
                        {
                            dP[i] -= L[i, j] * P[i]; // Исходящие вероятность
                            dP[i] += L[j, i] * P[j];   // Входящие вероятность
                        }
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    P[i] += dP[i] * dt;
                    if (P[i] < 0) P[i] = 0;
                    if (P[i] > 1) P[i] = 1;
                }

                solutions.Add((double[])P.Clone());
            }

            return solutions;
        }

        // Запись результатов в CSV файл
        public void WriteToCSV(List<double[]> solutions, double dt, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Time,S1,S2,S3,S4,S5"); // Заголовок
                for (int i = 0; i < solutions.Count; i++)
                {
                    writer.Write(i * dt);
                    foreach (var prob in solutions[i])
                    {
                        writer.Write("," + Math.Round(prob, 5).ToString(CultureInfo.InvariantCulture)); // Используйте CultureInfo для корректного формата
                    }
                    writer.WriteLine();
                }
            }
        }


        private string MatrixToString(double[,] matrix, int decimalPlaces)
        {
            var rows = new List<string>();
            string formatString = "F" + decimalPlaces; // Формат строки, например, "F1" для одного знака после запятой

            for (int i = 0; i < N; i++)
            {
                var columns = new List<string>();
                for (int j = 0; j < N; j++)
                {
                    columns.Add(Math.Round(matrix[i, j], decimalPlaces).ToString(formatString).Replace(',', '.'));
                }
                rows.Add(string.Join("\t", columns));
            }
            return string.Join("\n", rows);
        }


        private void ModifyGraphForNonErgodicity()
        {
            // Удаляем все переходы из первого состояния
            graph[0].Clear(); // S1 не имеет исходящих переходов
        }

        private bool Gauss(double[,] a, double[] x)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            for (int i = 0; i < n; i++)
            {
                // Поиск максимального элемента в столбце
                double maxEl = Math.Abs(a[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(a[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(a[k, i]);
                        maxRow = k;
                    }
                }

                // Перестановка строк
                for (int k = i; k < m; k++)
                {
                    double temp = a[maxRow, k];
                    a[maxRow, k] = a[i, k];
                    a[i, k] = temp;
                }

                // Проверка на ноль
                if (Math.Abs(a[i, i]) < 1e-12)
                {
                    return false; // Система не имеет единственного решения
                }

                // Приведение к треугольному виду
                for (int k = i + 1; k < n; k++)
                {
                    double c = -a[k, i] / a[i, i];
                    for (int j = i; j < m; j++)
                    {
                        a[k, j] += c * a[i, j];
                    }
                }
            }

            // Обратная подстановка
            for (int i = n - 1; i >= 0; i--)
            {
                if (Math.Abs(a[i, i]) < 1e-12)
                {
                    return false; // Система не имеет единственного решения
                }
                x[i] = a[i, m - 1] / a[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    a[k, m - 1] -= a[k, i] * x[i];
                }
            }
            return true;
        }

        private bool FindSteadyState(double[,] A, out double[] steadyState)
        {
            steadyState = new double[N];
            double[,] augmented = new double[N, N + 1];

            // Копируем первые (N-1) уравнений Колмогорова
            for (int i = 0; i < N - 1; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    augmented[i, j] = A[i, j];
                }
                augmented[i, N] = 0.0; // Правая часть = 0
            }

            // Последнее уравнение — условие нормировки
            for (int j = 0; j < N; j++)
            {
                augmented[N - 1, j] = 1.0;
            }
            augmented[N - 1, N] = 1.0; // Правая часть = 1

            // Решение системы
            return Gauss(augmented, steadyState);
        }

        private bool IsStronglyConnected(List<List<Transition>> graph)
        {
            bool bfs(int start, bool[] visited)
            {
                Queue<int> q = new Queue<int>();
                q.Enqueue(start);
                visited[start] = true;
                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    foreach (var trans in graph[u])
                    {
                        if (!visited[trans.To])
                        {
                            visited[trans.To] = true;
                            q.Enqueue(trans.To);
                        }
                    }
                }
                return !visited.Contains(false); // Проверка на охват всех узлов
            }

            // Проверка для каждого узла
            for (int i = 0; i < N; i++)
            {
                bool[] visited = new bool[N];
                if (!bfs(i, visited)) return false;
            }
            return true;
        }

        public string RunSimulation(double dt, double t_max)
        {
            // Построение первоначальных матриц
            double[,] L = BuildTransitionMatrix();
            double[,] A = BuildKolmogorovSystem(L);
            double[] P0 = { 1.0, 0.0, 0.0, 0.0, 0.0 }; // Начальные условия

            // Численное решение
            var solutions = EulerMethod(L, P0, dt, t_max);

            string LMatrixString = "Матрица переходных интенсивностей L:\n" + MatrixToString(L, 1); // Изменено
            string AMatrixString = "\nСистема Колмогорова (матрица A):\n" + MatrixToString(A, 1); // Изменено

            // Запись результатов в CSV файл
            WriteToCSV(solutions, dt, "output.csv");
            MessageBox.Show("Результаты численного решения записаны в файл output.csv", "Успех");

            // Нахождение предельных вероятностей
            string results = LMatrixString + AMatrixString + "\n";
            if (FindSteadyState(A, out var steadyState))
            {
                var steadyStateString = "\nПредельные вероятности:\n" +
                                         string.Join("\n", steadyState.Select((p, index) => $"P{index + 1} = {p:F5}"));
                results += steadyStateString + "\n";
            }
            else
            {
                results += "\nОшибка. \nНе удалось найти предельные вероятности (система не имеет единственного решения)\n";
            }

            // Проверка эргодичности
            bool ergodic = IsStronglyConnected(graph);
            results += $"\nГраф {(ergodic ? "эргодичен." : "неэргодичен.")}\n";

            // Изменяем граф для демонстрации неэргодичности
            ModifyGraphForNonErgodicity();

            // Пересчет новых матриц
            double[,] newL = BuildTransitionMatrix();
            double[,] newA = BuildKolmogorovSystem(newL);

            // Формирование нового вывода
            results += "\nИзменяем граф для неэргодичности (удаляем связи из S1).\nНовый граф неэргодичен.\n";
            results += "Новая матрица переходных интенсивностей L:\n" + MatrixToString(newL, 1); // Изменено
            results += "\nНовая система Колмогорова (матрица A):\n" + MatrixToString(newA, 1); // Изменено

            // Нахождение новых предельных вероятностей
            if (FindSteadyState(newA, out var newSteadyState))
            {
                var newSteadyStateString = "\nНовые предельные вероятности:\n" +
                                            string.Join("\n", newSteadyState.Select((p, index) => $"P{index + 1} = {Math.Abs(p):F5}")); // Изменено: добавлено Math.Abs
                results += newSteadyStateString + "\n";
            }
            else
            {
                results += "\nОшибка. \nНе удалось найти новые предельные вероятности (система не имеет единственного решения).\n";
            }

            return results;
        }

    }
}
