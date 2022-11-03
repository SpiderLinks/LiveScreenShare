﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Documents;

using SharpDX.DXGI;

using DiscordRPC;

using BetterLiveScreen.Clients;
using BetterLiveScreen.Extensions;
using BetterLiveScreen.Recording;
using BetterLiveScreen.Recording.Audio;
using BetterLiveScreen.Recording.Audio.WinCaptureAudio;
using BetterLiveScreen.Recording.Types;
using BetterLiveScreen.Recording.Video;
using BetterLiveScreen.Rooms;
using BetterLiveScreen.Users;

using BetterLiveScreen.BetterShare;

using Path = System.IO.Path;
using CvSize = OpenCvSharp.Size;
using BitmapConverter = BetterLiveScreen.Extensions.BitmapConverter;
using NAudio.Lame;
using NAudio.Wave;

namespace BetterLiveScreen
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public static UserInfo User { get; set; }
        internal static string UserToken { get; } = Guid.NewGuid().ToString();
        public static List<UserInfo> Users { get; set; } = new List<UserInfo>();

        public static ClientOne Client { get; set; } = new ClientOne();

        public static BetterShareWindow ShareWindow { get; private set; } = new BetterShareWindow();

        public static bool IsDevMode { get; private set; } = false;

        public static Dispatcher CurrentDispatcher { get; private set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            CurrentDispatcher = this.Dispatcher;

            DiscordHelper.Initialize();
            Rescreen.Initialize();

            this.Closing += MainWindow_Closing;
            AppDomain.CurrentDomain.UnhandledException += MainWindow_UnhandledException;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var startPage = new StartPage();

            startPage.Closing += (s, ee) =>
            {
                if (startPage.IsAccepted)
                {
                    usericon.Fill = BitmapConverter.CreateImageBrush(User.GetAvatarImage());
                    username.Content = User.NameInfo.Name;
                    username.ToolTip = $"#{User.NameInfo.Discriminator}";

                    this.IsEnabled = true;
                }
                else
                {
                    this.Close();
                }
            };

            InitializeUI();

            startPage.Show();
            this.IsEnabled = false;
        }

        private void InitializeUI()
        {
            name1.Content = string.Empty;
            name2.Content = string.Empty;
            name3.Content = string.Empty;
            name4.Content = string.Empty;
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            await Client?.CloseAsync();
            Application.Current.Shutdown();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Developer Mode
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.Y))
            {
                IsDevMode = !IsDevMode;
                credit.Content = string.Concat("[ Dev Moded ]", IsDevMode ? " !" : " ?");
            }
        }

        private void MainWindow_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            MessageBox.Show(ex.ToString(), "BetterLiveScreen: Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void goLive_Click(object sender, RoutedEventArgs e)
        {
            Rescreen.Start();
        }

        private void stopLive_Click(object sender, RoutedEventArgs e)
        {
            Rescreen.Stop();
        }

        public void AnalyzeReceive()
        {
            while (true)
            {

            }
        }

        private async void serverIpConnect_Click(object sender, RoutedEventArgs e)
        {
            AudioSessionManager.Initialize();
            foreach (var p in AudioSessionManager.GetAllPlayingSessions())
            {
                MessageBox.Show(ProcessHelper.GetProcessInfo(p));
            }
            return;

            var ach = new AudioCaptureHelper(8692);
            ach.Start();
            await Task.Delay(10000);
            ach.Stop();

            using (var writer2 = new LameMP3FileWriter(@"C:\Users\erics\Downloads\asdf.mp3", ach.Format.AsStandardWaveFormat(), 128))
            {
                while (ach.bytes.Count > 0)
                {
                    byte[] buffer = ach.bytes.Dequeue();
                    writer2.Write(buffer, 0, buffer.Length);
                }
            }
            MessageBox.Show("Done");
            return;

            await RecordingTest.RecordTestAsync(
                videoType: CaptureVideoType.DD,
                milliseconds: 10000,
                monitor: RescreenSettings.PrimaryMonitor,
                fps: 60,
                isHalf: false,
                nvencEncoding: true
                );
            return;

            string[] info = serverIp.Text.Trim().Split(':');

            if (info.Length == 0)
            {
                return;
            }

            if (!Client.IsReady)
            {
                await Client.ApplyEndPointAsync(info[0]);
            }

            if (IsDevMode)
            {
                await ConnectAsync("y oshi");
                return;
            }

            if (info.Length > 1)
            {
                await ConnectAsync(info[1]);
            }
            else
            {

            }
            
            async Task ConnectAsync(string id)
            {
                if (await RoomManager.ConnectAsync(id))
                {
                    MessageBox.Show("OK");
                }
                else
                {

                }
            }
        }

        private async void serverCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(serverIp.Text)) RecordingTest.TestName = serverIp.Text;
            var rt = new RecordingTest();
            rt.Show();
            return;

            if (!Client.IsReady)
            {
                await Client.ApplyEndPointAsync(serverIp.Text.Trim());
            }

            if (await RoomManager.CreateAsync("test", $"{User.NameInfo.Name}'s Server"))
            {

            }
            else
            {

            }
        }

        private void serverBetterShare_Click(object sender, RoutedEventArgs e)
        {
            if (ShareWindow.IsClosed) ShareWindow = new BetterShareWindow();
            ShareWindow.Show();
        }
    }
}
