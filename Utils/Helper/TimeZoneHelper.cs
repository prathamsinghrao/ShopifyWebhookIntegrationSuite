using TimeZoneConverter;

namespace Utils.Helper
{
    public interface ITimeZoneHelper
    {
        DateTime ConvertUctToLocalTime(DateTime timeUtc, string timeZone);

        DateTime ConvertLocalTimeToUct(DateTime localtime, string timeZone);
    }

    public class TimeZoneHelper : ITimeZoneHelper
    {
        public DateTime ConvertUctToLocalTime(DateTime timeUtc, string timeZone)
        {
            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZone);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZoneInfo);
            return localTime;
        }

        public DateTime ConvertLocalTimeToUct(DateTime localtime, string timeZone)
        {
            DateTime now = DateTime.SpecifyKind(localtime, DateTimeKind.Unspecified);
            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZone);
            DateTime localTime = TimeZoneInfo.ConvertTimeToUtc(now, timeZoneInfo);
            return localTime;
        }
    }
}
