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

        public AudioCaptureModel()
        {
        }

        public AudioCaptureModel(string outputFilePath)
        {
            this.capture = new WasapiLoopbackCapture();
            this.writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);

            capture.DataAvailable += DataAvailable;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
