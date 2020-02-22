using Microsoft.Win32;
using NAudio.Wave;
using System.Windows.Forms;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Threading;

namespace NAudioSample.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "SimpleRecoder";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand StartRecording { get; private set; }

        public ObservableCollection<string> WaveOutList { get; set; } = new ObservableCollection<string>();

        private void SetDeviceList()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                WaveOutList.Add(capabilities.ProductName);
                System.Console.WriteLine(capabilities.ProductName);
            }
        }

        public MainWindowViewModel()
        {
            SetDeviceList();
            StartRecording = new DelegateCommand(Recording);
        }

        ~MainWindowViewModel()
        {
        }

        private NAudio.Wave.WaveFileWriter waveWriter = null;

        public void Recording()
        {
            // 録音先のWavファイル
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Title = "ファイルを保存";
                dialog.Filter = "wavファイル|*.wav";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
                    Directory.CreateDirectory(outputFolder);
                    var outputFilePath = Path.Combine(outputFolder, "recorded.wav");
                    var capture = new WasapiLoopbackCapture();
                    var writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);

                    capture.DataAvailable += (s, a) =>
                    {
                        writer.Write(a.Buffer, 0, a.BytesRecorded);
                        if (writer.Position > capture.WaveFormat.AverageBytesPerSecond * 10)
                        {
                            capture.StopRecording();
                        }
                    };

                    capture.RecordingStopped += (s, a) =>
                    {
                        writer.Dispose();
                        writer = null;
                        capture.Dispose();
                    };

                    capture.StartRecording();
                }
                else
                {
                }
            }

            return;
        }

        private void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;
            // Waveファイルへ書き込み
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }
    }
}
