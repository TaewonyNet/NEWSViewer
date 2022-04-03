using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            var Processes = Process.GetProcessesByName(CurrentProcess.ProcessName);
            if ((Debugger.IsAttached != true) && (Processes.Length > 1))
            //if (Processes.Length > 1)
            {
                var result = MessageBox.Show("동일한 프로그램이 실행중입니다.\n기존 프로그램을 강제종료하고 실행하려면 확인버튼을 눌러주세요.\n취소를 누르면 현제 프로그램이 종료됩니다.", CurrentProcess.ProcessName, MessageBoxButton.OKCancel);
                //MessageBox.Show("동일한 프로그램이 실행중입니다.\n기존 프로그램을 종료 후 다시 실행해주세요.\n프로그램 종료중이면 잠시 기다린 후 다시 실행해주세요.", CurrentProcess.ProcessName);
                if (result == MessageBoxResult.OK)
                {
                    // 모두 종료
                    foreach (Process item in Processes)
                    {
                        if (item.Id != CurrentProcess.Id)
                        {
                            item.Kill();
                        }
                    }
                }
                else
                {
                    // 현재 프로그램 종료
                    this.Shutdown();
                }
            }
            base.OnStartup(e);
        }

        public App()
        {
            Global.Instance.Init().GetAwaiter().GetResult();
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Processes all messages currently in the message queue.
        /// </summary>
        /// <remarks>
        /// This method can potentially cause code re-entrancy problem, so use it with great care.
        /// </remarks>
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            //Global.Instance.StatusAdd("알려지지 않은 에러 발생. 관리자에게 문의하세요.");
            Log.Error("App_DispatcherUnhandledException 에러 메시지:{0}", ex);
            e.Handled = true;
        }
    }
}
