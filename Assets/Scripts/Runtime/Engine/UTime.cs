
using System;

    public static class UTime
    {
        public const int m_StampADay = 86400;
        public const int m_StampAnHour = 3600;
        public static readonly DateTime m_StampBegin = new DateTime(1970, 1, 1, 8, 0, 0);

        public static int GetTimeStampNow() => GetTimeStamp(DateTime.Now);
        public static int GetTimeStamp(DateTime dt) => (int)(dt - m_StampBegin).TotalSeconds;

        public static int GetDayStampNow() => GetDayStamp(DateTime.Now);
        public static int GetDayStamp(DateTime dt) => (int)(dt - m_StampBegin).TotalDays;

        public static string GetHourMinuteSecond(int seconds) => string.Format("{0:D2}:{1:D2}:{2:D2}", seconds / 3600, (seconds % 3600) / 60, (seconds % 3600) % 60);

        public static string GetMinuteSecond(int seconds) => string.Format("{0:D2}:{1:D2}", (seconds % 3600) / 60, (seconds % 3600) % 60);

        public static DateTime GetDateTime(int timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = ((long)timeStamp * 10000000);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }

        public static DateTime GetDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
    }

