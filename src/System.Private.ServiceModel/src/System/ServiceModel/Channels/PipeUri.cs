using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace System.ServiceModel.Channels
{
    internal static class PipeUri
    {
        public static string BuildSharedMemoryName(
            Uri uri,
            HostNameComparisonMode hostNameComparisonMode,
            bool global)
        {
            string path = PipeUri.GetPath(uri);
            string hostName = (string)null;
            switch (hostNameComparisonMode)
            {
                case HostNameComparisonMode.StrongWildcard:
                    hostName = "+";
                    break;
                case HostNameComparisonMode.Exact:
                    hostName = uri.Host;
                    break;
                case HostNameComparisonMode.WeakWildcard:
                    hostName = "*";
                    break;
            }
            return PipeUri.BuildSharedMemoryName(hostName, path, global);
        }

        internal static string BuildSharedMemoryName(
            string hostName,
            string path,
            bool global,
            AppContainerInfo appContainerInfo)
        {
            if (appContainerInfo == null)
                return PipeUri.BuildSharedMemoryName(hostName, path, global);
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Session\\{0}\\{1}\\{2}", new object[3]
            {
                (object) appContainerInfo.SessionId,
                (object) appContainerInfo.NamedObjectPath,
                (object) PipeUri.BuildSharedMemoryName(hostName, path, global)
            });
        }

        private static string BuildSharedMemoryName(string hostName, string path, bool global)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            stringBuilder1.Append(Uri.UriSchemeNetPipe);
            stringBuilder1.Append("://");
            stringBuilder1.Append(hostName.ToUpperInvariant());
            stringBuilder1.Append(path);
            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder1.ToString());
            byte[] inArray;
            string str;
            if (bytes.Length >= 128)
            {
                using (HashAlgorithm hashAlgorithm = PipeUri.GetHashAlgorithm())
                    inArray = hashAlgorithm.ComputeHash(bytes);
                str = ":H";
            }
            else
            {
                inArray = bytes;
                str = ":E";
            }
            StringBuilder stringBuilder2 = new StringBuilder();
            if (global)
                stringBuilder2.Append("Global\\");
            else
                stringBuilder2.Append("Local\\");
            stringBuilder2.Append(Uri.UriSchemeNetPipe);
            stringBuilder2.Append(str);
            stringBuilder2.Append(Convert.ToBase64String(inArray));
            return stringBuilder2.ToString();
        }

        private static HashAlgorithm GetHashAlgorithm()
        {
            // return !System.ServiceModel.LocalAppContextSwitches.UseSha1InPipeConnectionGetHashAlgorithm ? (SecurityUtilsEx.RequiresFipsCompliance ? (HashAlgorithm)new SHA256CryptoServiceProvider() : (HashAlgorithm)new SHA256Managed()) : (SecurityUtilsEx.RequiresFipsCompliance ? (HashAlgorithm)new SHA1CryptoServiceProvider() : (HashAlgorithm)new SHA1Managed());
            return new SHA256Managed();
        }

        public static string GetPath(Uri uri)
        {
            string upperInvariant = uri.LocalPath.ToUpperInvariant();
            if (!upperInvariant.EndsWith("/", StringComparison.Ordinal))
                upperInvariant += "/";
            return upperInvariant;
        }

        public static string GetParentPath(string path)
        {
            if (path.EndsWith("/", StringComparison.Ordinal))
                path = path.Substring(0, path.Length - 1);
            return path.Length == 0 ? path : path.Substring(0, path.LastIndexOf('/') + 1);
        }

        public static void Validate(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeNetPipe)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(uri), SR.Format("PipeUriSchemeWrong"));
        }
    }
}
