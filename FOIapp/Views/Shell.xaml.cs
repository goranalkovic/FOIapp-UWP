using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FOIapp.Classes;
using FOIapp.Views.Pages;
using Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using Course = FOIapp.Classes.Course;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FOIapp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<NavigationViewItem> mainMenuItems = new ObservableCollection<NavigationViewItem>();
        List<Course> currentUserCourses = new List<Course>();
        private Course currentCourse;
        string currentUserName = "";
        private LocalObjectStorageHelper localStorage = new LocalObjectStorageHelper();

        public MainPage()
        {
            InitializeComponent();

            // Extend into title bar
            FluentDesign.SetTransparentTitleBar(Colors.Black);

            // Bind and populate menu
            MainNavView.MenuItemsSource = mainMenuItems;
            AddDefaultMenuItems();
            MainNavView.SelectedItem = mainMenuItems[0];
            mainMenuItems[0].IsSelected = true;
            ContentFrame.Navigate(typeof(AllCourses));

            // Load user

            var localStorage = new LocalObjectStorageHelper();
            LoadUser(localStorage.Read<int>("currentUserId"));

        }



        public void LoadUser(int userId)
        {
            UserLoadingRing.IsActive = true;

            new Task(async () =>
            {
                var currentUser = new User();
                

                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
                using (var connection = new SqlConnection("Data Source=.;Initial Catalog=bp2-db;Integrated Security=True"))
                {
                    connection.Open();
                    var query = $"SELECT Name, Surname, Email FROM Users WHERE UserID = {userId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();

                            currentUser.Name = reader.GetString(0);
                            currentUser.Surname = reader.GetString(1);
                            currentUser.Email = reader.GetString(2);
                        }
                    }

                    query = $"SELECT Courses.CourseID, Courses.Name, ShortName, StudyYear, StudyName, CourseColor  FROM Courses JOIN UserCourses ON Courses.CourseID = UserCourses.CourseID WHERE UserID = {userId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                currentUserCourses.Add(new Course
                                {
                                    CourseID =  reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ShortName = reader.GetString(2),
                                    StudyYear =  reader.GetInt32(3),
                                    StudyName = reader.GetString(4),
                                    CourseColor = reader.GetString(5)

                                });

                                
                            }

                         
                        }
                    }

                    connection.Close();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UserLoadingRing.IsActive = false;

                    CurrentUserEmail.Text = currentUser.Email.Length > 0 ? currentUser.Email : "Greška";
                    CurrentUserName.Text = currentUser.FullName.Length > 0 ? currentUser.FullName : "Baza nedostupna";

                    if (currentUserCourses.Count > 0)
                    {
                        foreach (var course in currentUserCourses)
                        {
                            AddNewMenuItem(course.Name, (Symbol)0xE82D, $"course{course.CourseID}");
                            
                        }
                    }

                    NavMenuUserIconTooltip.Content = currentUser.FullName;


                });
            }).Start();
        }

        private void AddDefaultMenuItems()
        {
            AddNewMenuItem("Moji kolegiji", (Symbol)0xF0E2, "allCourses");
            AddNewMenuItem("Popis profesora", (Symbol)0xE179, "professors", true);
            //AddNewMenuItem("Kolegij", (Symbol)0xE82D, "course01");

        }

        private void AddNewMenuItem(string Title, Symbol Icon, string Tag, bool BottomMargin = false)
        {
            var menuItem = new NavigationViewItem
            {
                Content = Title,
                Icon = new SymbolIcon(Icon),
                Tag = Tag
            };

            if (BottomMargin)
            {
                menuItem.Margin = new Thickness(0, 0, 0, 8);
            }

            mainMenuItems.Add(menuItem);
        }

        private void NavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Settings));
                //MainNavView.Header = "Postavke";
            }
            else
            {
                var item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "allCourses":
                        ContentFrame.Navigate(typeof(AllCourses));
                        //MainNavView.Header = "Moji kolegiji";
                        break;

                    case "professors":
                        ContentFrame.Navigate(typeof(Professors));
                        //MainNavView.Header = "Popis profesora";
                        break;
                    default:
                        foreach (var course in currentUserCourses)
                        {
                            if ($"course{course.CourseID}" == item.Tag.ToString())
                            {
                                //MainNavView.Header = course.Name;


                                localStorage.Save("currentCourseName", course.Name);
                                localStorage.Save("currentCourseID", course.CourseID);

                            }
                        }

                        //while (localStorage.Read<string>("currentCourseName") == null) { }
                        ContentFrame.Navigate(typeof(Views.Pages.Course));


                        //MainNavView.Header = $"{item.Tag}";
                        break;
                }
            }
        }

    }
}
