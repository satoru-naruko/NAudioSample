using System;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudioSample.Model
{
    public class AudioCaptureModel : IDisposable
    {
        WasapiLoopbackCapture capture;
        WaveFileWriter writer;

        object lockObject = new object();

        public AudioCaptureModel(string outputFilePath)
        {
            capture = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);
            lockObject = new object();

            capture.DataAvailable += DataAvailable;
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                Stop();
                capture.Dispose();
            }
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
