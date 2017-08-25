using System;
using System.Windows;
using System.Windows.Controls;

namespace NodeNet.GUI.View
{
    public partial class AdnCalc
    {
        public AdnCalc()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button b = (Button) sender;
            Tb.Text += b.Content.ToString();
        }

        private void Result_click(object sender, RoutedEventArgs e)
        {
            try
            {
                Result();
            }
            catch (Exception)
            {
                Tb.Text = "Error!";
            }
        }

        private void Result()
        {
            string op;
            int iOp = 0;
            if (Tb.Text.Contains("+"))
            {
                iOp = Tb.Text.IndexOf("+", StringComparison.Ordinal);
            }
            else if (Tb.Text.Contains("-"))
            {
                iOp = Tb.Text.IndexOf("-", StringComparison.Ordinal);
            }
            else if (Tb.Text.Contains("*"))
            {
                iOp = Tb.Text.IndexOf("*", StringComparison.Ordinal);
            }
            else if (Tb.Text.Contains("/"))
            {
                iOp = Tb.Text.IndexOf("/", StringComparison.Ordinal);
            }
            
            op = Tb.Text.Substring(iOp, 1);
            double op1 = Convert.ToDouble(Tb.Text.Substring(0, iOp));
            double op2 = Convert.ToDouble(Tb.Text.Substring(iOp + 1, Tb.Text.Length - iOp - 1));

            switch (op)
            {
                case "+":
                    Tb.Text += "=" + (op1 + op2);
                    break;
                case "-":
                    Tb.Text += "=" + (op1 - op2);
                    break;
                case "*":
                    Tb.Text += "=" + (op1 * op2);
                    break;
                default:
                    Tb.Text += "=" + (op1 / op2);
                    break;
            }
        }

        private void Off_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            Tb.Text = "";
        }

        private void R_Click(object sender, RoutedEventArgs e)
        {
            if (Tb.Text.Length > 0)
            {
                Tb.Text = Tb.Text.Substring(0, Tb.Text.Length - 1);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
