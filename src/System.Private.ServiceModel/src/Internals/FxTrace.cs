// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.Diagnostics;

namespace System
{
    internal static partial class FxTrace
    {
        private const string baseEventSourceName = "System.ServiceModel";
        private const string EventSourceVersion = "4.0.0.0";

        private static Guid s_etwProviderId;
        private static string s_eventSourceName;
        private static EtwDiagnosticTrace s_diagnosticTrace;
        private static ExceptionTrace s_exceptionTrace;
        private static readonly object s_lockObject = new object();

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "This template is shared across all assemblies, some of which use this accessor.")]


        public static ExceptionTrace Exception
        {
            get
            {
                if (s_exceptionTrace == null)
                {
                    // don't need a lock here since a true singleton is not required
                    s_exceptionTrace = new ExceptionTrace(EventSourceName, Trace);
                }

                return s_exceptionTrace;
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "This template is shared across all assemblies, some of which use this accessor.")]
        public static EtwDiagnosticTrace Trace
        {
            get
            {
                EnsureEtwProviderInitialized();
                return FxTrace.s_diagnosticTrace;
            }
        }


        private static string EventSourceName
        {
            get
            {
                if (s_eventSourceName == null)
                {
                    s_eventSourceName = string.Concat(baseEventSourceName, " ", EventSourceVersion);
                }

                return s_eventSourceName;
            }
        }

        public static bool ShouldTraceInformation { get; internal set; } = false;
        public static bool ShouldTraceWarning { get; set; } = false;
        public static bool ShouldTraceError { get; set; } = false;
        public static bool ShouldTraceVerbose { get; set; } = false;
        public static bool ShouldTraceWarningToTraceSource { get; set; } = false;
        public static bool ShouldTraceInformationToTraceSource { get; set; } = false;
        public static bool ShouldTraceCritical { get; set; } = false;

        [SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.UseNewGuidHelperRule,
            Justification = "This is a method that creates ETW provider passing Guid Provider ID.")]
        private static EtwDiagnosticTrace InitializeTracing()
        {
            //Etw tracing is switched off by not enabling the session
            s_etwProviderId = EtwDiagnosticTrace.DefaultEtwProviderId;

            EtwDiagnosticTrace trace = new EtwDiagnosticTrace(baseEventSourceName, s_etwProviderId);

            return trace;
        }


        private static void EnsureEtwProviderInitialized()
        {
            if (null == FxTrace.s_diagnosticTrace)
            {
                lock (FxTrace.s_lockObject)
                {
                    if (null == FxTrace.s_diagnosticTrace)
                    {
                        FxTrace.s_diagnosticTrace = InitializeTracing();
                    }
                }
            }
        }

        public static void UpdateEventDefinitions(EventDescriptor[] descriptors, ushort[] toArray)
        {
           // throw new NotImplementedException();
        }

        public static bool IsEventEnabled(int eventIndex)
        {
            // TODO: add event trace support
            return false;
        }
    }
}
