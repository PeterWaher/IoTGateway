using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.P2P;

namespace Waher.Networking.XMPP.Test.E2eTests
{
    public abstract class E2eTests : CommunicationTests
    {
        protected IE2eEndpoint[] endpoints1;
        protected IE2eEndpoint[] endpoints2;
        protected EndpointSecurity endpointSecurity1;
        protected EndpointSecurity endpointSecurity2;

        public override void PrepareClient1(XmppClient Client)
        {
            base.PrepareClient1(Client);
            this.endpointSecurity1 = new EndpointSecurity(this.client1, 128, this.endpoints1);
        }

        public override void PrepareClient2(XmppClient Client)
        {
            base.PrepareClient2(Client);
            this.endpointSecurity2 = new EndpointSecurity(this.client2, 128, this.endpoints2);
        }

        public override void ConnectClients()
        {
            base.ConnectClients();

            this.SubscribedTo(this.client1, this.client2);
            this.SubscribedTo(this.client2, this.client1);
        }

        private void SubscribedTo(XmppClient From, XmppClient To)
        {
            RosterItem Item1 = From.GetRosterItem(To.BareJID);
            RosterItem Item2 = To.GetRosterItem(From.BareJID);
            if (Item1 is null || (Item1.State != SubscriptionState.Both && Item1.State != SubscriptionState.To) ||
                Item2 is null || (Item2.State != SubscriptionState.Both && Item2.State != SubscriptionState.From))
            {
                ManualResetEvent Done2 = new ManualResetEvent(false);
                ManualResetEvent Error2 = new ManualResetEvent(false);

                To.OnPresenceSubscribe += (sender, e) =>
                {
                    if (e.FromBareJID == From.BareJID)
                    {
                        e.Accept();
                        Done2.Set();
                    }
                    else
                    {
                        e.Decline();
                        Error2.Set();
                    }

                    return Task.CompletedTask;
                };

                From.RequestPresenceSubscription(To.BareJID);

                Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000));
            }
        }

        public abstract IE2eEndpoint GenerateEndpoint(IE2eSymmetricCipher Cipher);
    }
}
