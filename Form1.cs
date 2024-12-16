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
            }
            else
            {
                // Если ввод некорректный, выводим сообщение об ошибке
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для шага и времени.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
