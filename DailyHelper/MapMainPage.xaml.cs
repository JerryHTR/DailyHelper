using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace DailyHelper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MapMainPage : Page
    {
        Geolocator geolocator = null;
        public MapMainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            // 创建一个Geolocator对象
            geolocator = new Geolocator();
            // 设置地图的ServiceToken
            myMap.MapServiceToken = "lnBvO4oyxZzr2uk2r2SW~iTiXth-N_MeTqqCoeg_X4A~Aj5zkdRdSt3BZ5HqkU31-0IFrxoINq_oBG1fcR3GhfCVK0uEbqRWNVhgqtF3LBZ1";
        }

        private async void getlocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取当前的地理位置信息
                Geoposition pos = await geolocator.GetGeopositionAsync();
                // 设置地图的中心
                myMap.Center = pos.Coordinate.Point;
                // 纬度信息
                tbLatitude.Text = "纬度:" + pos.Coordinate.Point.Position.Latitude;
                // 经度信息
                tbLongitude.Text = "经度:" + pos.Coordinate.Point.Position.Longitude;
                // 准确性信息
                tbAccuracy.Text = "准确性:" + pos.Coordinate.Accuracy;
            }
            catch (System.UnauthorizedAccessException)
            {
                // 服务被禁用异常
                tbLatitude.Text = "No data";
                tbLongitude.Text = "No data";
                tbAccuracy.Text = "No data";
            }
            catch (TaskCanceledException)
            {
                // 请求被取消
                tbLatitude.Text = "Cancelled";
                tbLongitude.Text = "Cancelled";
                tbAccuracy.Text = "Cancelled";
            }
        }
    }
}
