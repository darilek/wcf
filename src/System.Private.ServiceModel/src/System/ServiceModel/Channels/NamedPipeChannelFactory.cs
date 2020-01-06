namespace System.ServiceModel.Channels
{
    internal class NamedPipeChannelFactory<TChannel> : ConnectionOrientedTransportChannelFactory<TChannel>, IPipeTransportFactorySettings, IConnectionOrientedTransportChannelFactorySettings, IConnectionOrientedTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts, IConnectionOrientedConnectionSettings
    {
        private static NamedPipeConnectionPoolRegistry connectionPoolRegistry = new NamedPipeConnectionPoolRegistry();

        public NamedPipeChannelFactory(
            NamedPipeTransportBindingElement bindingElement,
            BindingContext context)
            : base((ConnectionOrientedTransportBindingElement)bindingElement, context, NamedPipeChannelFactory<TChannel>.GetConnectionGroupName(bindingElement), bindingElement.ConnectionPoolSettings.IdleTimeout, bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint, false)
        {
            if (bindingElement.PipeSettings == null)
                return;
            this.PipeSettings = bindingElement.PipeSettings.Clone();
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeNetPipe;
            }
        }

        public NamedPipeSettings PipeSettings { get; private set; }

        private static string GetConnectionGroupName(NamedPipeTransportBindingElement bindingElement)
        {
            return bindingElement.ConnectionPoolSettings.GroupName + bindingElement.PipeSettings.ApplicationContainerSettings.GetConnectionGroupSuffix();
        }

        internal override IConnectionInitiator GetConnectionInitiator()
        {
            return (IConnectionInitiator)new BufferedConnectionInitiator((IConnectionInitiator)new PipeConnectionInitiator(this.ConnectionBufferSize, (IPipeTransportFactorySettings)this), this.MaxOutputDelay, this.ConnectionBufferSize);
        }

        internal override ConnectionPool GetConnectionPool()
        {
            return NamedPipeChannelFactory<TChannel>.connectionPoolRegistry.Lookup((IConnectionOrientedTransportChannelFactorySettings)this);
        }

        internal override void ReleaseConnectionPool(ConnectionPool pool, TimeSpan timeout)
        {
            NamedPipeChannelFactory<TChannel>.connectionPoolRegistry.Release(pool, timeout);
        }

        protected override bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return !(upgradeBindingElement is SslStreamSecurityBindingElement);
        }
    }
}