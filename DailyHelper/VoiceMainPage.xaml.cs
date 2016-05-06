using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
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
    public sealed partial class VoiceMainPage
    {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private IAsyncOperation<SpeechRecognitionResult> recognitionOperation;
        private enum NotifyType { StatusMessage, ErrorMessage }; // 表示识别状态的枚举量


        public VoiceMainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 进入页面时获取麦克风权限
        /// 保存UI线程分配器并初始化语音识别器
        /// </summary>
        /// <param name="e">导航事件的细节信息</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 保存UI线程分配器从而允许在UI界面显示识别状态信息
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                // 获得使用麦克风权限则激活识别按钮                
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                buttonOnListen.IsEnabled = true;
            }
            else
            {
                // 若未能获取使用麦克风权限则灭活识别按钮
                buttonOnListen.IsEnabled = false;
            }
        }

        // 识别按钮的点击事件处理函数
        private async void OnListenAsync(object sender, RoutedEventArgs e)
        {
            buttonOnListen.IsEnabled = false;

            // 开始识别
            try
            {
                recognitionOperation = speechRecognizer.RecognizeAsync();
                SpeechRecognitionResult speechRecognitionResult = await recognitionOperation;
                // 若成功则显示识别结果
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    // 通过speechRecognitionResult.Text来获取识别文本
                    var messageDialog = new MessageDialog(speechRecognitionResult.Text, "说话内容");
                    await messageDialog.ShowAsync();
                }
                else
                {
                    // 处理识别失败异常
                }
            }
            catch (TaskCanceledException exception)
            {
                // 若用户离开识别场景时识别进程还在活动
                // 则抛出TaskCanceledException
                System.Diagnostics.Debug.WriteLine("TaskCanceledException: 识别进程仍在活动（可忽略）");
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
            catch (Exception exception)
            {

                var messageDialog = new MessageDialog(exception.Message, "异常");
                await messageDialog.ShowAsync();
            }

            buttonOnListen.IsEnabled = true;
        }

        /// <summary>
        /// 保证在离开页面时清除所有在OnNavigatedTo函数中创建的状态追踪事件处理器从而防止泄露
        /// </summary>
        /// <param name="e">导航事件的细节信息</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    if (recognitionOperation != null)
                    {
                        recognitionOperation.Cancel();
                        recognitionOperation = null;
                    }
                }

                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
        }

        /// <summary>
        /// 初始化语音识别器并编译约束条件
        /// </summary>
        /// <param name="recognizerLanguage">语音识别器所使用的语言</param>
        /// <returns>可等待的任务</returns>
        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (speechRecognizer != null)
            {
                // 移除先前注册的事件以重新初始化场景
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            // 创建语音识别器实例
            speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            // 向用户提供识别状态的反馈信息
            speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

            // 给识别器添加web搜索标题约束
            var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "webSearch");
            speechRecognizer.Constraints.Add(webSearchGrammar);

            // 编译约束条件
            SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();
            if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
            {
                buttonOnListen.IsEnabled = false;
            }
        }

        /// <summary>
        /// 通过更新UI组件来处理识别器状态变化事件
        /// </summary>
        /// <param name="sender">产生状态事件的语音识别器</param>
        /// <param name="args">识别器的状态</param>
        private async void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NotifyUser("语音识别器状态: " + args.State.ToString(), NotifyType.StatusMessage);
            });
        }

        private void NotifyUser(string v, object statusMessage)
        {
            txtResult.Text = v + statusMessage.ToString();
        }
    }
}
