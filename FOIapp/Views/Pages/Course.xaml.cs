using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FOIapp.Classes;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;


namespace FOIapp.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Course : Page
    {
        public ObservableCollection<CourseCategory> CourseCategories = new ObservableCollection<CourseCategory>();
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();
        public ObservableCollection<CourseItem> TempItems = new ObservableCollection<CourseItem>();
        public ObservableCollection<AbsenceItem> TempAbsenceItems = new ObservableCollection<AbsenceItem>();
        public int CurrentCourseId;
        public int CurrentUserId;
        public string tooManyAbsencesTxt;


        public Course()
        {
            InitializeComponent();
            LoadInfoForCourse();

        }

        public void LoadInfoForCourse()
        {
            CurrentUserId = LocalStorage.Read<int>("currentUserId");
            CurrentCourseId = LocalStorage.Read<int>("currentCourseID");
            ActiveCourseName.Text = LocalStorage.Read<string>("currentCourseName");

            new Task(async () =>
            {
                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
                using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
                {
                    connection.Open();
                    var query = $"SELECT DISTINCT CourseItems.CategoryID, CourseCategories.CategoryName FROM CourseItems JOIN UserPoints ON CourseItems.CourseItemID = UserPoints.CourseItemID, Courses, CourseCategories WHERE Courses.CourseID = CourseItems.CourseID AND CourseCategories.CategoryID = CourseItems.CategoryID AND Courses.CourseID = {CurrentCourseId};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CourseCategories.Add(new CourseCategory
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1)
                                });
                            }
                        }
                    }

                    query = $"SELECT CourseItems.CourseItemID, CourseID, CategoryID, Name, CourseItems.Points, MinPoints, UserPoints.Points FROM UserPoints JOIN CourseItems ON UserPoints.CourseItemID = CourseItems.CourseItemID WHERE CourseID = {CurrentCourseId} AND UserPoints.UserID = {CurrentUserId};";

                    TempAbsenceItems.Clear();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TempItems.Add(new CourseItem
                                {
                                    CourseItemID = reader.GetInt32(0),
                                    CourseID = reader.GetInt32(1),
                                    CategoryID = reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    Points = reader.GetDouble(4),
                                    MinPoints = reader.GetDouble(5),
                                    CurrentPoints = reader.GetDouble(6)
                                });
                            }
                        }
                    }

                    query = $"SELECT UserAbsences.TimesAbsent, AbsenceCategories.CategoryID, AbsenceCategories.CategoryName, AbsenceItems.MaxTimesAbsent FROM UserAbsences JOIN AbsenceCategories ON UserAbsences.AbsenceCategoryID = AbsenceCategories.CategoryID, AbsenceItems WHERE UserAbsences.UserID = {CurrentUserId} AND UserAbsences.CourseID = {CurrentCourseId} AND AbsenceItems.CourseID = UserAbsences.CourseID AND AbsenceItems.AbsenceCategoryID = UserAbsences.AbsenceCategoryID;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TempAbsenceItems.Add(new AbsenceItem()
                                {
                                    AbsenceCategoryID = reader.GetInt32(1),
                                    AbsenceCategory = reader.GetString(2),
                                    MaxTimesAbsent = reader.GetInt32(3),
                                    TimesAbsent = reader.GetInt32(0)
                                });
                            }
                        }
                    }

                    connection.Close();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CourseCategoryPoints.ItemsSource = CourseCategories;

                    if (TempAbsenceItems.Count == 0)
                    {
                        DontHaveToComeLabel.Visibility = Visibility.Visible;
                        AbsencesList.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DontHaveToComeLabel.Visibility = Visibility.Collapsed;
                        AbsencesList.Visibility = Visibility.Visible;

                        AbsencesList.ItemsSource = TempAbsenceItems;
                    }

                    foreach (var item in CourseCategories)
                    {
                        foreach (var catItem in TempItems)
                        {
                            if (catItem.CategoryID == item.CategoryID)
                            {
                                item.ChildItems.Add(catItem);
                            }
                        }
                    }
                });
            }).Start();

        }

        private void AbsencePlus_OnClick(object sender, RoutedEventArgs e)
        {
            // Get object

            var button = sender as Button;

            var dataObject = button.DataContext as AbsenceItem;

            var index = TempAbsenceItems.IndexOf(dataObject);

            // Show warning if at max allowed

            if (TempAbsenceItems[index].TimesAbsent + 1 == dataObject.MaxTimesAbsent)
            {
                BeforeTooManyNotifTxt.Text = "Ne smiješ više izostati\n";

                switch (dataObject.AbsenceCategory)
                {
                    case "Predavanja":
                        BeforeTooManyNotifTxt.Text += "s predavanja";
                        break;
                    case "Seminari":
                        BeforeTooManyNotifTxt.Text += "sa seminara";
                        break;
                    case "Laboratorijske vježbe":
                        BeforeTooManyNotifTxt.Text += "s laboratorijskih vježbi";
                        break;
                    case "Dolasci na TZK":
                        BeforeTooManyNotifTxt.Text += "s TZK";
                        break;
                }

                var anim1 = AbsencesList.Blur(5, 200);
                var anim2 = AbsencesList.Blur(0, 200);

                BeforeTooManyAbsencesNotification.Closing += async (o, args) =>
                {
                    await anim2.StartAsync();
                    anim1.Stop();
                    AbsencesList.IsHitTestVisible = true;
                };

                BeforeTooManyAbsencesNotification.Opening += async (o, args) =>
                {
                    AbsencesList.IsHitTestVisible = false;
                    await anim1.StartAsync();
                };

                BeforeTooManyAbsencesNotification.Show(1500);
            }

            // Show warning if at more than allowed

            if (TempAbsenceItems[index].TimesAbsent + 1 > dataObject.MaxTimesAbsent)
            {
                TooManyNotifTxt.Text = "Imaš previše izostanaka\n";

                switch (dataObject.AbsenceCategory)
                {
                    case "Predavanja":
                        TooManyNotifTxt.Text += "s predavanja";
                        break;
                    case "Seminari":
                        TooManyNotifTxt.Text += "sa seminara";
                        break;
                    case "Laboratorijske vježbe":
                        TooManyNotifTxt.Text += "s laboratorijskih vježbi";
                        break;
                    case "Dolasci na TZK":
                        TooManyNotifTxt.Text += "s TZK";
                        break;
                }
                
                var anim1 = AbsencesList.Blur(5);
                var anim2 = AbsencesList.Blur();

                TooManyAbsencesNotification.Closing += async (o, args) =>
                {
                    await anim2.StartAsync();
                    anim1.Stop();
                    AbsencesList.IsHitTestVisible = true;
                };

                TooManyAbsencesNotification.Opening += async (o, args) =>
                {
                    AbsencesList.IsHitTestVisible = false;
                    await anim1.StartAsync();
                };

                TooManyAbsencesNotification.Show(2000);

            }

            // Update database

            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();

                var query =
                    $"UPDATE UserAbsences SET TimesAbsent += 1 WHERE UserID = {CurrentUserId} AND CourseID = {CurrentCourseId} AND AbsenceCategoryID = {dataObject.AbsenceCategoryID};";

                var command = new SqlCommand(query, connection);
                using (command.ExecuteReader()) { }

                connection.Close();

            }

            // Update view

            TempAbsenceItems[index].TimesAbsent += 1;


        }

        private void AbsenceMinus_OnClick(object sender, RoutedEventArgs e)
        {
            // Get object

            var button = sender as Button;

            var dataObject = button.DataContext as AbsenceItem;

            var index = TempAbsenceItems.IndexOf(dataObject);

            // Show warning if at 0

            if (TempAbsenceItems[index].TimesAbsent - 1 < 0)
            {
                var anim1 = AbsencesList.Blur(5);
                var anim2 = AbsencesList.Blur();

                AbsenceNotification.Closing += async (o, args) =>
                {
                    await anim2.StartAsync();
                    anim1.Stop();
                    AbsencesList.IsHitTestVisible = true;
                };

                AbsenceNotification.Opening += async (o, args) =>
                {
                    AbsencesList.IsHitTestVisible = false;
                    await anim1.StartAsync();
                };

                AbsenceNotification.Show(2000);


                return;
            }

            // Update database

            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();

                var query =
                    $"UPDATE UserAbsences SET TimesAbsent -= 1 WHERE UserID = {CurrentUserId} AND CourseID = {CurrentCourseId} AND AbsenceCategoryID = {dataObject.AbsenceCategoryID};";

                var command = new SqlCommand(query, connection);
                using (command.ExecuteReader()) { }

                connection.Close();

            }

            // Update view

            TempAbsenceItems[index].TimesAbsent -= 1;

            

        }
    }
}
