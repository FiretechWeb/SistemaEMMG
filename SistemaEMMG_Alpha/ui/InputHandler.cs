using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha.ui
{
    public static class InputHandler
    {
        public static bool OnlyDateTimeText(System.Windows.Input.TextCompositionEventArgs e, TextBox inputTextBox)
        {
            Regex regex = new Regex("[^0-9]+");
            bool validInput = inputTextBox.Text.Length < 10 && !regex.IsMatch(e.Text);
            if (validInput)
            {
                string numbersOnlyText = inputTextBox.Text.Replace("/", "");
                if (numbersOnlyText.Length == 2 || numbersOnlyText.Length == 4)
                {
                    inputTextBox.Text += "/";
                    inputTextBox.CaretIndex = inputTextBox.Text.Length;
                }
                Regex regexFinal = new Regex("[^0-9/]+");
                return regexFinal.IsMatch(e.Text);
            }
            else
            {
                return true;
            }
        }
        public static bool OnlyNumbers(System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            return regex.IsMatch(e.Text);
        }

        public static bool OnlyMoney(System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,.]+");
            return regex.IsMatch(e.Text);
        }
    }
}
