﻿using NAudio.Wave;
using SoundTimeParametersEvaluation.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SoundTimeParametersEvaluation
{
    public partial class MainForm : Form
    {
        private Dictionary<FrameLevelParamType, Chart> charts;
        private Dictionary<FrameLevelParamType, Label> chartLabels;
        private Dictionary<ClipLevelParamType, Label> labels;

        private int milisecondsPerFrame = 100;
        private int samplesPerFrame;
        private double sampleRate;

        private CustomPoint[] parsedFile;

        public MainForm()
        {
            InitializeComponent();
            InitializeCollections();

            UpdateMPFTextBox();
        }

        private void InitializeCollections()
        {
            charts = new Dictionary<FrameLevelParamType, Chart>();
            charts.Add(FrameLevelParamType.Volume, volumeChart);
            charts.Add(FrameLevelParamType.ShortTimeEnergy, steChart);
            charts.Add(FrameLevelParamType.ZeroCrossingRate, zcrChart);
            charts.Add(FrameLevelParamType.SilentRatio, silenceChart);

            chartLabels = new Dictionary<FrameLevelParamType, Label>();
            chartLabels.Add(FrameLevelParamType.Volume, volumeValueLabel);
            chartLabels.Add(FrameLevelParamType.ShortTimeEnergy, steValueLabel);
            chartLabels.Add(FrameLevelParamType.ZeroCrossingRate, zcrValueLabel);
            chartLabels.Add(FrameLevelParamType.SilentRatio, silenceValueLabel);

            labels = new Dictionary<ClipLevelParamType, Label>();
            labels.Add(ClipLevelParamType.VolumeStandardDeviation, vstdValueLabel);
            labels.Add(ClipLevelParamType.VolumeDynamicRange, vdrValueLabel);
        }

        private void UpdateParameters()
        {
            UpdateFrameLevelParameter(FrameLevelParamType.Volume);
            UpdateFrameLevelParameter(FrameLevelParamType.ShortTimeEnergy);
            UpdateFrameLevelParameter(FrameLevelParamType.ZeroCrossingRate);
            UpdateFrameLevelParameter(FrameLevelParamType.SilentRatio);

            var volume = volumeChart.Series[0].Points.SelectMany(point => { return point.YValues; }).ToArray();

            UpdateClipLevelParameter(ClipLevelParamType.VolumeStandardDeviation, volume);
            UpdateClipLevelParameter(ClipLevelParamType.VolumeDynamicRange, volume);
        }

        private void UpdateFrameLevelParameter(FrameLevelParamType parameter)
        {
            double result = Calculator.CalculateFrameLevelParameter(parameter, parsedFile, samplesPerFrame, sampleRate, out double[] valueInFrame);
            chartLabels[parameter].Text = result.ToString("0.000");

            var chart = charts[parameter];
            ChartHelper.UpdateChart(ref chart, valueInFrame, samplesPerFrame, parsedFile.Length, sampleRate);
        }

        private void UpdateClipLevelParameter(ClipLevelParamType parameter, double[] volume)
        {
            double result = Calculator.CalculateClipLevelParameter(parameter, volume);
            labels[parameter].Text = result.ToString("0.000");
        }

        private void UpdateMPFTextBox()
        {
            mpfTextBox.Text = milisecondsPerFrame.ToString();
        }

        #region Forms handlers

        // TODO: Refactor
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "\\.";
            openFileDialog.Filter = "Wave form audio format (*.wav)|*.wav|" +
                                    "MP3 audio file (*.mp3)|*.mp3";

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                chart1.Series[0].Points.Clear();

                using (var audioFileReader = new AudioFileReader(filePath))
                {
                    var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                    var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                    int samplesRead;
                    while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        wholeFile.AddRange(readBuffer.Take(samplesRead));
                    }

                    parsedFile = new CustomPoint[wholeFile.Count];
                    sampleRate = audioFileReader.WaveFormat.SampleRate;
                    samplesPerFrame = milisecondsPerFrame * audioFileReader.WaveFormat.SampleRate / 1000;
                    for (int i = 0; i < wholeFile.Count; i++)
                    {
                        double timeInSeconds = i / sampleRate;
                        parsedFile[i] = new CustomPoint(timeInSeconds, wholeFile[i]);

                        chart1.Series[0].Points.AddXY(parsedFile[i].X, parsedFile[i].Y);
                    }
                }
            }

            UpdateParameters();
        }

        #endregion
    }
}
