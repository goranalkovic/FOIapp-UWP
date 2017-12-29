using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FOIapp.Classes;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FOIapp.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Professors : Page
    {
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();
        public ObservableCollection<Teacher> Teachers = new ObservableCollection<Teacher>(); 
        public ObservableCollection<string> TeacherNames = new ObservableCollection<string>();
        private int _selectedIndex;

        public Professors()
        {
            InitializeComponent();

            LoadData();
        }

        public void LoadData()
        {
            LoadingRing.IsActive = true;

            new Task(async () =>
            {
                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Teachers;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var t = new Teacher
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Surname = reader.GetString(2),
                                    Title = reader.GetString(3),
                                    IsTitleAfterName = reader.GetBoolean(4),
                                    Email = reader.GetString(5),
                                    ImageUrl = reader.GetString(6),
                                    OfficeLocation = reader.GetString(7),
                                    OfficeNumber = reader.GetString(8)
                                };

                                Teachers.Add(t);
                            }

                            

                        }
                    }

                    foreach (var t in Teachers)
                    {
                        query = $"SELECT ConsultationTime, ConsultationDay, IsFoi2 FROM TeacherConsultations WHERE TeacherID = {t.Id};";

                        t.Consultations = new List<TeacherConsultations>();

                        using (var command = new SqlCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    var cons = new TeacherConsultations()
                                    {
                                        Day = reader.GetInt32(1),
                                        Time = reader.GetString(0),
                                        IsFoi2 = reader.GetBoolean(2)
                                    };

                                    t.Consultations.Add(cons);
                                }

                                

                            }
                        }
                    }

                    connection.Close();
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    TeachersCarousel.ItemsSource = Teachers;

                    LoadingRing.IsActive = false;

                    foreach (var teacher in Teachers)
                    {
                        TeacherNames.Add(teacher.FullName);
                    }

                    ProfessorSearch.ItemsSource = TeacherNames;
                });
            }).Start();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dataObj = TeachersCarousel.SelectedItem as Teacher;

            await Windows.System.Launcher.LaunchUriAsync(new Uri(dataObj.MailToLink));
        }

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var filteredList = new ObservableCollection<string>();

            foreach (var t in TeacherNames)
            {
                if (t.ToLower().Contains(sender.Text.ToLower()))
                {
                    filteredList.Add(t);
                }
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = filteredList;

            }
        }

        private void SearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _selectedIndex = TeacherNames.IndexOf(args.SelectedItem.ToString());
        }

        private async void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await NoResultsText.Offset(150, duration: 0).StartAsync();

            if (args.ChosenSuggestion != null)
            {
                TeachersCarousel.SelectedIndex = _selectedIndex;

                ProfessorSearch.Text = "";
                ProfessorSearch.BorderThickness = new Thickness(0);
                ProfessorSearch.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                ProfessorSearch.Text = "";
                await NoResultsText.Fade(1.0F).Offset().StartAsync();
                await NoResultsText.Fade(delay: 900).Offset(150, delay:900).StartAsync();
            }
        }
    }
}