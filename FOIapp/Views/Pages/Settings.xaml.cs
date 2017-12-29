using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using FOIapp.Classes;
using Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;


namespace FOIapp.Views.Pages
{
    public sealed partial class Settings : Page
    {
        private int currentUserId;
        private int noOfUsers;
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();
        private List<Classes.Course> AllCourses = new List<Classes.Course>();

        public Settings()
        {
            InitializeComponent();

            LoadCurrentUserInfo();
        }

        public async void LoadCurrentUserInfo()
        {
            UserLoadingRing.IsActive = true;
            ShowNotificationsToggle.IsEnabled = false;

            // Get local storage data

            currentUserId = LocalStorage.Read<int>("currentUserId");

            ThemeToggle.IsOn = LocalStorage.Read<bool>("uiTheme");

            // Set theme

            var rootFrame = Window.Current.Content as Frame;

            rootFrame.RequestedTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;

            // Extend into title bar

            FluentDesign.SetTransparentTitleBar(LocalStorage.Read<bool>("uiTheme")
                ? Colors.White
                : Color.FromArgb(255, 40, 40, 40));

            // Make the switch toggleable

            ThemeToggle.Toggled += ChangeUiThemeAndSave;

            // Query the database

            new Task(async () =>
            {
                var currentUser = new User()
                {
                    UserID = currentUserId
                };
                //bool cuNotifications;

                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();
                    var query =
                        $"SELECT Name, Surname, Email, ShowNotifications FROM Users INNER JOIN UserSettings ON Users.UserID = UserSettings.UserID WHERE Users.UserID = {currentUserId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            try
                            {
                                while (reader.Read())
                                {
                                    currentUser.Name = reader.GetString(0);
                                    currentUser.Surname = reader.GetString(1);
                                    currentUser.Email = reader.GetString(2);
                                    //cuNotifications = reader.GetBoolean(3);
                                }
                            }
                            catch (Exception e)
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                                {
                                    await new MessageDialog($"{e.Message}").ShowAsync();

                                    ThemeToggle.IsOn = true;

                                    NoUserHero.Visibility = Visibility.Visible;
                                    currentUserId = -1;
                                });
                                return;
                            }
                        }
                    }

                    connection.Close();
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UserLoadingRing.IsActive = false;

                    CurrentUserEmail.Text = currentUser.Email.Length > 0 ? currentUser.Email : "ERR";
                    CurrentUserName.Text = currentUser.FullName.Length > 0 ? currentUser.FullName : "ERR";

