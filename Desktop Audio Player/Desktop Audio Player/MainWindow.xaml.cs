using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml.Schema;

namespace Desktop_Audio_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        string filename;
        bool slider_clicked = false;
        double totalTime = 1.0;
        bool is_total_time_set = false;
        bool play_not_pause = true;
        double volume = 1.0;
        bool is_muted = false;
        public MainWindow() {
            InitializeComponent();
            file_search.Visibility = Visibility.Visible;
            controls.Visibility = Visibility.Hidden;
            LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            myHorizontalGradient.StartPoint = new Point(0,0.5);
            myHorizontalGradient.EndPoint = new Point(1,0.5);
            myHorizontalGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#12c2e9"), 0.0));
            myHorizontalGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#c471ed"), 0.5));
            myHorizontalGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#f64f59"), 1.0));
            open_button.Background = myHorizontalGradient;
            MouseDown += Window_MouseDown;
            slider_timer.AddHandler(MouseLeftButtonDownEvent,
                      new MouseButtonEventHandler(slider_pressed),
                      true);
            slider_timer.AddHandler(MouseLeftButtonUpEvent,
                      new MouseButtonEventHandler(slider_unpressed),
                      true);
            slider_timer.ValueChanged += slider_changed;
            slider_volume.AddHandler(MouseLeftButtonUpEvent,
                      new MouseButtonEventHandler(volume_changed),
                      true);
            slider_volume.ValueChanged += volume_changed;
        }

        private void slider_pressed(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("pressed");
            slider_clicked = true;
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, (int)(((slider_timer.Value / 10) * totalTime)));
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
        }


        private void slider_changed(object sender, RoutedEventArgs e)
        {
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, (int)(((slider_timer.Value / 10) * totalTime)));
            mediaPlayer.Position = my_time;
        }

        private void play_pause(object sender, RoutedEventArgs e)
        {
            if (play_not_pause)
            {
                BT_Click_Play(sender, e);
                play_not_pause = false;
            }
            else
            {
                BT_Click_Pause(sender, e);
                play_not_pause = true;
            }
        }

        private void slider_unpressed(object sender, MouseButtonEventArgs e)
        {
            TimeSpan my_time = new TimeSpan(0,0,0,0, (int)(((slider_timer.Value / 10) * totalTime )));
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
            slider_clicked = false;
        }

        private void volume_changed(object sender, RoutedEventArgs e)
        {
            volume = slider_volume.Value / 10;
            mediaPlayer.Volume = volume;
            if (volume == 0.0)
            {
                is_muted = true;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
            }
            else
            {
                is_muted = false;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void BT_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.DefaultExt = ".mp3";
            fileDialog.Filter = "MP3 File (*.mp3)|*.mp3|FLAC File (*.flac)|*.flac|WAV File (*.wav)|*.wav|M4A [Stop Using This] (*.m4a)|*.m4a";
            bool? dialogOk = fileDialog.ShowDialog();
            if (dialogOk == true)
            {
                filename = fileDialog.FileName;
                File_Name.Text = filename;
                mediaPlayer.Open(new Uri(filename));
                controls.Visibility = Visibility.Visible;
                file_search.Visibility = Visibility.Hidden;
            }
        }

        private void BT_Click_Mute(object sender, RoutedEventArgs e)
        {
            if (is_muted)
            {
                if (volume == 0.0)
                {
                    volume = 0.1;
                }
                mediaPlayer.Volume = volume;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                is_muted = false;
            }
            else
            {
                mediaPlayer.Volume = 0.0;
                volume_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                is_muted = true;
            }
        }

        private void BT_Click_Play(object sender, RoutedEventArgs e)
        { 
            mediaPlayer.Play();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += timer_Tick;
            timer.Start();
            file_search.Visibility = Visibility.Hidden;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            mediaPlayer.Volume = volume;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!slider_clicked)
            {
                if (!is_total_time_set)
                {
                    try
                    {
                        totalTime = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                        final_time.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString("hh':'mm':'ss");
                        is_total_time_set = true;
                    }
                    catch (InvalidOperationException e1)
                    {
                        Debug.WriteLine(e1.Message);
                    }
                }
                var progress_per = mediaPlayer.Position.TotalMilliseconds / totalTime * 10;
                slider_timer.Value = progress_per;
                curr_time.Text = mediaPlayer.Position.ToString("hh':'mm':'ss");
            }
        }

        private void BT_Click_Reset(object sender, RoutedEventArgs e)
        {
            TimeSpan my_time = new TimeSpan(0, 0, 0, 0, 0);
            mediaPlayer.Position = my_time;
            curr_time.Text = my_time.ToString("hh':'mm':'ss");
        }

        private void BT_Click_Pause(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            file_search.Visibility = Visibility.Hidden;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
        }

        private void BT_Click_Stop(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            filename = "";
            File_Name.Text = filename;
            mediaPlayer.Close();
            controls.Visibility = Visibility.Hidden;
            file_search.Visibility = Visibility.Visible;
            curr_time.Text = "--:--:--";
            final_time.Text = "--:--:--";
            play_not_pause = true;
            play_pause_button.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
        }
    }
}
