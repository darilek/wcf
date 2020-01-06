using System.Runtime;

namespace System.ServiceModel.Channels
{
    /// <summary>Represents settings that control the behavior of the named pipe connection pool.</summary>
    public sealed class NamedPipeConnectionPoolSettings
    {
        private string groupName;
        private TimeSpan idleTimeout;
        private int maxOutputConnectionsPerEndpoint;

        internal NamedPipeConnectionPoolSettings()
        {
            this.groupName = "default";
            this.idleTimeout = ConnectionOrientedTransportDefaults.IdleTimeout;
            this.maxOutputConnectionsPerEndpoint = 10;
        }

        internal NamedPipeConnectionPoolSettings(NamedPipeConnectionPoolSettings namedPipe)
        {
            this.groupName = namedPipe.groupName;
            this.idleTimeout = namedPipe.idleTimeout;
            this.maxOutputConnectionsPerEndpoint = namedPipe.maxOutputConnectionsPerEndpoint;
        }

        /// <summary>Gets or sets the group name of the connection pool group on the client. </summary>
        /// <returns>The name of the connection pool group on the client. The default name is "default".</returns>
        /// <exception cref="T:System.ArgumentNullException">The value is <see langword="null" />.</exception>
        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                this.groupName = value;
            }
        }

        /// <summary>Gets or sets the maximum time the connection can be idle in the connection pool before being disconnected.</summary>
        /// <returns>Returns a <see cref="T:System.TimeSpan" /> structure that indicates the maximum time the connection can be idle in the connection pool before being disconnected.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than zero.</exception>
        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value), (object)value, SR.Format("SFxTimeoutOutOfRange0")));
                if (TimeoutHelper.IsTooLarge(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value), (object)value, SR.Format("SFxTimeoutOutOfRangeTooBig")));
                this.idleTimeout = value;
            }
        }

        /// <summary>Gets or sets the maximum number of outbound connections for each endpoint that is cached in the connection pool.</summary>
        /// <returns>The maximum number of allowed outbound connections for each endpoint that is cached in the connection pool. The default value is 10.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than or equal to zero.</exception>
        public int MaxOutboundConnectionsPerEndpoint
        {
            get
            {
                return this.maxOutputConnectionsPerEndpoint;
            }
            set
            {
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value), (object)value, SR.Format("ValueMustBeNonNegative")));
                this.maxOutputConnectionsPerEndpoint = value;
            }
        }

        internal NamedPipeConnectionPoolSettings Clone()
        {
            return new NamedPipeConnectionPoolSettings(this);
        }

        internal bool IsMatch(NamedPipeConnectionPoolSettings namedPipe)
        {
            return !(this.groupName != namedPipe.groupName) && !(this.idleTimeout != namedPipe.idleTimeout) && this.maxOutputConnectionsPerEndpoint == namedPipe.maxOutputConnectionsPerEndpoint;
        }
    }
}
