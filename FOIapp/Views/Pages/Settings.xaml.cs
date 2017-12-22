using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FOIapp.Classes;
using Microsoft.Toolkit.Uwp.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FOIapp.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private int currentUserId;
        public Settings()
        {
            InitializeComponent();
            
            LoadCurrentUserInfo();
        }


        public void LoadCurrentUserInfo()
        {
            UserLoadingRing.IsActive = true;
            ShowNotificationsToggle.IsEnabled = false;

            // Get local storage data
            var localStorage = new LocalObjectStorageHelper();

            currentUserId = localStorage.Read<int>("currentUserId");

            // Query the database

            new Task(async () =>
            {
                var currentUser = new User();
                bool cuNotifications;

                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
                using (var connection = new SqlConnection("Data Source=.;Initial Catalog=bp2-db;Integrated Security=True"))
                {
                    connection.Open();
                    var query = $"SELECT Name, Surname, Email, ShowNotifications FROM Users INNER JOIN UserSettings ON Users.UserID = UserSettings.UserID WHERE Users.UserID = {currentUserId};";
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();

                            currentUser.Name = reader.GetString(0);
                            currentUser.Surname = reader.GetString(1);
                            currentUser.Email = reader.GetString(2);
                            cuNotifications = reader.GetBoolean(3);
                        }
                    }

                    connection.Close();
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        UserLoadingRing.IsActive = false;

                        CurrentUserEmail.Text = currentUser.Email.Length > 0 ? currentUser.Email : "ERR";
                        CurrentUserName.Text = currentUser.FullName.Length > 0 ? currentUser.FullName : "ERR";

                        ShowNotificationsToggle.IsOn = cuNotifications;
                        ShowNotificationsToggle.IsEnabled = true;
                    });
            }).Start();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var availableUsers = new List<User>();


                
                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
                using (var connection = new SqlConnection("Data Source=.;Initial Catalog=bp2-db;Integrated Security=True"))
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
   

            UserSelectList.ItemsSource = availableUsers;

            await UserPicker.ShowAsync();
        }

        private void ShowNotificationsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsLoadingRing.IsActive = true;
            ShowNotificationsToggle.IsEnabled = false;

            var settingValue = ShowNotificationsToggle.IsOn ? 1 : 0;


                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
            using (var connection = new SqlConnection("Data Source=.;Initial Catalog=bp2-db;Integrated Security=True"))
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
        }

        private void UserPicker_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var localStorage = new LocalObjectStorageHelper();

            var id = (UserSelectList.SelectedItem as User).UserID;

            localStorage.Save("currentUserId", id);

            LoadCurrentUserInfo();

            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));

        }
    }
}