                    //ShowNotificationsToggle.IsOn = cuNotifications;
                    //ShowNotificationsToggle.IsEnabled = true;
                });

            }).Start();

            new Task(async () =>
            {
                AllCourses.Clear();

                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();

                    var query =
                        $"SELECT Courses.CourseID, Courses.Name, ShortName, StudyYear, StudyName, CourseColor  FROM Courses JOIN UserCourses ON Courses.CourseID = UserCourses.CourseID WHERE UserID = {currentUserId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AllCourses.Add(new Classes.Course
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

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UserLoadingRing.IsActive = false;

                    if (AllCourses.Count > 0)
                    {
                        CourseListView.ItemsSource = AllCourses.OrderBy(n => n.Name);
                    }
                    else
                    {
                        NoCoursesHero.Visibility = Visibility.Visible;
                        AddCourseTopButton.Visibility = Visibility.Collapsed;
                    }
                });
            }).Start();


            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();
                const string query = "SELECT COUNT(*) FROM Users;";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            noOfUsers = reader.GetInt32(0);
                        }
                    }
                }
                connection.Close();
            }

            SwitchActiveUser.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;
            SwitchActiveUserSeparator.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;

        }





        private void ShowNotificationsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsLoadingRing.IsActive = true;
            ShowNotificationsToggle.IsEnabled = false;

            var settingValue = ShowNotificationsToggle.IsOn ? 1 : 0;

            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();
                var query =
                    $"UPDATE UserSettings SET ShowNotifications = {settingValue} WHERE UserID = {currentUserId};";

                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteReader();
                }

                connection.Close();
            }

            SettingsLoadingRing.IsActive = false;
            ShowNotificationsToggle.IsEnabled = true;

        }

        private void UserSelectList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserPicker.IsPrimaryButtonEnabled = true;
            UserPicker.IsSecondaryButtonEnabled = true;
        }

        private void UserPicker_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            var id = (UserSelectList.SelectedItem as User).UserID;

            LocalStorage.Save("currentUserId", id);

            LoadCurrentUserInfo();

            var rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(MainPage));

        }


        private void ChangeUiThemeAndSave(object sender, RoutedEventArgs routedEventArgs)
        {
            LocalStorage.Save("uiTheme", ThemeToggle.IsOn);

            var rootFrame = Window.Current.Content as Frame;

            rootFrame.RequestedTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;

            FluentDesign.SetTransparentTitleBar(LocalStorage.Read<bool>("uiTheme")
                ? Colors.White
                : Color.FromArgb(255, 40, 40, 40));
        }


        private async void DeleteCourse(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var dataObj = btn.DataContext as Classes.Course;

            var CourseDeleteDialog = new ContentDialog
            {
                Title = "🙋‍♀️ Upozorenje",
                Content = "Nećeš moći vratiti podatke o kolegiju!",
                PrimaryButtonText = "Obriši",
                CloseButtonText = "Odustani",
                DefaultButton = ContentDialogButton.Primary,
                Background = (SolidColorBrush)Application.Current.Resources["PageBackground"],
                RequestedTheme = LocalStorage.Read<bool>("uiTheme") ? ElementTheme.Dark : ElementTheme.Light,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold
            };

            var res = await CourseDeleteDialog.ShowAsync();

            if (res == ContentDialogResult.Primary)
            {
                // Update database

                CourseListView.IsEnabled = false;

                using (var connection = new SqlConnection(FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();
                    var query =
                        $"DELETE FROM UserCourses WHERE UserID = {currentUserId} AND CourseID = {dataObj.CourseID};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteReader();
                    }

                    connection.Close();
                }

                CourseListView.IsEnabled = true;

                // Refresh page

                var rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private void AddCourseButtonPressed(object sender, RoutedEventArgs e)
        {
            LoadingProgress.IsActive = true;
            AllCourses.Clear();

            new Task(async () =>
            {
                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();

                    var query =
                        $"SELECT Courses.CourseID, Courses.Name, ShortName, StudyYear, StudyName, CourseColor FROM Courses EXCEPT SELECT Courses.CourseID, Courses.Name, ShortName, StudyYear, StudyName, CourseColor FROM Courses RIGHT JOIN UserCourses ON Courses.CourseID = UserCourses.CourseID WHERE UserID = {currentUserId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AllCourses.Add(new Classes.Course
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

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                {
                    LoadingProgress.IsActive = false;
                    AllCoursesList.ItemsSource = AllCourses;

                    await AddCourseDialog.ShowAsync();
                });
            }).Start();

        }

        private void AddUserCourseToDb(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();

                var query =
                    $"INSERT INTO UserCourses VALUES ({currentUserId}, {(AllCoursesList.SelectedItem as Classes.Course).CourseID});";

                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteReader();
                }

                connection.Close();
            }

            // Refresh page

            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private async void AddUserToDb(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                using (var connection = new SqlConnection(FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();

                    var query =
                        $"INSERT INTO Users (Name, Surname, Email) VALUES ('{NewUserName.Text}', '{NewUserSurname.Text}', '{NewUserEmail.Text}');";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteReader();
                    }

                    connection.Close();
                }

                noOfUsers++;

                SwitchActiveUser.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;
                SwitchActiveUserSeparator.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;

            }
            finally
            {
                if (currentUserId == -1)
                {
                    NewUserHeroContinueButton.IsEnabled = true;

                    await NewUserHeroAddDialog.Fade().StartAsync();
                    await NewUserHeroIntro.Fade().StartAsync();
                    await NewUserHeroContinueButton.Fade(1).StartAsync();
                }

            }
        }

        private void SearchCourseList(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (CourseSearch.Text.Length < 2)
            {
                AllCoursesList.ItemsSource = AllCourses;
                return;
            }

            var list = (from course in AllCourses
                        where (course.Name.Contains(CourseSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                               course.ShortName.Contains(CourseSearch.Text, StringComparison.OrdinalIgnoreCase))
                        select course).ToList();

            AllCoursesList.ItemsSource = list.Count > 0 ? list : AllCourses;

            AddCourseDialog.IsPrimaryButtonEnabled = false;
        }

        private void AllCoursesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddCourseDialog.IsPrimaryButtonEnabled = true;
        }

        private async void ValidateNewUser(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            // Prepare animations

            var fadeIn = NewUserInfoText.Fade(1.0F, 200D);
            var fadeOut = NewUserInfoText.Fade(0.0F, 100D);

            await fadeOut.StartAsync();

            // Check inputs

            AddUserDialog.IsPrimaryButtonEnabled = false;

            if (NewUserName.Text.Length < 1)
            {
                NewUserInfoText.Text = "❌ Prazno polje (ime)";
                await fadeIn.StartAsync();
                return;
            }

            if (Regex.Replace(NewUserName.Text, "[A-zšđčćžŠĐČĆŽ-]", "").Length != 0)
            {
                NewUserInfoText.Text = "😠 Nedozvoljeni znakovi u imenu";
                await fadeIn.StartAsync();
                return;
            }

            if (NewUserSurname.Text.Length < 1)
            {
                NewUserInfoText.Text = "❌ Prazno polje (prezime)";
                await fadeIn.StartAsync();
                return;

            }

            if (Regex.Replace(NewUserSurname.Text, "[A-zšđčćžŠĐČĆŽ-]", "").Length != 0)
            {
                NewUserInfoText.Text = "😠 Nedozvoljeni znakovi u prezimenu";
                await fadeIn.StartAsync();
                return;
            }

            if (NewUserEmail.Text.Length < 1)
            {
                NewUserInfoText.Text = "❌ Prazno polje (e-mail)";
                await fadeIn.StartAsync();
                return;
            }

            if (Regex.Replace(NewUserEmail.Text, @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", "").Length != 0)
            {
                NewUserInfoText.Text = "😠 Neispravan e-mail";
                await fadeIn.StartAsync();
                return;
            }

            var userList = new List<string>();

            using (var connection = new SqlConnection(FunctionsAndInterfaces.connectionString))
            {
                connection.Open();
                const string query = "SELECT Email FROM Users;";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userList.Add(reader.GetString(0));
                        }

                    }
                }

                connection.Close();
            }

            if (userList.Any(u => u == NewUserEmail.Text))
            {
                AddUserDialog.IsPrimaryButtonEnabled = false;

                NewUserInfoText.Text = "🙆‍♂️ Korisnik s tim e-mailom već postoji";
                await fadeIn.StartAsync();
                return;
            }

            NewUserInfoText.Text = currentUserId == -1 ? "✔ To je to!" : "✔ Seems legit";

            await fadeIn.StartAsync();

            AddUserDialog.IsPrimaryButtonEnabled = true;

        }

        private async void NewUserHeroContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewUserProgressRing.IsActive = true;

            NewUserHeroContinueButton.IsEnabled = false;

            using (var connection = new SqlConnection(FunctionsAndInterfaces.connectionString))
            {
                await connection.OpenAsync();
                const string query = "SELECT TOP(1) UserID FROM Users;";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                currentUserId = reader.GetInt32(0);

                                LocalStorage.Save("currentUserId", reader.GetInt32(0));

                                NewUserProgressRing.IsActive = false;

                                NoUserHero.Visibility = Visibility.Collapsed;

                                var frame = Window.Current.Content as Frame;
                                frame.Navigate(typeof(MainPage));
                            }
                        }
                        catch (Exception ex)
                        {
                            await new MessageDialog($"Dogodila se greška 😟\n\nDetaljnije: {ex.Message}").ShowAsync();
                        }
                    }
                }

                connection.Close();
            }


        }

        private async void ShowUserPicker(object sender, RoutedEventArgs e)
        {
            var availableUsers = new List<User>();

            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();
                var query = $"SELECT UserID, Name, Surname, Email FROM Users WHERE UserID != {currentUserId};";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availableUsers.Add(new User
                            {
                                UserID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Surname = reader.GetString(2),
                                Email = reader.GetString(3)
                            });
                        }

                    }
                }

                connection.Close();
            }

            if (availableUsers.Count < 1)
            {
                currentUserId = -1;
            }
            else
            {
                UserSelectList.ItemsSource = availableUsers;
                await UserPicker.ShowAsync();
            }
        }

        private async void ShowUserAddDialog(object sender, RoutedEventArgs e)
        {
            AddUserDialog.SecondaryButtonText = currentUserId != -1 ? "Odustani" : "";

            NewUserName.Text = "";
            NewUserSurname.Text = "";
            NewUserEmail.Text = "";

            await AddUserDialog.ShowAsync();
        }

        private void RemoveUserFromDb(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var id = (UserSelectList.SelectedItem as User).UserID;

            using (var connection = new SqlConnection(FunctionsAndInterfaces.connectionString))
            {
                connection.Open();

                var query = $"DELETE FROM UserSettings WHERE UserID = {id};";

                using (var command = new SqlCommand(query, connection))
                {
                    using (command.ExecuteReader())
                    {
                    }
                }

                query = $"DELETE FROM Users WHERE UserID = {id};";

                using (var command = new SqlCommand(query, connection))
                {
                    using (command.ExecuteReader())
                    {
                    }
                }

                query = $"DELETE FROM UserCourses WHERE UserID = {id};";

                using (var command = new SqlCommand(query, connection))
                {
                    using (command.ExecuteReader())
                    {
                    }
                }

                connection.Close();
            }



            noOfUsers--;

            SwitchActiveUser.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;
            SwitchActiveUserSeparator.Visibility = noOfUsers > 1 ? Visibility.Visible : Visibility.Collapsed;

        }
    }

}