using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.ServiceModel.Channels
{
    internal class NamedPipeConnectionPoolRegistry : ConnectionPoolRegistry
    {
        protected override ConnectionPool CreatePool(
            IConnectionOrientedTransportChannelFactorySettings settings)
        {
            return (ConnectionPool)new NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool((IPipeTransportFactorySettings)settings);
        }

        private class NamedPipeConnectionPool : ConnectionPool
        {
            private NamedPipeConnectionPoolRegistry.PipeNameCache pipeNameCache;
            private IPipeTransportFactorySettings transportFactorySettings;

            public NamedPipeConnectionPool(IPipeTransportFactorySettings settings)
                : base((IConnectionOrientedTransportChannelFactorySettings)settings, TimeSpan.MaxValue)
            {
                this.pipeNameCache = new NamedPipeConnectionPoolRegistry.PipeNameCache();
                this.transportFactorySettings = settings;
            }

            protected override CommunicationPool<string, IConnection>.EndpointConnectionPool CreateEndpointConnectionPool(
                string key)
            {
                return (CommunicationPool<string, IConnection>.EndpointConnectionPool)new NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool.NamedPipeEndpointConnectionPool(this, key);
            }

            protected override string GetPoolKey(EndpointAddress address, Uri via)
            {
                string pipeName;
                lock (this.ThisLock)
                {
                    if (!this.pipeNameCache.TryGetValue(via, out pipeName))
                    {
                        pipeName = PipeConnectionInitiator.GetPipeName(via, this.transportFactorySettings);
                        this.pipeNameCache.Add(via, pipeName);
                    }
                }
                return pipeName;
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.pipeNameCache.Clear();
            }

            private void OnConnectionAborted(string pipeName)
            {
                lock (this.ThisLock)
                    this.pipeNameCache.Purge(pipeName);
            }

            protected class NamedPipeEndpointConnectionPool : IdlingCommunicationPool<string, IConnection>.IdleTimeoutEndpointConnectionPool
            {
                private NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool parent;

                public NamedPipeEndpointConnectionPool(
                    NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool parent,
                    string key)
                    : base((IdlingCommunicationPool<string, IConnection>)parent, key)
                {
                    this.parent = parent;
                }

                protected override void OnConnectionAborted()
                {
                    this.parent.OnConnectionAborted(this.Key);
                }
            }
        }

        private class PipeNameCache
        {
            private Dictionary<Uri, string> forwardTable = new Dictionary<Uri, string>();
            private Dictionary<string, ICollection<Uri>> reverseTable = new Dictionary<string, ICollection<Uri>>();

            public void Add(Uri uri, string pipeName)
            {
                this.forwardTable.Add(uri, pipeName);
                ICollection<Uri> uris;
                if (!this.reverseTable.TryGetValue(pipeName, out uris))
                {
                    uris = (ICollection<Uri>)new Collection<Uri>();
                    this.reverseTable.Add(pipeName, uris);
                }
                uris.Add(uri);
            }

            public void Clear()
            {
                this.forwardTable.Clear();
                this.reverseTable.Clear();
            }

            public void Purge(string pipeName)
            {
                ICollection<Uri> uris;
                if (!this.reverseTable.TryGetValue(pipeName, out uris))
                    return;
                this.reverseTable.Remove(pipeName);
                foreach (Uri key in (IEnumerable<Uri>)uris)
                    this.forwardTable.Remove(key);
            }

            public bool TryGetValue(Uri uri, out string pipeName)
            {
                return this.forwardTable.TryGetValue(uri, out pipeName);
            }
        }
    }
}