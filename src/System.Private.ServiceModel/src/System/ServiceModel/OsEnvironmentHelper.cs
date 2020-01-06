namespace System.ServiceModel
{
    internal static class OsEnvironmentHelper
    {
        private static readonly byte currentServicePack = (byte)Environment.OSVersion.Version.MajorRevision;
        private static readonly OSVersion currentVersion;

        static OsEnvironmentHelper()
        {
            int major = Environment.OSVersion.Version.Major;
            int minor = Environment.OSVersion.Version.Minor;
            if (major < 5 || major == 5 && minor == 0)
                OsEnvironmentHelper.currentVersion = OSVersion.PreWinXP;
            if (major == 5 && minor == 1)
                OsEnvironmentHelper.currentVersion = OSVersion.WinXP;
            else if (major == 5 && minor == 2)
                OsEnvironmentHelper.currentVersion = OSVersion.Win2003;
            else if (major == 6 && minor == 0)
                OsEnvironmentHelper.currentVersion = OSVersion.WinVista;
            else if (major == 6 && minor == 1)
                OsEnvironmentHelper.currentVersion = OSVersion.Win7;
            else if (major == 6 && minor == 2)
                OsEnvironmentHelper.currentVersion = OSVersion.Win8;
            else if (major > 6 || major == 6 && minor > 2)
                OsEnvironmentHelper.currentVersion = OSVersion.PostWin8;
            else
                OsEnvironmentHelper.currentVersion = OSVersion.Unknown;
        }

        internal static bool IsVistaOrGreater
        {
            get
            {
                return OsEnvironmentHelper.IsAtLeast(OSVersion.WinVista);
            }
        }

        internal static bool IsApplicationTargeting45
        {
            get
            {
                // return WebSocket.IsApplicationTargeting45();
                return true;
            }
        }

        internal static int ProcessorCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }

        internal static bool IsAtLeast(OSVersion version)
        {
            return OsEnvironmentHelper.IsAtLeast(version, (byte)0);
        }

        private static bool IsAtLeast(OSVersion version, byte servicePack)
        {
            if (servicePack == (byte)0)
                return version <= OsEnvironmentHelper.currentVersion;
            return version == OsEnvironmentHelper.currentVersion ? (int)servicePack <= (int)OsEnvironmentHelper.currentServicePack : version < OsEnvironmentHelper.currentVersion;
        }
    }
}
