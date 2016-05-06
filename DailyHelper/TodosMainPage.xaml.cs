using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace DailyHelper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TodosMainPage : Page
    {
        public TodosMainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 480));
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
            this.ViewModel = new ViewModels.TodoItemViewModel();
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }
        Models.TodoItem shareItem;

        // 刚进入页面时初始化viewmodel
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
            {
                this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
            }
            ViewModel.SelectedItem = null;
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        // 离开页面时移除数据共享事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }

        // 待办事项列表条目的点击处理函数
        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (Models.TodoItem)(e.ClickedItem);
            if (InlineToDoItemViewGrid.Visibility != Visibility.Visible)
            {
                Frame.Navigate(typeof(TodosNewPage), ViewModel);
            }
            else
            {
                if (ViewModel.SelectedItem == null)
                {
                    createButton.Content = "Create";
                }
                else
                {
                    createButton.Content = "Update";
                    CancelButton.Content = "Delete";
                    idBox.Text = ViewModel.SelectedItem.id;
                    title.Text = ViewModel.SelectedItem.title;
                    description.Text = ViewModel.SelectedItem.description;
                    date.Date = ViewModel.SelectedItem.date;
                    backpic.Source = ViewModel.SelectedItem.source;
                }
            }
        }

        // 添加新事项的按钮点击处理函数
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (InlineToDoItemViewGrid.Visibility != Visibility.Visible)
            {
                ViewModel.SelectedItem = null;
                Frame.Navigate(typeof(TodosNewPage), ViewModel);
            }
            else
            {
                idBox.Text = "";
                title.Text = "";
                description.Text = "";
                date.Date = DateTime.Now;
                createButton.Content = "Create";
                CancelButton.Content = "Cancel";
                backpic.Source = new BitmapImage(new Uri("ms-appx://Todo/Assets/background.jpg"));
            }
        }

        // 隐藏页面create按钮点击处理函数
        private void createButton_Click(object sender, RoutedEventArgs e)
        {
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

        // 取消或删除按钮点击处理函数
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)CancelButton.Content == "Delete")
            {
                int id = int.Parse(idBox.Text);
                ViewModel.RemoveTodoItem();
                Delete_Item(id);
                ViewModel.SelectedItem = null;
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
            else {
                title.Text = "";
                description.Text = "";
                date.Date = DateTime.Now;
                backpic.Source = new BitmapImage(new Uri("ms-appx://Todo/Assets/background.jpg"));
            }
        }

        // 数据共享事件执行
        private async void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var defl = args.Request.GetDeferral();

            DataPackage dp = new DataPackage();
            dp.Properties.Title = shareItem.title;
            dp.Properties.Description = "共享的任务";
            dp.SetText(shareItem.description);

            if (((BitmapImage)shareItem.source).UriSource != null)
            {
                string path = ((BitmapImage)shareItem.source).UriSource.ToString();
                path = path.Remove(0, 15);
                path = path.Replace('/', '\\');

                StorageFile file = await Package.Current.InstalledLocation.GetFileAsync(path);
                RandomAccessStreamReference img = RandomAccessStreamReference.CreateFromFile(file);
                dp.SetBitmap(img);
            }

            args.Request.Data = dp;
            defl.Complete();
        }

        // 共享按钮点击事件处理函数
        private void share_Click(object sender, RoutedEventArgs e)
        {
            var v = sender as FrameworkElement;
            var dc = v.DataContext;
            ListViewItem item = this.ToDoListView.ContainerFromItem(dc) as ListViewItem;
            shareItem = (Models.TodoItem)item.Content;
            DataTransferManager.ShowShareUI();
        }

        // 更新磁贴按钮点击事件处理函数
        private void updateTile_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument tileXml = new XmlDocument();
            tileXml.LoadXml(File.ReadAllText("Tile.xml"));
            Models.TodoItem LastItem = ViewModel.AllItems.Last();

            XmlNodeList str = tileXml.GetElementsByTagName("text");
            for (int i = 0; i < str.Count; i++)
            {
                ((XmlElement)str[i]).InnerText = LastItem.title;
                if (i == 0) continue;
                i++;
                ((XmlElement)str[i]).InnerText = LastItem.description;
            }
            TileNotification notifi = new TileNotification(tileXml);
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Update(notifi);
        }

        // 从本机中选择图片
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

        // 数据库查询
        public List<Todoitem> GetTodoitem(string queryStr)
        {
            var db = App.conn;
            List<Todoitem> todoList = new List<Todoitem>();
            string inputQuery = queryStr;
            using (var statement = db.Prepare("SELECT Id, Title, Context, Date FROM Todoitems WHERE Title LIKE '%'||?||'%' OR Context LIKE '%'||?||'%' OR DATE LIKE '%'||?||'%'"))
            {
                statement.Bind(1, inputQuery);
                statement.Bind(2, inputQuery);
                statement.Bind(3, inputQuery);
                while (SQLiteResult.ROW == statement.Step())
                {
                    Todoitem todoitem = new Todoitem();
                    todoitem.Id = (long)statement[0];
                    todoitem.Title = (string)statement[1];
                    todoitem.Context = (string)statement[2];
                    todoitem.Date = (string)statement[3];
                    todoList.Add(todoitem);
                }
            }
            return todoList;
        }

        // 查询所有事项按钮点击处理函数
        private async void BtnGetAll_Click(object sender, RoutedEventArgs e)
        {
            //Query
            string inputQuery = Query.Text;
            List<Todoitem> todoList = new List<Todoitem>();
            todoList = GetTodoitem(inputQuery);
            string queryResult = "";
            for (int i = 0; i < todoList.Count(); i++)
            {
                queryResult += "Id : " + todoList[i].Id.ToString() + "\n" + "Title : " + todoList[i].Title + "\n"
                + "Context : " + todoList[i].Context + "\n" + "Date : " + todoList[i].Date + "\n\n";
            }
            var resultMessage = new ContentDialog()
            {
                Title = "Result Message",
                Content = queryResult,
                PrimaryButtonText = "确定",
                FullSizeDesired = false,
            };
            await resultMessage.ShowAsync();
        }

        // 获取事项的id
        public Todoitem GetTodoitemById(int id)
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

        // 数据库创建
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

        // 数据库更新
        private void Update_Item(object sender, RoutedEventArgs e)
        {
            //Update
            var db = App.conn;
            int todoId = int.Parse(idBox.Text);

            Todoitem todoitem = new Todoitem();
            string todoDate = date.Date.ToString("yyyy-MM-dd");
            string todoTitle = title.Text;
            string todoContext = description.Text;
            todoitem.Title = todoTitle;
            todoitem.Context = todoContext;
            todoitem.Date = todoDate;
            todoitem.Id = todoId;

            var existingTodoitem = GetTodoitemById(todoId);
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

        // 数据库删除
        private void Delete_Item(int id)
        {
            var db = App.conn;
            using (var statement = db.Prepare("DELETE FROM Todoitems WHERE Id = ?"))
            {
                statement.Bind(1, id);
                statement.Step();
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
    }

    // 数据类型转换器
    public class DataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? ischeched = value as bool?;
            if (ischeched == null || ischeched == false)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
