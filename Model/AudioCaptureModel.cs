using System;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;

namespace NAudioSample.Model
{
    public class AudioCaptureModel : IDisposable
    {
        WasapiLoopbackCapture capture;
        WaveFileWriter writer;

        object lockObject = new object();

        public AudioCaptureModel(string outputFilePath)
        {
            lockObject = new object();
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                Stop();
                capture.Dispose();
            }
        }

        public bool Init(String speakerName, String outputFilePath)
        {
            var collection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            MMDevice device = collection.FirstOrDefault(_device => _device.DeviceFriendlyName == speakerName);

            if (device == null)
            {
                return false;
            }

            capture = new WasapiLoopbackCapture(device);
            capture.DataAvailable += DataAvailable;
            writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);

            return true;
        }

        public void Start()
        {
            lock (lockObject)
            {
                capture.StartRecording();
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                capture.StopRecording();
                if (writer != null) { 
                    writer.Dispose();
                    writer = null;
                }
            }
        }

        public static List<string> ActiveSpeakers()
        {
            var deviceList = new List<string>();
            var collection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var item in collection)
            {
                Console.WriteLine(item.DeviceFriendlyName);
                deviceList.Add(item.DeviceFriendlyName);
            }
            return deviceList;

        }

        public delegate void WaveDataAvaliableEventHandler(object sender, AudioCaptureWaveDataEventArgs e);
        public event WaveDataAvaliableEventHandler WaveDataAvaliableEvent;
        protected void OnWaveDataAvailable(WaveInEventArgs e)
        {
            var e2 = new AudioCaptureWaveDataEventArgs(e.Buffer, e.BytesRecorded);
            WaveDataAvaliableEvent?.Invoke(this, e2);
        }

        private void DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (lockObject)
            {
                if(writer != null)
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                //if (writer.Position > capture.WaveFormat.AverageBytesPerSecond * 10)
                //{
                //    capture.StopRecording();
                //}
            }
            this.OnWaveDataAvailable(e);
        }

        public class AudioCaptureWaveDataEventArgs : WaveInEventArgs
        {
            public AudioCaptureWaveDataEventArgs(byte[] buffer, int bytesrecorded ) : base(buffer, bytesrecorded)
            {
            }

            public new byte[] Buffer => this.Buffer;
            public new byte[] BytesRecorded => this.BytesRecorded;
        }
    }
}
