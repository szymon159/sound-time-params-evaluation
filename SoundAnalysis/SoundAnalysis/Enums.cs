﻿namespace SoundAnalysis
{
    public enum FrameLevelParamType
    {
        Volume,
        ShortTimeEnergy,
        ZeroCrossingRate,
        SilentRatio,
        SoundlessSpeech,
        SoundSpeech,
        Music,
        FrequencyVolume,
        FrequencyCentroid,
        EffectiveBandwidth,
        BandEnergy
    }

    public enum ClipLevelParamType
    {
        VolumeStandardDeviation,
        VolumeDynamicRange,
        LowShortTimeEnergyRatio,
        HighZeroCrossingRateRatio
    }

    public enum StatisticsType
    {
        Silence,
        SoundlessSpeech,
        SoundSpeech,
        Music
    }

    public enum WindowType
    {
        Rectangular,
        Hamming,
        Hann
    }

    public enum AnalysisType
    {
        SoundTimeParameters,
        Fourier,
        Spectrum,
        FundamentalFrequency,
        SoundFrequencyParameters
    }

    public enum FourierTransfromScope
    {
        WholeClip,
        OneFrame
    }
}
