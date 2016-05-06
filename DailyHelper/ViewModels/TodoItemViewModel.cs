using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DailyHelper.ViewModels
{
    class TodoItemViewModel
    {
        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        public ObservableCollection<Models.TodoItem> AllItems { get { return this.allItems; } }

        private Models.TodoItem selectedItem = default(Models.TodoItem);
        public Models.TodoItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; }  }

        public TodoItemViewModel()
        {
            // 加入三个用来测试的item
            this.allItems.Add(new Models.TodoItem("", "银时", "万事屋党委书记，天然卷的都不是坏蛋", DateTime.Now, new BitmapImage(new Uri("ms-appx://Todos/Assets/gintoki.jpg"))));
            this.allItems.Add(new Models.TodoItem("", "神乐", "夜兔族战神系列，旗袍大胃王少女", DateTime.Now, new BitmapImage(new Uri("ms-appx://Todos/Assets/kakura.jpg"))));
            this.allItems.Add(new Models.TodoItem("", "新八", "眼镜才是本体", DateTime.Now, new BitmapImage(new Uri("ms-appx://Todos/Assets/xinba.jpg"))));
        }

        public void AddTodoItem(string id, string title, string description, DateTime date, ImageSource source)
        {
            this.allItems.Add(new Models.TodoItem(id, title, description, date, source));
        }

        public void RemoveTodoItem()
        {
            this.allItems.Remove(selectedItem);
        }

        public void UpdateTodoItem(string title, string description, DateTime date, ImageSource source)
        {
            int index = allItems.IndexOf(selectedItem);
            if (selectedItem != null)
            {
                allItems[index].title = title;
                allItems[index].description = description;
                allItems[index].date = date;
                allItems[index].source = source;
            }
        }
    }
}
