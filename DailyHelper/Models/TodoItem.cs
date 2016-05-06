using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace DailyHelper.Models
{
    class TodoItem
    {

        public string id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public bool completed { get; set; }

        public DateTime date { get; set; }

        public ImageSource source { get; set; }

        public TodoItem(string id, string title, string description, DateTime date, ImageSource source)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.completed = false; //默认为未完成
            this.date = date;
            this.source = source;
        }
    }
}
