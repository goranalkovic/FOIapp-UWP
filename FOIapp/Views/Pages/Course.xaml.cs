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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FOIapp.Classes;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp.Helpers;
using User = Windows.System.User;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FOIapp.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Course : Page
    {
        private ObservableCollection<CourseCategory> courseCategories = new ObservableCollection<CourseCategory>();
        private LocalObjectStorageHelper localStorage = new LocalObjectStorageHelper();
        private List<CourseItem> tempItems = new List<CourseItem>();
        private int currentCourseID;
        private int currentUserID;


        public Course()
        {
            InitializeComponent();
            LoadInfoForCourse();
            
        }

        public void LoadInfoForCourse()
        {
            currentUserID = localStorage.Read<int>("currentUserId");
            currentCourseID = localStorage.Read<int>("currentCourseID");
            ActiveCourseName.Text = localStorage.Read<string>("currentCourseName");

            new Task(async () =>
            {
                //using (var connection = Helpers.Databases.GetSqlConnection("galkovic-bp2.database.windows.net", "Goc", "sony-yield-pencil1", "bp2-projekt"))
                using (var connection = new SqlConnection("Data Source=.;Initial Catalog=bp2-db;Integrated Security=True"))
                {
                    connection.Open();
                    var query = $"SELECT DISTINCT CourseItems.CategoryID, CourseCategories.CategoryName FROM CourseItems JOIN UserPoints ON CourseItems.CourseItemID = UserPoints.CourseItemID, Courses, CourseCategories WHERE Courses.CourseID = CourseItems.CourseID AND CourseCategories.CategoryID = CourseItems.CategoryID AND Courses.CourseID = {currentCourseID};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courseCategories.Add(new CourseCategory
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1)
                                });
                            }
                        }
                    }

                    query = $"SELECT CourseItems.CourseItemID, CourseID, CategoryID, Name, CourseItems.Points, MinPoints, UserPoints.Points FROM UserPoints JOIN CourseItems ON UserPoints.CourseItemID = CourseItems.CourseItemID WHERE CourseID = {currentCourseID} AND UserPoints.UserID = {currentUserID};";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tempItems.Add(new CourseItem
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

                    connection.Close();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CourseCategoryPoints.ItemsSource = courseCategories;

                    foreach (var item in courseCategories)
                    {
                        foreach (var catItem in tempItems)
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

    }
}
