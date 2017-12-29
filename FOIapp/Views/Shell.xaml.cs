using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FOIapp.Classes;
using FOIapp.Views.Pages;
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
        public ObservableCollection<NavigationViewItem> MainMenuItems = new ObservableCollection<NavigationViewItem>();
        public List<Course> CurrentUserCourses = new List<Course>();
        public Course CurrentCourse;
        public string currentUserName = "";
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();

        public MainPage()
        {
            InitializeComponent();
            
            var items = MainNavView.MenuItems.ToList();

            foreach (var item in items)
            {
                MainMenuItems.Add(item as NavigationViewItem);
            }

            // Bind and populate menu
            MainNavView.MenuItemsSource = MainMenuItems;

            MainNavView.SelectedItem = MainMenuItems[1];
            MainMenuItems[1].IsSelected = true;
            ContentFrame.Navigate(typeof(Settings));

            // Load user

            LoadUser(LocalStorage.Read<int>("currentUserId"));

            // Set window sizing options

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(600, 675));
            ApplicationView.PreferredLaunchViewSize = new Size(1040, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

        }

        public void LoadUser(int userId)
        {
            UserLoadingRing.IsActive = true;
            MainNavView.IsEnabled = false;

            new Task(async () =>
            {
                var currentUser = new User();

                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();
                    var query = $"SELECT Name, Surname, Email FROM Users WHERE UserID = {userId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentUser.Name = reader.GetString(0);
                                currentUser.Surname = reader.GetString(1);
                                currentUser.Email = reader.GetString(2);
                            }
                        }
                    }

                    query = $"SELECT Courses.CourseID, Courses.Name, ShortName, StudyYear, StudyName, CourseColor  FROM Courses JOIN UserCourses ON Courses.CourseID = UserCourses.CourseID WHERE UserID = {userId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CurrentUserCourses.Add(new Course
                                {
                                    CourseID = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ShortName = reader.GetString(2),
                                    StudyYear = reader.GetInt32(3),
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

                    CurrentUserName.Text = currentUser.FullName.Length > 0 ? currentUser.FullName : "Baza nedostupna";

                    if (CurrentUserCourses.Count > 0)
                    {
                        foreach (var course in CurrentUserCourses.OrderBy(n => n.Name))
                        {
                            AddNewMenuItem(course.Name, (Symbol)0xE82D, $"course{course.CourseID}");

                        }
                    }

                    NavMenuUserIconTooltip.Content = currentUser.FullName;

                    MainNavView.IsEnabled = true;

                });
            }).Start();
        }


        private void AddNewMenuItem(string Title, Symbol Icon, string Tag, bool BottomMargin = false)
        {
            string name;

            if (Title.Length > 30)
            {
                name = Title.Substring(0, 30);
                name += "...";
            }
            else
            {
                name = Title;
            }

            var menuItem = new NavigationViewItem
            {
                Content = name,
                Icon = new SymbolIcon(Icon),
                Tag = Tag
            };

            var toolTip = new ToolTip
            {
                Content = Title
            };
            ToolTipService.SetToolTip(menuItem, toolTip);

            if (BottomMargin)
            {
                menuItem.Margin = new Thickness(0, 0, 0, 8);
            }

            MainMenuItems.Add(menuItem);
        }

        private void NavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

            var item = args.SelectedItem as NavigationViewItem;

            switch (item.Tag)
            {
                case "UserTile":
                    ContentFrame.Navigate(typeof(Settings));
                    break;
                case "ProfessorList":
                    ContentFrame.Navigate(typeof(Professors));
                    break;
                default:
                    foreach (var course in CurrentUserCourses)
                    {
                        if ($"course{course.CourseID}" != item.Tag.ToString()) continue;
                        LocalStorage.Save("currentCourseName", course.Name);
                        LocalStorage.Save("currentCourseID", course.CourseID);
                    }

                    ContentFrame.Navigate(typeof(Views.Pages.Course));

                    break;

            }
        }

    }
}
