﻿using Love;
using NAudio.Wave;
using System;

namespace AudioVisualizer
{
    /*
     * Visualizer using frequencies
     */
    class FreqVisualizer : VisualizerWindow
    {
        private WaveBuffer buffer;

        private int Intensity = 2;
        private int Zoom = 8;

        private int M = 6;

        public override void Load()
        {
            WindowTitle = "Frequency Visualizer";
            base.Load();

            // start audio capture
            var capture = new WasapiLoopbackCapture();

            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            capture.StartRecording();
        }

        public void DataAvailable(object sender, WaveInEventArgs e)
        {
            buffer = new WaveBuffer(e.Buffer); // save the buffer in the class variable
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);

            switch (key)
            {
                case KeyConstant.Right:
                    Zoom += 1;
                    break;
                case KeyConstant.Left:
                    Zoom = Math.Max(Zoom - 1, 1);
                    break;
                case KeyConstant.R:
                    Zoom = 8;
                    Intensity = 2;
                    break;
            }
        }

        public override void WheelMoved(int x, int y)
        {
            Intensity = Math.Max(Intensity - y, 1);
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            int len = buffer.FloatBuffer.Length / Zoom;

            if (Zoom <= 0)
                Graphics.Print("Zoom is invalid");

            float pad = (float)len / WindowWidth; // samples per pixels

            // fft
            NAudio.Dsp.Complex[] values = new NAudio.Dsp.Complex[len];
            for (int i = 0; i < len; i++)
            {
                values[i].Y = 0;
                values[i].X = buffer.FloatBuffer[i];
            }
            NAudio.Dsp.FastFourierTransform.FFT(true, M, values);

            float size = (float)WindowWidth / ((float)Math.Pow(2, M) / 2);

            for (int i = 1; i < Math.Pow(2, M) / 2; i++)
            {
                //Graphics.Print(i.ToString() + ": " + values[i].X.ToString("N2") + " i " + (values[i].Y + 0.50f).ToString("N2"), 0, (i + 1) * 16);
                Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight / 2, size, -Math.Abs(values[i].X) * (WindowHeight / 2) * 10);
            }
        }
    }
}
