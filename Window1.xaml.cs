using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Lab5_ZPO.MainWindow;

namespace Lab5_ZPO
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public event Action ContactAdded;

        public Contact EditingContact { get; set; } = new Contact();

        public Window1(Contact contact = null)
        {
            InitializeComponent();

            if (contact == null)
            {
                EditingContact = new Contact();
                Title = "Dodawanie";
                addButtonWindow.Content = "Dodaj";
            }
            else
            {
                EditingContact = new Contact
                {
                    Id = contact.Id,
                    Name = contact.Name,
                    Surname = contact.Surname,
                    PhoneNumber = contact.PhoneNumber,
                    Email = contact.Email,
                    Trait = contact.Trait
                };

                Title = "Edytowanie";
                addButtonWindow.Content = "Zapisz";
            }

            DataContext = EditingContact;
        }

        // regex do numeru telefonu
        private static readonly Regex _regex = new Regex("^[0-9]{0,9}$");

        // regex do walidacji emailu: dowolne znaki muszą być zakończone "@<dowolne litery/cyfry>.<minimum 2 dowolne litery>
        private static readonly Regex email_regex = new Regex(@"^[^@\s]+@[a-zA-Z0-9]+\.[a-zA-Z]{2,}$");

        // tylko cyfry
        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        // nie można wpisać spacji
        private void PhoneNumberTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // nie można wkleić blędnych symboli
        private void PhoneNumberTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (String)e.DataObject.GetData(typeof(String));
                if (!_regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
        }

        private void addButtonWindow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditingContact.Name) || !EditingContact.PhoneNumber.HasValue)
            {
                MessageBox.Show("Wymagane pola: imię i numer telefonu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(EditingContact.Email) && !email_regex.IsMatch(EditingContact.Email))
            {
                MessageBox.Show("Adres e-mail jest niepoprawny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new MainWindow.ContactsContext())
            {
                if (EditingContact.Id == 0)
                    context.Contacts.Add(EditingContact);
                else
                    context.Contacts.Update(EditingContact);

                context.SaveChanges();
            }

            ContactAdded?.Invoke();
            Close();
        }

        private void cancelButtonWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
