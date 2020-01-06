using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal static class MessagingVersionHelper
    {
        public static AddressingVersion AddressingVersion(ProtocolVersion protocolVersion)
        {
            //ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), nameof(AddressingVersion));
            if (protocolVersion == ProtocolVersion.Version10)
                return System.ServiceModel.Channels.AddressingVersion.WSAddressingAugust2004;
            return protocolVersion == ProtocolVersion.Version11 ? System.ServiceModel.Channels.AddressingVersion.WSAddressing10 : (AddressingVersion)null;
        }

        public static MessageVersion MessageVersion(ProtocolVersion protocolVersion)
        {
            //ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), nameof(MessageVersion));
            if (protocolVersion == ProtocolVersion.Version10)
                return System.ServiceModel.Channels.MessageVersion.Soap11WSAddressingAugust2004;
            return protocolVersion == ProtocolVersion.Version11 ? System.ServiceModel.Channels.MessageVersion.Soap11WSAddressing10 : (MessageVersion)null;
        }

        public static MessageSecurityVersion SecurityVersion(
            ProtocolVersion protocolVersion)
        {
            // ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), nameof(SecurityVersion));
            if (protocolVersion == ProtocolVersion.Version10)
                return MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            return protocolVersion == ProtocolVersion.Version11 ? MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12 : (MessageSecurityVersion)null;
        }

        public static void SetReplyAddress(
            Message message,
            EndpointAddress replyTo,
            ProtocolVersion protocolVersion)
        {
            // ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), nameof(SetReplyAddress));
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    if (message.Headers.ReplyTo == (EndpointAddress)null)
                        message.Headers.ReplyTo = replyTo;
                    if (!(message.Headers.MessageId == (UniqueId)null))
                        break;
                    message.Headers.MessageId = new UniqueId();
                    break;
                case ProtocolVersion.Version11:
                    if (!(message.Headers.From == (EndpointAddress)null))
                        break;
                    message.Headers.From = replyTo;
                    break;
            }
        }
    }
}