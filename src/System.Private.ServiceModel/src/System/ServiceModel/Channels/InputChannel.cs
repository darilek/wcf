// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Threading.Tasks;

namespace System.ServiceModel.Channels
{
    internal class InputChannel
    {
        internal static async Task<Message> HelpReceiveAsync(IAsyncInputChannel channel, TimeSpan timeout)
        {
            (bool success, Message message) = await channel.TryReceiveAsync(timeout);

            if (success)
            {
                return message;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateReceiveTimedOutException(channel, timeout));
            }
        }

        private static Exception CreateReceiveTimedOutException(IInputChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(SR.Format(SR.ReceiveTimedOut, channel.LocalAddress.Uri.AbsoluteUri, timeout));
            }
            else
            {
                return new TimeoutException(SR.Format(SR.ReceiveTimedOutNoLocalAddress, timeout));
            }
        }

        internal static Message HelpReceive(IInputChannel channel, TimeSpan timeout)
        {
            Message message;
            if (channel.TryReceive(timeout, out message))
            {
                return message;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateReceiveTimedOutException(channel, timeout));
            }
        }

        internal static IAsyncResult HelpBeginReceive(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveAsyncResult(channel, timeout, callback, state);
        }

        internal static Message HelpEndReceive(IAsyncResult result)
        {
            return HelpReceiveAsyncResult.End(result);
        }

        class HelpReceiveAsyncResult : AsyncResult
        {
            IInputChannel channel;
            TimeSpan timeout;
            static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
            Message message;

            public HelpReceiveAsyncResult(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.timeout = timeout;
                IAsyncResult result = channel.BeginTryReceive(timeout, onReceive, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                HandleReceiveComplete(result);
                base.Complete(true);
            }

            public static Message End(IAsyncResult result)
            {
                HelpReceiveAsyncResult thisPtr = AsyncResult.End<HelpReceiveAsyncResult>(result);
                return thisPtr.message;
            }

            void HandleReceiveComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceive(result, out this.message))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        InputChannel.CreateReceiveTimedOutException(this.channel, this.timeout));
                }
            }

            static void OnReceive(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                HelpReceiveAsyncResult thisPtr = (HelpReceiveAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    thisPtr.HandleReceiveComplete(result);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }
        }
    }
}
