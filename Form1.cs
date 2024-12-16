using OxyPlot.Axes;
using OxyPlot.WindowsForms;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Modeling6
{
    public partial class Form1 : Form
    {
        public Label[,] labels; // Массив для хранения ссылок на Label

        public Form1()
        {
            InitializeComponent();
            InitializeLabels();
            FillLabels(LoadDataForTable());
        }

        private void InitializeLabels()
        {
            labels = new Label[5, 5]
            {
                { label_11, label_12, label_13, label_14, label_15 },
                { label_21, label_22, label_23, label_24, label_25 },
                { label_31, label_32, label_33, label_34, label_35 },
                { label_41, label_42, label_43, label_44, label_45 },
                { label_51, label_52, label_53, label_54, label_55 }
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private List<List<int>> LoadDataForTable()
        {
            List<List<int>> data = new List<List<int>>();
            try
            {
                using (StreamReader reader = new StreamReader("input.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        List<int> row = new List<int>();
                        string[] values = line.Split(',');

                        foreach (string value in values)
                        {
                            if (int.TryParse(value, out int number))
                            {
                                row.Add(number);
                            }
                        }

                        data.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            }

            return data;
        }

        private void FillLabels(List<List<int>> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    if (i < labels.GetLength(0) && j < labels.GetLength(1))
                    {
                        labels[i, j].Text = data[i][j].ToString();
                    }
                }
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            // Получаем значения из полей ввода
            double step;
            double time;

            // Проверяем, что вводимые значения являются числами
            if (double.TryParse(textBoxStep.Text, out step) && double.TryParse(textBoxTime.Text, out time))
            {
                Markov markov = new Markov(this);
                string result = markov.RunSimulation(step, time);
                richTextBox.AppendText(result + Environment.NewLine);
                SetupPlotView();
                List<DataPoint> dataPoints = LoadDataFromCSV(); // Получение данных для графика по модели
                LoadDataIntoPlot(dataPoints); // Загружаем данные на график
            }
            else
            {
                // Если ввод некорректный, выводим сообщение об ошибке
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для шага и времени.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupPlotView()
        {
            // Создаем новую модель
            plotView.Model = new PlotModel();

            // Настраиваем оси
            plotView.Model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Время",
                Minimum = 0,
                Maximum = 5
            });
            plotView.Model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Вероятность",
                Minimum = 0,
                Maximum = 1
            });

        }

        private void LoadDataIntoPlot(List<DataPoint> data)
        {
            try
            {

                if (data.Count > 0)
                {
                    plotView.Model.Series.Clear(); // Очищаем предыдущие серии

                    // Добавляем линии для каждого S, используя OxyPlot.DataPoint
                    plotView.Model.Series.Add(CreateLineSeries("S1", data.Select(dp => new OxyPlot.DataPoint(dp.Time, dp.S1))));
                    plotView.Model.Series.Add(CreateLineSeries("S2", data.Select(dp => new OxyPlot.DataPoint(dp.Time, dp.S2))));
                    plotView.Model.Series.Add(CreateLineSeries("S3", data.Select(dp => new OxyPlot.DataPoint(dp.Time, dp.S3))));
                    plotView.Model.Series.Add(CreateLineSeries("S4", data.Select(dp => new OxyPlot.DataPoint(dp.Time, dp.S4))));
                    plotView.Model.Series.Add(CreateLineSeries("S5", data.Select(dp => new OxyPlot.DataPoint(dp.Time, dp.S5))));
                    plotView.Invalidate(); // Обновляем график
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}"); // Вывод информации об ошибке в консоль
            }
        }

        // Метод для создания серии данных для графика
        private LineSeries CreateLineSeries(string title, IEnumerable<OxyPlot.DataPoint> points)
        {
            var series = new LineSeries
            {
                Title = title,
                StrokeThickness = 2,
                MarkerType = MarkerType.None
            };

            foreach (var point in points)
            {
                series.Points.Add(point);
            }

            return series;
        }

        private List<DataPoint> LoadDataFromCSV()
        {
            string[] lines = File.ReadAllLines("output.csv");
            var data = new List<DataPoint>();

            // Пропускаем строку заголовка
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(' ');
                if (values.Length >= 6)
                {
                    double time = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture); // Время
                    double S1 = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    double S2 = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    double S3 = double.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
                    double S4 = double.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    double S5 = double.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);

                    data.Add(new DataPoint(time, S1, S2, S3, S4, S5));
                }
            }

            return data; // Возвращаем список загруженных данных
        }


        // Класс для хранения данных остается без изменений
        public class DataPoint
        {
            public double Time { get; set; }
            public double S1 { get; set; }
            public double S2 { get; set; }
            public double S3 { get; set; }
            public double S4 { get; set; }
            public double S5 { get; set; }

            public DataPoint(double time, double s1, double s2, double s3, double s4, double s5)
            {
                Time = time;
                S1 = s1;
                S2 = s2;
                S3 = s3;
                S4 = s4;
                S5 = s5;
            }
        }
    }
}