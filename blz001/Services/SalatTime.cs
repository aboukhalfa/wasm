//--------------------- Copyright Block ----------------------
/* 

PrayTime.cs: Prayer Times Calculator (ver 1.2)
Copyright (C) 2007-2010 PrayTimes.org

C# Code By: Jandost Khoso
Original JS Code By: Hamid Zarrabi-Zadeh

License: GNU General Public License, ver 3

TERMS OF USE:
	Permission is granted to use this code, with or 
	without modification, in any website or application 
	provided that credit is given to the original work 
	with a link back to PrayTimes.org.

This program is distributed in the hope that it will 
be useful, but WITHOUT ANY WARRANTY. 

PLEASE DO NOT REMOVE THIS COPYRIGHT BLOCK.

*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    #region Enums

    public enum CalculationMethod
    {
        //[Description("Egyptian General Authority of Survey")]
        Egypt,      // Egyptian General Authority of Survey
        //[Description("Islamic Society of North America (ISNA)")]
        Isna,       // Islamic Society of North America (ISNA)
        //[Description("University of Islamic Sciences, Karachi")]
        Karachi,    // University of Islamic Sciences, Karachi
        //[Description("Muslim World League (MWL)")]
        Mwl,        // Muslim World League (MWL)
        //[Description("Umm al-Qura, Makkah")]
        UmmAlQura,  // Umm al-Qura, Makkah
        //[Description("Custom Setting")]
        Custom,     // Custom Setting
        //[Description("Institute of Geophysics, University of Tehran")]
        Tehran,     // Institute of Geophysics, University of Tehran
        //[Description("Ithna Ashari/Jafari")]
        Jafari      // Ithna Ashari/Jafari
    }

    public enum AsrJuristicMethod
    {
        Shafii, //Standard
        Hanafi
    }

    public enum HighLatitudesAdjustment
    {
        None,       // No adjustment
        Midnight,   // Middle of night
        OneSeventh, // 1/7th of night
        AngleBased  // Angle/60th of night
    }

    public enum TimeFormats
    {
        Time24,         // 24-hour format
        Time12,         // 12-hour format
        Time12NoSuffix, // 12-hour format with no suffix
        Floating        // floating point number
    }

    #endregion

    public class SalatTime
    {

        #region Static Properties

        public static string[] TimeNames = new[] { "Fajr", "Sunrise", "Duhr", "Asr", "Sunset", "Maghrib", "Isha" };

        #endregion

        #region Fields

        private const String InvalidTime = "----";

        private CalculationMethod _calcMethod = CalculationMethod.Mwl;		// Caculation method
        private AsrJuristicMethod _asrJuristic;		// Juristic method for Asr
        private double _dhuhrMinutes;		            // minutes after mid-day for Dhuhr
        private HighLatitudesAdjustment _adjustHighLats = HighLatitudesAdjustment.Midnight;	// adjusting method for higher latitudes

        private TimeFormats _timeFormat = TimeFormats.Time24;		// time string format

        private double _lat;        // latitude
        private double _lng;        // longitude
        private double _timeZone;   // time-zone
        private double _jDate;      // Julian date

        private const int NumIterations = 1;		// number of iterations needed to compute times

        //------------------- Calc Method Parameters --------------------
        private readonly Dictionary<CalculationMethod, double[]> _methodParams = new Dictionary<CalculationMethod, double[]>();

        #endregion

        #region Constructor

        public SalatTime()
        {
            _methodParams[CalculationMethod.Jafari] = new[] { 16.0, 0, 4, 0, 14 };
            _methodParams[CalculationMethod.Karachi] = new[] { 18.0, 1, 0, 0, 18 };
            _methodParams[CalculationMethod.Isna] = new[] { 15.0, 1, 0, 0, 15 };
            _methodParams[CalculationMethod.Mwl] = new[] { 18.0, 1, 0, 0, 17 };
            _methodParams[CalculationMethod.UmmAlQura] = new[] { 18.5, 1, 0, 1, 90 };
            _methodParams[CalculationMethod.Egypt] = new[] { 19.5, 1, 0, 0, 17.5 };
            _methodParams[CalculationMethod.Tehran] = new[] { 17.7, 0, 4.5, 0, 14 };
            _methodParams[CalculationMethod.Custom] = new[] { 18.0, 1, 0, 0, 17 };
        }

        #endregion

        #region Public methods

        // return prayer times for a given date
        public String[] GetPrayerTimes(int year, int month, int day, double latitude, double longitude, int timeZone)
        {
            return GgetDatePrayerTimes(year, month + 1, day, latitude, longitude, timeZone);
        }

        // return prayer times for a given date
        public String[] GgetDatePrayerTimes(int year, int month, int day, double latitude, double longitude, int timeZone)
        {
            _lat = latitude;
            _lng = longitude;
            _timeZone = timeZone;
            _jDate = JulianDate(year, month, day) - longitude / (15 * 24);

            return ComputeDayTimes();
        }

        // Get times (no sunset)
        public IEnumerable<DateTime> GetTimes(int year, int month, int day, double latitude, double longitude, int timeZone)
        {
            var times = GetPrayerTimesAsDateTime(year, month, day, latitude, longitude, timeZone).ToList();
            // This is for no sunset: return times.Take(4).Concat(times.Skip(5));
            return times;
        }

        // Get prayer times for a given date (DateTime)
        public IEnumerable<DateTime> GetPrayerTimesAsDateTime(DateTime date, double latitude, double longitude, int timeZone)
        {
            return GetPrayerTimesAsDateTime(date.Year, date.Month, date.Day, latitude, longitude, timeZone);
        }

        // set the calculation method
        public void SetCalcMethod(CalculationMethod methodId)
        {
            _calcMethod = methodId;
        }

        // set the juristic method for Asr
        public void SetAsrMethod(AsrJuristicMethod methodId)
        {
            _asrJuristic = methodId;
        }

        // set the angle for calculating Fajr
        public void SetFajrAngle(double angle)
        {
            SetCustomParams(new[] { (int)angle, -1, -1, -1, -1 });
        }

        // set the angle for calculating Maghrib
        public void SetMaghribAngle(double angle)
        {
            SetCustomParams(new[] { -1, 0, (int)angle, -1, -1 });
        }

        // set the angle for calculating Isha
        public void SsetIshaAngle(double angle)
        {
            SetCustomParams(new[] { -1, -1, -1, 0, (int)angle });
        }

        // set the minutes after mid-day for calculating Dhuhr
        public void SetDhuhrMinutes(int minutes)
        {
            _dhuhrMinutes = minutes;
        }

        // set the minutes after Sunset for calculating Maghrib
        public void SetMaghribMinutes(int minutes)
        {
            SetCustomParams(new[] { -1, 1, minutes, -1, -1 });
        }

        // set the minutes after Maghrib for calculating Isha
        public void SetIshaMinutes(int minutes)
        {
            SetCustomParams(new[] { -1, -1, -1, 1, minutes });
        }

        // set custom values for calculation parameters
        public void SetCustomParams(int[] param)
        {
            for (int i = 0; i < 5; i++)
            {
                if (param[i] == -1)
                    _methodParams[CalculationMethod.Custom][i] = _methodParams[_calcMethod][i];
                else
                    _methodParams[CalculationMethod.Custom][i] = param[i];
            }

            _calcMethod = CalculationMethod.Custom;
        }

        // set adjusting method for higher latitudes
        public void SetHighLatsMethod(HighLatitudesAdjustment methodId)
        {
            _adjustHighLats = methodId;
        }

        // set the time format
        public void SetTimeFormat(TimeFormats timeFormat)
        {
            _timeFormat = timeFormat;
        }

        #endregion

        #region Private methods

        // Get prayer times for a given date (DateTime) including sunset time
        private IEnumerable<DateTime> GetPrayerTimesAsDateTime(int year, int month, int day, double latitude, double longitude, int timeZone)
        {
            _lat = latitude;
            _lng = longitude;
            _timeZone = timeZone;
            _jDate = JulianDate(year, month, day) - longitude / (15 * 24);

            var times = ComputeDayTimesAsDoubles();
            var result = new DateTime[times.Length];

            for (int i = 0; i < times.Length; i++)
            {
                double dtime = times[i];
                if (dtime < 0)
                {
                    result[i] = DateTime.MinValue;
                    continue;
                }

                dtime = FixHour(dtime + 0.5 / 60);  // add 0.5 minutes to round
                var hours = (int)Math.Floor(dtime);
                var minutes = (int)Math.Floor((dtime - hours) * 60);
                result[i] = new DateTime(year, month, day, hours, minutes, 0);
                var dayIncr = Math.Floor(times[i] / 24.0);
                result[i] = result[i].AddDays(dayIncr);
            }

            return result;
        }

        // convert float hours to 24h format
        private String FloatToTime24(double time)
        {
            if (time < 0)
                return InvalidTime;
            time = FixHour(time + 0.5 / 60);  // add 0.5 minutes to round
            double hours = Math.Floor(time);
            double minutes = Math.Floor((time - hours) * 60);
            return TwoDigitsFormat((int)hours) + ":" + TwoDigitsFormat((int)minutes);
        }

        // convert float hours to 12h format
        private String FloatToTime12(double time, bool noSuffix)
        {
            if (time < 0)
                return InvalidTime;
            time = FixHour(time + 0.5 / 60);  // add 0.5 minutes to round
            double hours = Math.Floor(time);
            double minutes = Math.Floor((time - hours) * 60);
            String suffix = hours >= 12 ? " pm" : " am";
            hours = (hours + 12 - 1) % 12 + 1;
            return ((int)hours) + ":" + TwoDigitsFormat((int)minutes) + (noSuffix ? "" : suffix);
        }

        // convert float hours to 12h format with no suffix
        private String FloatToTime12Ns(double time)
        {
            return FloatToTime12(time, true);
        }

        //---------------------- Compute Prayer Times -----------------------


        // compute declination angle of sun and equation of time
        private double[] SunPosition(double jd)
        {
            double bigD = jd - 2451545.0;
            double g = FixAngle(357.529 + 0.98560028 * bigD);
            double q = FixAngle(280.459 + 0.98564736 * bigD);
            double bigL = FixAngle(q + 1.915 * Dsin(g) + 0.020 * Dsin(2 * g));

            //double bigR = 1.00014 - 0.01671 * Dcos(g) - 0.00014 * Dcos(2 * g);
            double e = 23.439 - 0.00000036 * bigD;

            double d = Darcsin(Dsin(e) * Dsin(bigL));
            double bigRbigA = Darctan2(Dcos(e) * Dsin(bigL), Dcos(bigL)) / 15;
            bigRbigA = FixHour(bigRbigA);
            double bigEqT = q / 15 - bigRbigA;

            return new[] { d, bigEqT };
        }

        // compute equation of time
        private double EquationOfTime(double jd)
        {
            return SunPosition(jd)[1];
        }

        // compute declination angle of sun
        private double SunDeclination(double jd)
        {
            return SunPosition(jd)[0];
        }

        // compute mid-day (Dhuhr, Zawal) time
        private double ComputeMidDay(double t)
        {
            double T = EquationOfTime(_jDate + t);
            double bigZ = FixHour(12 - T);
            return bigZ;
        }

        // compute time for a given angle bigG
        private double ComputeTime(double bigG, double t)
        {
            //System.out.println("G: "+G);

            double bigD = SunDeclination(_jDate + t);
            double bigZ = ComputeMidDay(t);
            double bigV = ((double)1 / 15) * Darccos((-Dsin(bigG) - Dsin(bigD) * Dsin(_lat)) /
                    (Dcos(bigD) * Dcos(_lat)));
            return bigZ + (bigG > 90 ? -bigV : bigV);
        }

        // compute the time of Asr
        private double ComputeAsr(int step, double t)  // Shafii: step=1, Hanafi: step=2
        {
            double bigD = SunDeclination(_jDate + t);
            double bigG = -Darccot(step + Dtan(Math.Abs(_lat - bigD)));
            return ComputeTime(bigG, t);
        }

        //---------------------- Compute Prayer Times -----------------------

        // compute prayer times at given julian date
        private double[] ComputeTimes(double[] times)
        {
            double[] t = DayPortion(times);

            double fajr = ComputeTime(180 - _methodParams[_calcMethod][0], t[0]);
            double sunrise = ComputeTime(180 - 0.833, t[1]);
            double dhuhr = ComputeMidDay(t[2]);
            double ssr = ComputeAsr(_asrJuristic == AsrJuristicMethod.Shafii ? 1 : 2, t[3]);
            double sunset = ComputeTime(0.833, t[4]);
            double maghrib = ComputeTime(_methodParams[_calcMethod][2], t[5]);
            double isha = ComputeTime(_methodParams[_calcMethod][4], t[6]);

            return new[] { fajr, sunrise, dhuhr, ssr, sunset, maghrib, isha };
        }

        // adjust Fajr, Isha and Maghrib for locations in higher latitudes
        private double[] AdjustHighLatTimes(double[] times)
        {
            double nightTime = GetTimeDifference(times[4], times[1]); // sunset to sunrise

            // Adjust Fajr
            double fajrDiff = NightPortion(_methodParams[_calcMethod][0]) * nightTime;
            if (GetTimeDifference(times[0], times[1]) > fajrDiff)
                times[0] = times[1] - fajrDiff;

            // Adjust Isha
            double ishaAngle = (_methodParams[_calcMethod][3] < 0.001) ? _methodParams[_calcMethod][4] : 18;
            double ishaDiff = NightPortion(ishaAngle) * nightTime;
            if (GetTimeDifference(times[4], times[6]) > ishaDiff)
                times[6] = times[4] + ishaDiff;

            // Adjust Maghrib
            double maghribAngle = (_methodParams[_calcMethod][1] < 0.001) ? _methodParams[_calcMethod][2] : 4;
            double maghribDiff = NightPortion(maghribAngle) * nightTime;
            if (GetTimeDifference(times[4], times[5]) > maghribDiff)
                times[5] = times[4] + maghribDiff;

            return times;
        }

        // the night portion used for adjusting times in higher latitudes
        private double NightPortion(double angle)
        {
            double val = 0;
            if (_adjustHighLats == HighLatitudesAdjustment.AngleBased)
                val = 1.0 / 60.0 * angle;
            if (_adjustHighLats == HighLatitudesAdjustment.Midnight)
                val = 1.0 / 2.0;
            if (_adjustHighLats == HighLatitudesAdjustment.OneSeventh)
                val = 1.0 / 7.0;

            return val;
        }

        private double[] DayPortion(double[] times)
        {
            for (int i = 0; i < times.Length; i++)
            {
                times[i] /= 24;
            }
            return times;
        }

        // compute prayer times at given julian date
        private String[] ComputeDayTimes()
        {
            var times = ComputeDayTimesAsDoubles();
            return AdjustTimesFormat(times);
        }

        // compute prayer times at given julian date as doubles
        private double[] ComputeDayTimesAsDoubles()
        {
            double[] times = { 5, 6, 12, 13, 18, 18, 18 }; //default times

            for (int i = 0; i < NumIterations; i++)
            {
                times = ComputeTimes(times);
            }

            times = AdjustTimes(times);
            return times;
        }

        // adjust times in a prayer time array
        public double[] AdjustTimes(double[] times)
        {
            for (int i = 0; i < 7; i++)
            {
                times[i] += _timeZone - _lng / 15;
            }
            times[2] += _dhuhrMinutes / 60; //Dhuhr
            if (_methodParams[_calcMethod][1] == 1) // Maghrib
                times[5] = times[4] + _methodParams[_calcMethod][2] / 60.0;
            if (_methodParams[_calcMethod][3] == 1) // Isha
                times[6] = times[5] + _methodParams[_calcMethod][4] / 60.0;

            if (_adjustHighLats != HighLatitudesAdjustment.None)
            {
                times = AdjustHighLatTimes(times);
            }

            return times;
        }

        private String[] AdjustTimesFormat(double[] times)
        {
            var formatted = new String[times.Length];

            if (_timeFormat == TimeFormats.Floating)
            {
                for (int i = 0; i < times.Length; ++i)
                {
                    formatted[i] = times[i] + "";
                }
                return formatted;
            }
            for (int i = 0; i < 7; i++)
            {
                if (_timeFormat == TimeFormats.Time12)
                    formatted[i] = FloatToTime12(times[i], true);
                else if (_timeFormat == TimeFormats.Time12NoSuffix)
                    formatted[i] = FloatToTime12Ns(times[i]);
                else
                    formatted[i] = FloatToTime24(times[i]);
            }
            return formatted;
        }

        //---------------------- Misc Functions -----------------------

        // compute the difference between two times
        private double GetTimeDifference(double c1, double c2)
        {
            double diff = FixHour(c2 - c1);
            return diff;
        }

        // add a leading 0 if necessary
        private String TwoDigitsFormat(int num)
        {

            return (num < 10) ? "0" + num : num + "";
        }

        //---------------------- Julian Date Functions -----------------------

        // calculate julian date from a calendar date
        private double JulianDate(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }
            double bigA = Math.Floor(year / 100.0);
            double bigB = 2 - bigA + Math.Floor(bigA / 4);

            double bigJbigD = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + bigB - 1524.5;
            return bigJbigD;
        }


        //---------------------- Time-Zone Functions -----------------------


        // detect daylight saving in a given date
        //private bool UseDayLightaving(int year, int month, int day)
        //{
        //    return TimeZoneInfo.Local.IsDaylightSavingTime(new DateTime(year, month, day));
        //}

        // ---------------------- Trigonometric Functions -----------------------

        // degree sin
        private double Dsin(double d)
        {
            return Math.Sin(DegreeToRadian(d));
        }

        // degree cos
        private double Dcos(double d)
        {
            return Math.Cos(DegreeToRadian(d));
        }

        // degree tan
        private double Dtan(double d)
        {
            return Math.Tan(DegreeToRadian(d));
        }

        // degree arcsin
        private double Darcsin(double x)
        {
            return RadianToDegree(Math.Asin(x));
        }

        // degree arccos
        private double Darccos(double x)
        {
            return RadianToDegree(Math.Acos(x));
        }

        // degree arctan
        //private double Darctan(double x)
        //{
        //    return RadianToDegree(Math.Atan(x));
        //}

        // degree arctan2
        private double Darctan2(double y, double x)
        {
            return RadianToDegree(Math.Atan2(y, x));
        }

        // degree arccot
        private double Darccot(double x)
        {
            return RadianToDegree(Math.Atan(1 / x));
        }

        // Radian to Degree
        private double RadianToDegree(double radian)
        {
            return (radian * 180.0) / Math.PI;
        }

        // degree to radian
        private double DegreeToRadian(double degree)
        {
            return (degree * Math.PI) / 180.0;
        }

        private double FixAngle(double angle)
        {
            angle = angle - 360.0 * (Math.Floor(angle / 360.0));
            angle = angle < 0 ? angle + 360.0 : angle;
            return angle;
        }

        // range reduce hours to 0..23
        private double FixHour(double hour)
        {
            hour = hour - 24.0 * (Math.Floor(hour / 24.0));
            hour = hour < 0 ? hour + 24.0 : hour;
            return hour;
        }

        #endregion
    }
}
