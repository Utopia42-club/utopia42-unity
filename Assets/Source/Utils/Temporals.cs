using System;

namespace Source.Utils
{
    public class Temporals
    {
        public static DateTime Epoch()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }
        
        public static DateTime FromEpochSeconds(long seconds)
        {
            return Epoch().AddSeconds(seconds);
        }
    }
}