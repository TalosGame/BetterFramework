using System;
using UnityEngine;

public class TimeUtil 
{
    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1);
    private static TimeSpan timeOffest = new TimeSpan(0);

	static public DateTime GetNow()
	{
		return DateTime.Now + timeOffest;
	}

	public static long DateTimeToMS(DateTime date)
	{
        return (long)((date.ToUniversalTime().Ticks - Jan1st1970.Ticks) / 10000);
	}

    /// <summary>
    /// 获取系统毫秒值
    /// </summary>
    /// <returns></returns>
    public static long CurrentTimeMillis()
    {
        return (DateTime.UtcNow.Ticks - Jan1st1970.Ticks) / TimeSpan.TicksPerMillisecond;
    }

	/// <summary>
	/// 通过系统毫秒转化为时间
	/// </summary>
	/// <param name="milliSecs"></param>
	/// <returns></returns>
	public static DateTime ConvertMiliSecondToDate(long milliSecs)
    {
        DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime dt = UTCBaseTime.Add(new TimeSpan(milliSecs * TimeSpan.TicksPerMillisecond)).ToLocalTime();
        return dt;
    }

    /// <summary>
    /// 获取与当前时间的差(毫秒)
    /// <param name="time">比较的Date</param>
    /// <returns></returns>
    public static long GetDiffMiliSecFromNow(DateTime time)
    {
        //Debug.Log("之前Date：" + time.ToString());
        //Debug.Log("现在Date：" + DateTime.Now.ToString());

        var span = DateTime.Now - time;
        return (long)span.TotalMilliseconds;
    }

	/// <summary>
	/// 获取2个时间差(秒)
	/// <param name="time1">比较的Date1</param>
	/// <param name="time2">比较的Date2</param>
	/// <returns></returns>
	public static long GetDiffMiliSecFrom2Time(DateTime time1, DateTime time2)
    {
        //Debug.Log("Date1:" + time1);
        //Debug.Log("Date2:" + time2);

        long ret = (time2.Ticks - time1.Ticks) / TimeSpan.TicksPerMillisecond;
        return ret;
    }

	/// <summary>
	/// 通过unix时间戳格式化为年-月-日 小时-分
	/// </summary>
	/// <param name="unixTimeStamp">unix 时间戳</param>
	/// <returns></returns>
	public static string ConvertToTimeString(long unixTimeStamp)
	{
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(Jan1st1970); // 当地时区
		DateTime dt = startTime.AddSeconds(unixTimeStamp);
		return dt.ToString("yyyy-MM-dd HH:mm");
	}
}
