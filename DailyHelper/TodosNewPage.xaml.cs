using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using SQLitePCL;
using DailyHelper.ViewModels;

namespace DailyHelper
{
    public sealed partial class TodosNewPage : Page
    {
        public TodosNewPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;

        }

        private ViewModels.TodoItemViewModel ViewModel;

        private void Create_Item(object sender, RoutedEventArgs e)
        {
            //Insert
            var db = App.conn;
            int todoId = int.Parse(idBox.Text);

            string todoDate = date.Date.ToString("yyyy-MM-dd");
            string todoTitle = title.Text;
            string todoContext = description.Text;

            using (var todoitem = db.Prepare("INSERT INTO Todoitems (Id, Title, Context, Date) VALUES (?, ?, ?, ?)"))
            {
                todoitem.Bind(1, todoId);
                todoitem.Bind(2, todoTitle);
                todoitem.Bind(3, todoContext);
                todoitem.Bind(4, todoDate);
                todoitem.Step();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = ((ViewModels.TodoItemViewModel)e.Parameter);
            if (ViewModel.SelectedItem == null)
            {
                createButton.Content = "Create";
            }
            else
            {
                createButton.Content = "Update";
                idBox.Text = ViewModel.SelectedItem.id;
                title.Text = ViewModel.SelectedItem.title;
                description.Text = ViewModel.SelectedItem.description;
                date.Date = ViewModel.SelectedItem.date;
                backpic.Source = ViewModel.SelectedItem.source;
            }
        }
        private void CreateButton_Clicked(object sender, RoutedEventArgs e)
        {
            // check the textbox and datapicker
            // if ok
            bool flag;
            string inputDate = date.Date.ToString("yyyy-MM-dd");
            string nowDate = DateTime.Now.ToString("yyyy-MM-dd");

            if (title.Text.ToString() == "")
            {
                var i = new MessageDialog("Title不能为空").ShowAsync();
                flag = false;
            }
            else if (description.Text.ToString() == "")
            {
                var i = new MessageDialog("Details不能为空").ShowAsync();
                flag = false;
            }
            else if (string.Compare(inputDate, nowDate) < 1)
            {
                var i = new MessageDialog("Date错误").ShowAsync();
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                if (createButton.Content.ToString() == "Update")
                {
                    if (ViewModel.SelectedItem != null)
                    {
                        ViewModel.UpdateTodoItem(title.Text, description.Text, date.Date.DateTime, backpic.Source);
                        Update_Item(sender, e);
                        ViewModel.SelectedItem = null;
                    }
                }
                else
                {
                    int todoId = ViewModel.AllItems.Count() - 2;
                    idBox.Text = todoId.ToString();
                    ViewModel.AddTodoItem(idBox.Text, title.Text, description.Text, date.Date.DateTime, backpic.Source);
                    Create_Item(sender, e);
                }
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        public class Todoitem
        {
            public long Id { get; set; }
            public string Title { get; set; }
            public string Context { get; set; }
            public string Date { get; set; }

            public Todoitem()
            {
                Id = 0;
                Title = "";
                Context = "";
                Date = "";
            }
        }

        public Todoitem GetTodoitem(int id)
        {
            var db = App.conn;
            Todoitem todoitem = new Todoitem();
            using (var statement = db.Prepare("SELECT Id, Title, Context, Date FROM Todoitems WHERE Id = ?"))
            {
                statement.Bind(1, id);
                if (SQLiteResult.DONE != statement.Step())
                {
                    todoitem.Id = (long)statement[0];
                    todoitem.Title = (string)statement[1];
                    todoitem.Context = (string)statement[2];
                    todoitem.Date = (string)statement[3];
                }
            }
            return todoitem;
        }

        private void Update_Item(object sender, RoutedEventArgs e)
        {
            //Update
            var db = App.conn;
            int id = int.Parse(idBox.Text);

            Todoitem todoitem = new Todoitem();
            string todoDate = date.Date.ToString("yyyy-MM-dd");
            string todoTitle = title.Text;
            string todoContext = description.Text;
            todoitem.Title = todoTitle;
            todoitem.Context = todoContext;
            todoitem.Date = todoDate;
            todoitem.Id = id;

            var existingTodoitem = GetTodoitem(id);
            if (existingTodoitem != null)
            {
                using (var todotemp = db.Prepare("UPDATE Todoitems SET Title = ?, Context = ?, Date = ? WHERE Id = ?"))
                {
                    todotemp.Bind(1, todoitem.Title);
                    todotemp.Bind(2, todoitem.Context);
                    todotemp.Bind(3, todoitem.Date);
                    todotemp.Bind(4, todoitem.Id);
                    todotemp.Step();
                }
            }
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(idBox.Text);
            ViewModel.RemoveTodoItem();
            Delete_Item(id);
            ViewModel.SelectedItem = null;
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void Delete_Item(int id)
        {
            var db = App.conn;
            using (var statement = db.Prepare("DELETE FROM Todoitems WHERE Id = ?"))
            {
                statement.Bind(1, id);
                statement.Step();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            description.Text = "";
            date.Date = DateTime.Now;
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            // Set up the file picker.
            Windows.Storage.Pickers.FileOpenPicker openPicker =
                new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.ViewMode =
                Windows.Storage.Pickers.PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            // Open the file picker.
            Windows.Storage.StorageFile file =
                await openPicker.PickSingleFileAsync();

            // 'file' is null if user cancels the file picker.
            if (file != null)
            {
                // Open a stream for the selected file.
                // The 'using' block ensures the stream is disposed
                // after the image is loaded.
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                        new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    backpic.Source = bitmapImage;
                }
            }
        }
    }
}
