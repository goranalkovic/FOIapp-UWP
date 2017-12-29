using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using FOIapp.Classes;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;


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
        public string TooManyAbsencesTxt;
        public CourseItem TempCourseItem;
        public bool IsPointInputValid;

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
                    var query =
                        $"SELECT DISTINCT CourseItems.CategoryID, CourseCategories.CategoryName FROM CourseItems JOIN UserPoints ON CourseItems.CourseItemID = UserPoints.CourseItemID, Courses, CourseCategories WHERE Courses.CourseID = CourseItems.CourseID AND CourseCategories.CategoryID = CourseItems.CategoryID AND Courses.CourseID = {CurrentCourseId};";

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

                    query =
                        $"SELECT CourseItems.CourseItemID, CourseID, CategoryID, Name, CourseItems.Points, MinPoints, UserPoints.Points FROM UserPoints JOIN CourseItems ON UserPoints.CourseItemID = CourseItems.CourseItemID WHERE CourseID = {CurrentCourseId} AND UserPoints.UserID = {CurrentUserId};";

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

                    query =
                        $"SELECT UserAbsences.TimesAbsent, AbsenceCategories.CategoryID, AbsenceCategories.CategoryName, AbsenceItems.MaxTimesAbsent FROM UserAbsences JOIN AbsenceCategories ON UserAbsences.AbsenceCategoryID = AbsenceCategories.CategoryID, AbsenceItems WHERE UserAbsences.UserID = {CurrentUserId} AND UserAbsences.CourseID = {CurrentCourseId} AND AbsenceItems.CourseID = UserAbsences.CourseID AND AbsenceItems.AbsenceCategoryID = UserAbsences.AbsenceCategoryID;";

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

                    CalculateTotalPoints();
                });
            }).Start();
        }

        private void CalculateTotalPoints()
        {
            var SumOfPoints = 0.0;

            foreach (var c in CourseCategories)
            {
                foreach (var i in c.ChildItems)
                {
                    SumOfPoints += i.CurrentPoints;
                }
            }

            TotalPoints.Text = $"{SumOfPoints}";

            if (SumOfPoints < 50)
            {
                GradeText.Text = "Nedovoljan (1)";
            }
            else if (SumOfPoints >= 50 && SumOfPoints < 61)
            {
                GradeText.Text = "Dovoljan (2)";
            }
            else if (SumOfPoints >= 51 && SumOfPoints < 76)
            {
                GradeText.Text = "Dobar (3)";
            }
            else if (SumOfPoints >= 76 && SumOfPoints < 91)
            {
                GradeText.Text = "Vrlo dobar (4)";
            }
            else if (SumOfPoints >= 91)
            {
                GradeText.Text = "Izvrstan (5)";
            }
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
                using (command.ExecuteReader())
                {
                }

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
                using (command.ExecuteReader())
                {
                }

                connection.Close();

            }

            // Update view

            TempAbsenceItems[index].TimesAbsent -= 1;



        }

        private async void CourseItemClick(object sender, ItemClickEventArgs e)
        {
            TempCourseItem = e.ClickedItem as CourseItem;

            // Init dialog data

            // Init data
            foreach (var c in CourseCategories)
            {
                if (c.CategoryID == TempCourseItem.CategoryID)
                {
                    NewPointParentCategoryName.Text = c.CategoryName.ToUpper();
                }
            }

            NewPointCategoryName.Text = TempCourseItem.Name;

            var bodoviTxt = "";
            var minPoints = int.Parse(TempCourseItem.MinPoints.ToString()).ToString();

            if (minPoints.EndsWith("1") && minPoints != "11")
            {
                bodoviTxt = "bod";
            }
            else if (minPoints.EndsWith("2") || minPoints.EndsWith("3") || minPoints.EndsWith("4")
                     && minPoints != "12" && minPoints != "13" && minPoints != "14")
            {
                bodoviTxt = "boda";
            }
            else
            {
                bodoviTxt = "bodova";
            }

            MaxNewPoints.Text = $"/{TempCourseItem.Points}";
            MinNewPoints.Text = $"{TempCourseItem.MinPoints} {bodoviTxt}";
            if (TempCourseItem.MinPoints < 1)
            {
                ConditionStackPanel.Visibility = Visibility.Collapsed;
            }

            NewPoints.Text = "";

            // Open dialog

            await EditPointsDialog.ShowAsync();




        }

        private void EditPointsDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Normalize number
            var clearedPoints = Regex.Replace(NewPoints.Text, "[^0-9.]", "");
            clearedPoints = Regex.Replace(clearedPoints, @"(?:\.0+)|(?:\.$)", "");

            if (clearedPoints == "")
            {
                EditPointsDialog.IsPrimaryButtonEnabled = false;
                return;
            }

            var counter = 0;
            foreach (var c in clearedPoints)
            {
                if (c.CompareTo('0') == 0)
                {
                    counter++;
                    continue;
                }

                clearedPoints = clearedPoints.Substring(counter);
                break;
            }

            if (clearedPoints.StartsWith(".")) clearedPoints = $"0{clearedPoints}";

            // Update database

            using (var connection = new SqlConnection(Helpers.FunctionsAndInterfaces.connectionString))
            {
                connection.Open();

                var query =
                    $"UPDATE UserPoints SET Points = {clearedPoints} WHERE UserID = {CurrentUserId} AND CourseItemID = {TempCourseItem.CourseItemID};";

                var command = new SqlCommand(query, connection);
                using (command.ExecuteReader())
                {
                }

                connection.Close();

            }

            // Update view

            foreach (var c in CourseCategories)
            {
                foreach (var i in c.ChildItems)
                {
                    if (c.CategoryID == TempCourseItem.CategoryID && i.CourseItemID == TempCourseItem.CourseItemID)
                    {
                        i.CurrentPoints = double.Parse(clearedPoints);
                    }
                }
            }

            CalculateTotalPoints();

        }

        private void NewPoints_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            // Normalize number
            var clearedPoints = Regex.Replace(NewPoints.Text, "[^0-9.]", "");
            clearedPoints = Regex.Replace(clearedPoints, @"(?:\.0+)|(?:\.$)", "");

            // Check conditions

            if (clearedPoints == "")
            {
                EditPointsDialog.IsPrimaryButtonEnabled = false;
            }
            else
            {
                var counter = 0;
                foreach (var c in clearedPoints)
                {
                    if (c.CompareTo('0') == 0)
                    {
                        counter++;
                        continue;
                    }

                    clearedPoints = clearedPoints.Substring(counter);
                    break;
                }

                if (clearedPoints.StartsWith(".")) clearedPoints = $"0{clearedPoints}";

                if (double.Parse(clearedPoints) > TempCourseItem.Points)
                {
                    EditPointsDialog.IsPrimaryButtonEnabled = false;
                    return;
                }

                EditPointsDialog.IsPrimaryButtonEnabled = true;
            }
        }

        private async void NewPoints_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Normalize number
            var clearedPoints = Regex.Replace(NewPoints.Text, "[^0-9.]", "");
            clearedPoints = Regex.Replace(clearedPoints, @"(?:\.0+)|(?:\.$)", "");

            // Prepare animations

            var fadeIn = PointInputInfoText.Fade(1.0F, 200D);
            var fadeOut = PointInputInfoText.Fade(0.0F, 100D);

            await fadeOut.StartAsync();

            // Check conditions

            if (clearedPoints == "")
            {
                PointInputInfoText.Text = "❌ Moraš unijeti broj bodova";
            }
            else
            {
                var counter = 0;
                foreach (var c in clearedPoints)
                {
                    if (c.CompareTo('0') == 0)
                    {
                        counter++;
                        continue;
                    }

                    clearedPoints = clearedPoints.Substring(counter);
                    break;
                }

                if (clearedPoints.StartsWith(".")) clearedPoints = $"0{clearedPoints}";

                if (double.Parse(clearedPoints) > TempCourseItem.Points)
                {
                    PointInputInfoText.Text = "❌ Ne možeš imati toliko bodova";
                    await fadeIn.StartAsync();
                    return;
                }

                PointInputInfoText.Text = "✔";
            }

            await fadeIn.StartAsync();

        }
    }
}