using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Diagnostics;
using System.Security.Principal;

namespace System.ServiceModel.Channels
{
    /// <summary>Represents the binding element for the named pipe transport.</summary>
    public class NamedPipeTransportBindingElement : ConnectionOrientedTransportBindingElement
    {
        private List<SecurityIdentifier> allowedUsers = new List<SecurityIdentifier>();
        private NamedPipeConnectionPoolSettings connectionPoolSettings = new NamedPipeConnectionPoolSettings();
        private NamedPipeSettings settings = new NamedPipeSettings();
        private Collection<SecurityIdentifier> allowedUsersCollection;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.Channels.NamedPipeTransportBindingElement" /> class. </summary>
        public NamedPipeTransportBindingElement()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.Channels.NamedPipeTransportBindingElement" /> class.</summary>
        /// <param name="elementToBeCloned">An instance of the <see cref="T:System.ServiceModel.Channels.NamedPipeTransportBindingElement" /> class.</param>
        protected NamedPipeTransportBindingElement(NamedPipeTransportBindingElement elementToBeCloned)
            : base((ConnectionOrientedTransportBindingElement)elementToBeCloned)
        {
            if (elementToBeCloned.allowedUsers != null)
            {
                foreach (SecurityIdentifier allowedUser in elementToBeCloned.allowedUsers)
                    this.allowedUsers.Add(allowedUser);
            }
            this.connectionPoolSettings = elementToBeCloned.connectionPoolSettings.Clone();
            this.settings = elementToBeCloned.settings.Clone();
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
            set
            {
                this.allowedUsers = value;
            }
        }

        /// <summary>Gets a collection of allowed <see cref="T:System.Security.Principal.SecurityIdentifier" /> instances.</summary>
        /// <returns>A collection of allowed <see cref="T:System.Security.Principal.SecurityIdentifier" /> instances.</returns>
        public Collection<SecurityIdentifier> AllowedSecurityIdentifiers
        {
            get
            {
                if (this.allowedUsersCollection == null)
                    this.allowedUsersCollection = new Collection<SecurityIdentifier>((IList<SecurityIdentifier>)this.allowedUsers);
                return this.allowedUsersCollection;
            }
        }

        /// <summary>Gets a collection of connection pool settings. </summary>
        /// <returns>A <see cref="T:System.ServiceModel.Channels.NamedPipeConnectionPoolSettings" /> object that contains various properties related to connection pooling.</returns>
        public NamedPipeConnectionPoolSettings ConnectionPoolSettings
        {
            get
            {
                return this.connectionPoolSettings;
            }
        }

        /// <summary>Gets the pipe settings for the named pipe transport binding element.</summary>
        /// <returns>The pipe settings for the named pipe transport binding element.</returns>
        public NamedPipeSettings PipeSettings
        {
            get
            {
                return this.settings;
            }
        }

        /// <summary>Returns the URI scheme for the transport.</summary>
        /// <returns>Returns the URI scheme for the transport, which is "net.pipe".</returns>
        public override string Scheme
        {
            get
            {
                return "net.pipe";
            }
        }

      /*  internal override string WsdlTransportUri
        {
            get
            {
                return "http://schemas.microsoft.com/soap/named-pipe";
            }
        }*/

        /// <summary>Creates a copy of the current binding element.</summary>
        /// <returns>Returns a copy of the current binding element.</returns>
        public override BindingElement Clone()
        {
            return (BindingElement)new NamedPipeTransportBindingElement(this);
        }

        /// <summary>Creates a channel factory of the specified type that can be used to create channels.</summary>
        /// <param name="context">Members that describe bindings, behaviors, contracts and other information required to create the channel factory.</param>
        /// <typeparam name="TChannel">Type of channel factory to create.</typeparam>
        /// <returns>Returns a channel factory of the specified type.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> cannot be <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid argument was passed.</exception>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(
            BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(context));
            if (!this.CanBuildChannelFactory<TChannel>(context))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(TChannel), SR.Format("ChannelTypeNotSupported", (object)typeof(TChannel)));
            return (IChannelFactory<TChannel>)new NamedPipeChannelFactory<TChannel>(this, context);
        }

     /*   /// <summary>Creates a channel listener of the specified type.</summary>
        /// <param name="context">Members that describe bindings, behaviors, contracts and other information required to create the channel factory.</param>
        /// <typeparam name="TChannel">Type of channel listener to create.</typeparam>
        /// <returns>Returns a channel listener of the specified type.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> cannot be <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid argument was passed.</exception>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(
            BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(context));
            if (!this.CanBuildChannelListener<TChannel>(context))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(TChannel), SR.Format("ChannelTypeNotSupported", (object)typeof(TChannel)));
            NamedPipeChannelListener pipeChannelListener;
            if (typeof(TChannel) == typeof(IReplyChannel))
                pipeChannelListener = (NamedPipeChannelListener)new NamedPipeReplyChannelListener(this, context);
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                pipeChannelListener = (NamedPipeChannelListener)new NamedPipeDuplexChannelListener(this, context);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(TChannel), SR.Format("ChannelTypeNotSupported", (object)typeof(TChannel)));
            AspNetEnvironment.Current.ApplyHostedContext((TransportChannelListener)pipeChannelListener, context);
            return (IChannelListener<TChannel>)pipeChannelListener;
        } */

        /// <summary>Gets a specified object from the <see cref="T:System.ServiceModel.Channels.BindingContext" />.</summary>
        /// <param name="context">A <see cref="T:System.ServiceModel.Channels.BindingContext" />.</param>
        /// <typeparam name="T">The object to get.</typeparam>
        /// <returns>The specified object from the <see cref="T:System.ServiceModel.Channels.BindingContext" />, or <see langword="null" /> if the object isn't found.</returns>
        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(context));
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            return typeof(T) == typeof(NamedPipeSettings) ? (T)(object)this.PipeSettings : base.GetProperty<T>(context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            return base.IsMatch(b) && b is NamedPipeTransportBindingElement transportBindingElement && (this.ConnectionPoolSettings.IsMatch(transportBindingElement.ConnectionPoolSettings) && this.PipeSettings.IsMatch(transportBindingElement.PipeSettings));
        }

        private class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }

            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get
                {
                    return true;
                }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get
                {
                    return false;
                }
            }
        }
    }
}
