using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace DailyHelper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = new ViewModels.TodoItemViewModel();
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }

        // 汉堡菜单按钮点击事件处理函数
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = (Splitter.IsPaneOpen == true) ? false : true;
        }

        // 导航至待办事项
        private void Navigate_Todos(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = false;
            Frame.Navigate(typeof(TodosMainPage), ViewModel);
        }

        // 导航至地理位置
        private void Navigate_Map(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = false;
            Frame.Navigate(typeof(MapMainPage));
        }

        // 导航至播放视频
        private void Navigate_Video(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = false;
            Frame.Navigate(typeof(VideoMainPage));
        }

        // 导航至语音识别
        private void Navigate_Voice(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = false;
            Frame.Navigate(typeof(VoiceMainPage));
        }

    }
}
