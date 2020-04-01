﻿using SoundTimeParametersEvaluation.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundTimeParametersEvaluation
{
    public static class Calculator
    {
        public static double CalculateFrameLevelParameter(FrameLevelParamType parameter, CustomPoint[] parsedFile, int samplesPerFrame, double sampleRate, out double[] resultInFrame)
        {
            int framesCount = parsedFile.Length / samplesPerFrame;
            if (parsedFile.Length % samplesPerFrame != 0)
                framesCount++;

            switch (parameter)
            {
                case FrameLevelParamType.Volume:
                    return GetEnergy(parsedFile, samplesPerFrame, framesCount, out resultInFrame, true);
                case FrameLevelParamType.ShortTimeEnergy:
                    return GetEnergy(parsedFile, samplesPerFrame, framesCount, out resultInFrame);
                case FrameLevelParamType.ZeroCrossingRate:
                    return GetZeroCrossingRate(parsedFile, samplesPerFrame, framesCount, sampleRate, out resultInFrame);
                case FrameLevelParamType.SilentRatio:
                    return GetSilentRatio(parsedFile, samplesPerFrame, framesCount, sampleRate, out resultInFrame);
                default:
                    resultInFrame = new double[framesCount];
                    return 0.0;
            }
        }

        public static double CalculateClipLevelParameter(ClipLevelParamType parameter, double[] volume, double[] energy, double[] zeroCrossingRate, CustomPoint[] parsedFile, double sampleRate)
        {
            switch (parameter)
            {
                case ClipLevelParamType.VolumeStandardDeviation:
                    return GetVolumeStandardDeviation(volume);
                case ClipLevelParamType.VolumeDynamicRange:
                    return GetVolumeDynamicRange(volume);
                case ClipLevelParamType.LowShortTimeEnergyRatio:
                    return GetLowShortTimeEnergyRatio(energy, parsedFile, sampleRate);
                case ClipLevelParamType.HighZeroCrossingRateRatio:
                    return GetHighZeroCrossingRateRatio(zeroCrossingRate, parsedFile, sampleRate);
                default:
                    return 0.0;
            }
        }

        private static double GetEnergy(CustomPoint[] parsedFile, int samplesPerFrame, int framesCount, out double[] resultInFrame, bool takeRoot = false)
        {
            double avgResult = 0.0f;
            resultInFrame = new double[framesCount];

            for (int i = 0; i < framesCount; i++)
            {
                double squaredSum = 0.0f;
                for (int j = 0; j < samplesPerFrame; j++)
                {
                    int sampleIdx = i * samplesPerFrame + j;
                    if (sampleIdx >= parsedFile.Length)
                        break;

                    // Operations for each sample
                    squaredSum += parsedFile[sampleIdx].Y * parsedFile[sampleIdx].Y;
                    //
                }

                // Operations for each frame
                resultInFrame[i] = squaredSum / samplesPerFrame;
                if (takeRoot)
                    resultInFrame[i] = Math.Sqrt(resultInFrame[i]);
                //

                avgResult += resultInFrame[i];
            }

            avgResult /= framesCount;
            return avgResult;
        }

        private static double GetZeroCrossingRate(CustomPoint[] parsedFile, int samplesPerFrame, int framesCount, double sampleRate, out double[] resultInFrame)
        {
            double avgResult = 0.0f;
            resultInFrame = new double[framesCount];

            for (int i = 0; i < framesCount; i++)
            {
                double squaredSum = 0.0f;
                for (int j = 0; j < samplesPerFrame; j++)
                {
                    int sampleIdx = i * samplesPerFrame + j;
                    if (sampleIdx >= parsedFile.Length)
                        break;

                    // Operations for each sample
                    int signI = Math.Sign(parsedFile[sampleIdx].Y);
                    if (sampleIdx > 0)
                    {
                        sampleIdx--;
                        squaredSum += Math.Abs(signI - Math.Sign(parsedFile[sampleIdx].Y));
                    }
                    else
                    {
                        squaredSum += Math.Abs(signI);
                    }
                    //
                }

                // Operations for each frame
                resultInFrame[i] = (sampleRate * squaredSum) / (2 * samplesPerFrame);
                //

                avgResult += resultInFrame[i];
            }

            avgResult /= framesCount;
            return avgResult;
        }

        private static double GetSilentRatio(CustomPoint[] parsedFile, int samplesPerFrame, int framesCount, double sampleRate, out double[] resultInFrame)
        {
            double avgResult = 0.0f;
            resultInFrame = new double[framesCount];

            GetEnergy(parsedFile, samplesPerFrame, framesCount, out double[] volumeResultInFrame, true);
            GetZeroCrossingRate(parsedFile, samplesPerFrame, framesCount, sampleRate, out double[] zcrResultInFrame);

            for (int i = 0; i < framesCount; i++)
            {
                // Operations for each frame
                if (volumeResultInFrame[i] < 0.02 && zcrResultInFrame[i] < 50)
                    resultInFrame[i] = 1;
                //

                avgResult += resultInFrame[i];
            }

            avgResult /= framesCount;
            return avgResult;
        }
    
        private static double GetVolumeStandardDeviation(double[] volume)
        {
            if (volume.Length <= 1)
                return 0.0;

            var avg = volume.Average();
            var sum = volume.Sum(value => (value - avg) * (value - avg));
            sum /= volume.Length;

            return Math.Sqrt(sum) / volume.Max();
        }

        private static double GetVolumeDynamicRange(double[] volume)
        {
            var max = volume.Max();
            var min = volume.Min();

            return (max - min) / max;
        }

        private static double GetLowShortTimeEnergyRatio(double[] energy, CustomPoint[] parsedFile, double sampleRate)
        {
            int framesCount = parsedFile.Length / (int)sampleRate;
            if (parsedFile.Length % sampleRate != 0)
                framesCount++;

            GetEnergy(parsedFile, (int)sampleRate, framesCount, out double[] energyInOneSecFrame);

            var avg = energyInOneSecFrame.Average();
            var sum = energy.Sum(value => Math.Sign(0.5 * avg - value) + 1);

            return sum / (2.0 * energy.Length);
        }

        private static double GetHighZeroCrossingRateRatio(double[] zeroCrossingRate, CustomPoint[] parsedFile, double sampleRate)
        {
            int framesCount = parsedFile.Length / (int)sampleRate;
            if (parsedFile.Length % sampleRate != 0)
                framesCount++;

            GetZeroCrossingRate(parsedFile, (int)sampleRate, framesCount, sampleRate, out double[] zcrInOneSecFrame);

            var avg = zcrInOneSecFrame.Average();
            var sum = zeroCrossingRate.Sum(value => Math.Sign(value - 1.5 * avg) + 1);

            return sum / (2.0 * zeroCrossingRate.Length);
        }
    }
}
