using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.Loader;
using static Lab5_ZPO.MainWindow;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace Lab5_ZPO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EnsureDatabaseCreated();
            this.DataContext = this;
            LoadContacts();
        }

        private void EnsureDatabaseCreated()
        {
            using (var context = new ContactsContext())
            {
                context.Database.EnsureCreated(); // sprawdza i tworzy bazę, jeśli nie istnieje
            }
        }

        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();
        public class ContactsContext : DbContext
        {
            public DbSet<Contact> Contacts { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=contacts.db");
            }
        }

        // konstruktor i wyświetlanie elementów na liście
        public class Contact
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string? Surname { get; set; }
            public int? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? Trait { get; set; }

            public override string ToString()
            {
                return $"• {Name} {Surname} ({(string.IsNullOrEmpty(Trait) ? "Brak kategorii" : Trait)})\n    Numer: {PhoneNumber}\n    E-mail: {(string.IsNullOrEmpty(Email) ? "nie podano" : Email)}";
            }
        }

        private void LoadContacts()
        {
            Contacts.Clear();

            using (var context = new ContactsContext())
            {
                var contactsFromDb = context.Contacts.ToList();

                foreach (var contact in contactsFromDb)
                {
                    Contacts.Add(contact);
                }
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var addContactWindow = new Window1();
            addContactWindow.Owner = this;
            addContactWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addContactWindow.ContactAdded += LoadContacts;
            addContactWindow.ShowDialog();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (phoneBookListBox.SelectedItem != null)
            {
                var result = MessageBox.Show("Usunąć wybrany kontakt?", "Usuwanie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var test = phoneBookListBox.SelectedIndex;
                    var selectedContact = (Contact)phoneBookListBox.SelectedItem;

                    Contacts.Remove(selectedContact);

                    using (var context = new ContactsContext())
                    {
                        var contactToDelete = context.Contacts.FirstOrDefault(c => c.Id == selectedContact.Id);

                        if (contactToDelete != null)
                        {
                            context.Contacts.Remove(contactToDelete);
                            context.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Nie znaleziono kontaktu w bazie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Wybierz kontakt do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {

            if (phoneBookListBox.SelectedItem is Contact selectedContact)
            {
                var editContactWindow = new Window1(selectedContact) 
                { 
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                editContactWindow.ContactAdded += LoadContacts;
                editContactWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Wybierz kontakt który chcesz edytować.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool sortAscending = true;
        private void sortButton_Click(object sender, RoutedEventArgs e)
        {
            // ?if :else
            var sorted = sortAscending
                ? Contacts.OrderBy(c => c.Name).ToList()
                : Contacts.OrderByDescending(c => c.Name).ToList();

            Contacts.Clear();
            foreach (var c in sorted)
            {
                Contacts.Add(c);
            }
            sortAscending = !sortAscending;
        }

        private bool filtersApplied = false;
        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            var filterContactsWindow = new Window2 { Owner = this };
            filterContactsWindow.WindowStartupLocation= WindowStartupLocation.CenterOwner;

            if (filterContactsWindow.ShowDialog() == true)
            {
                var filterContacts = filterContactsWindow.SelectedFilter;

                using (var context = new ContactsContext())
                {
                    var query = context.Contacts.AsQueryable();

                    if (filterContacts.HasSurname == true)
                        query = query.Where(c => !string.IsNullOrEmpty(c.Surname));
                    if (filterContacts.HasEmail == true)
                        query = query.Where(c => !string.IsNullOrEmpty(c.Email));
                    if (!string.IsNullOrEmpty(filterContacts.Trait))
                        query = query.Where(c => c.Trait == filterContacts.Trait);

                    filtersApplied = true;
                    clearFiltersButton.Visibility = Visibility.Visible;

                    Contacts.Clear();
                    foreach (var contact in query.ToList())
                    {
                        Contacts.Add(contact);
                    }
                }
            }
        }

        private void deleteAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Czy na pewno chcesz usunąć wszystkie kontakty?", "Usuwanie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var context = new ContactsContext())
                {
                    context.Contacts.RemoveRange(context.Contacts);
                    context.SaveChanges();
                }

                LoadContacts();

                MessageBox.Show("Wszystkie kontakty zostały usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void cancelFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            filtersApplied = false;
            clearFiltersButton.Visibility = Visibility.Collapsed;
            LoadContacts();
        }
    }
}