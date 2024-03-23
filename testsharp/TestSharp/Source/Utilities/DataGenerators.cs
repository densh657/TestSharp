using System;
using System.Linq;
using TestSharp.Source.Utilities;

namespace TestSharp.Source.DataHelpers
{
    public static class DataGenerators
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Creates and returns random string
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Creates and returns random number
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomNum(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateFormats"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static string GetDate(string dateFormats = "M/d/yyyy", int addTime = 0, TimeType timeType = TimeType.Days)
        {
            var date = DateTime.Now;
            switch (timeType)
            {
                case TimeType.Days:
                    date.AddDays(addTime);
                    break;
                case TimeType.Hours:
                    date.AddHours(addTime);
                    break;
                case TimeType.Minutes:
                    date.AddMinutes(addTime);
                    break;
            }
            return date.ToString(dateFormats);
        }

    }
}