using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Text;

namespace Capri.Tests
{
    class MockTransportBindingElement : TransportBindingElement
    {
        public MockTransportBindingElement(string scheme)
        {
            Scheme = scheme;
        }

        public MockTransportBindingElement(TransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            Scheme = elementToBeCloned.Scheme;
            RequestDelegate = ((MockTransportBindingElement)elementToBeCloned).RequestDelegate;
        }

        public override string Scheme { get; }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IRequestChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return (IChannelFactory<TChannel>)new MockChannelFactory(context, RequestDelegate);
        }

        public override BindingElement Clone()
        {
            return new MockTransportBindingElement(this);
        }

        public Func<Message, Message> RequestDelegate { get; set; } = (request) =>
        {
            Debugger.Break();
            return null;
        };
    }
}
