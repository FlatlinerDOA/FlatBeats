namespace FlatBeats.DataModel.Services
{
    using System;

    public static class PlatformHelper
    {
        private static readonly Version TargetedVersion8 = new Version(8, 0);

        private static readonly Version TargetedVersion78 = new Version(7, 10, 8858); 

        public static bool IsWindowsPhone8OrLater { get { return Environment.OSVersion.Version >= TargetedVersion8; } }

        public static bool IsWindowsPhone78OrLater { get { return Environment.OSVersion.Version >= TargetedVersion78; } }
    }
}
