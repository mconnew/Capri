using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Capri.Tests
{
    internal class MockChannelFactory : ChannelFactoryBase<IRequestChannel>
    {
        private BindingContext _context;
        private MessageVersion _messageVersion;

        public MockChannelFactory(BindingContext context, Func<Message, Message> requestDelegate)
        {
            _context = context;
            RequestDelegate = requestDelegate;
            Collection<MessageEncodingBindingElement> messageEncoderBindingElements
                = ((KeyedByTypeCollection<object>)(object)context.BindingParameters).FindAll<MessageEncodingBindingElement>();

            if (messageEncoderBindingElements.Count > 1)
            {
                throw new InvalidOperationException("Multiple MessageEncodingBindingElement instances in BindingParameters");
            }
            else if (messageEncoderBindingElements.Count == 1)
            {
                MessageEncoderFactory = messageEncoderBindingElements[0].CreateMessageEncoderFactory();
                ((KeyedByTypeCollection<object>)(object)context.BindingParameters).Remove<MessageEncodingBindingElement>();
            }
            else
            {
                throw new InvalidOperationException("Missing MessageEncodingBindingElement in BindingParameters");
            }

            if (null != MessageEncoderFactory)
                _messageVersion = MessageEncoderFactory.MessageVersion;
            else
                _messageVersion = MessageVersion.None;
        }

        public Func<Message, Message> RequestDelegate { get; }

        public MessageEncoderFactory MessageEncoderFactory { get; }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => Task.CompletedTask;
        protected override void OnEndOpen(IAsyncResult result) { }

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new MockRequestChannel(this, address, via);
        }

        protected override void OnOpen(TimeSpan timeout) { }
    }
}