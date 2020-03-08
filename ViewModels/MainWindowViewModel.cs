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
using NAudioSample.Model;

namespace NAudioSample.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private String _title = "SimpleRecoder";
        public String Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private String _selectedAudioSouece = "";
        public String SelectedAudioSouece
        {
            get { return _selectedAudioSouece; }
            set { SetProperty(ref _selectedAudioSouece, value); }
        }

        private AudioCaptureModel _audioCaputure = null;
        public AudioCaptureModel AudioCaputure
        {
            get { return _audioCaputure; }
            set { SetProperty(ref _audioCaputure, value); }
        }

        public DelegateCommand StartRecording { get; private set; }
        public DelegateCommand StopRecording { get; private set; }

        public ObservableCollection<String> ActiveSpeaker { get; set; } = new ObservableCollection<String>();

        private void SetDeviceList()
        {
            var speakerList = AudioCaptureModel.ActiveSpeakers();
            foreach (var speaker in speakerList)
            {
                ActiveSpeaker?.Add(speaker);
            }
        }

        public MainWindowViewModel()
        {
            SetDeviceList();

            StartRecording = new DelegateCommand(Recording);
            StopRecording = new DelegateCommand(
                () => { AudioCaputure.Stop(); },
                () => (AudioCaputure != null)
                ).ObservesProperty(() => AudioCaputure);
        }

        ~MainWindowViewModel()
        {
            AudioCaputure?.Dispose();
        }

        private NAudio.Wave.WaveFileWriter waveWriter = null;


        public void Recording()
        {
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Title = "ファイルを保存";
                dialog.Filter = "wavファイル|*.wav";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AudioCaputure = new AudioCaptureModel(dialog.FileName);
                    bool ret = AudioCaputure.Init(SelectedAudioSouece, dialog.FileName);
                    if (ret)
                    {
                        AudioCaputure?.Start();
                    }
                }
                else
                {
                    return;
                }
            }

            return;
        }

        private void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Waveファイルへ書き込み
            waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter?.Flush();
        }
    }
}
