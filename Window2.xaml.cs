using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lab5_ZPO
{
    /// <summary>
    /// Logika interakcji dla klasy Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public ContactFilter SelectedFilter { get; private set; }

        public Window2()
        {
            InitializeComponent();
        }

        public class ContactFilter
        {
            public bool? HasSurname { get; set; }    // true = ma, false = nie ma, null = ignoruj
            public bool? HasEmail { get; set; }
            public string Trait { get; set; }        // null = nie filtruj po trait
        }

        private void applyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // odczytaj wartości z checkboxów
            bool? hasSurname = nazwiskoCheckbox.IsChecked == true ? true : (bool?)null;
            bool? hasEmail = emailCheckBox.IsChecked == true ? true : (bool?)null;

            // jeśli nie ma wybranego traitu to nie filtrujemy po nim
            var selectedTraitItem = filterTraitsComboBox.SelectedItem as ComboBoxItem;
            string selectedTrait = selectedTraitItem?.Content?.ToString();

            SelectedFilter = new ContactFilter
            {
                HasSurname = hasSurname,
                HasEmail = hasEmail,
                Trait = selectedTrait
            };

            this.DialogResult = true;
            this.Close();
        }

        private void cancelFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
