﻿using System;

namespace SoundAnalysis
{
    public struct TimeMarker
    {
        public TimeSpan Begin { get; set; }
        public TimeSpan End { get; set; }

        public TimeSpan Duration => End - Begin;

        public TimeMarker(TimeSpan begin, TimeSpan end)
        {
            Begin = begin;
            End = end;
        }

        public void ModifyEndMarker(TimeSpan newValue)
        {
            End = newValue;
        }

        public override string ToString()
        {
            return Begin + "," + End + "," + Duration.TotalMilliseconds;
        }

        public static TimeMarker FromSample(double sampleCenter, int milisecondsPerSample)
        {
            int sampleCenterMs = (int)(1000 * sampleCenter);

            var begin = new TimeSpan(0, 0, 0, 0, sampleCenterMs - milisecondsPerSample / 2);
            var end = new TimeSpan(0, 0, 0, 0, sampleCenterMs + milisecondsPerSample / 2);

            return new TimeMarker(begin, end);
        }
    }
}
