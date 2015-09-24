using System;
using System.Configuration;

namespace BullySharp.Core
{
    public static class Settings
    {
        public static TimeSpan ElectionMessageReceiveTimeout = TimeSpan.FromMilliseconds(1000);

        public static TimeSpan ElectionMessageReceiveLinger = TimeSpan.FromMilliseconds(1000);

        public static TimeSpan ElectionInterval = TimeSpan.FromMilliseconds(2500);

        public static TimeSpan VictoryMessageTimeout = TimeSpan.FromMilliseconds(1000);

        public static TimeSpan PingTimeout = TimeSpan.FromMilliseconds(1000);

        public static TimeSpan PingInterval = TimeSpan.FromMilliseconds(2500);

        public static string PeerId => ConfigurationManager.AppSettings.Get("PeerId");

        public static string ElectionEndpoints => ConfigurationManager.AppSettings.Get("ElectionEndpoints");

        public static string ElectionListenerEndpoint => ConfigurationManager.AppSettings.Get("ElectionListenerEndpoint");

        public static string LeaderEndpoints => ConfigurationManager.AppSettings.Get("LeaderEndpoints");

        public static string LeaderListenerEndpoint => ConfigurationManager.AppSettings.Get("LeaderListenerEndpoint");

        public static string PingEndpoints => ConfigurationManager.AppSettings.Get("PingEndpoints");

        public static string PingListenerEndpoint => ConfigurationManager.AppSettings.Get("PingListenerEndpoint");
    }
}
