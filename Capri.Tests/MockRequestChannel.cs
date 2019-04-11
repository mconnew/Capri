using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Capri.Tests
{
    internal class MockRequestChannel : ChannelBase, IRequestChannel
    {
        public MockRequestChannel(MockChannelFactory factory, EndpointAddress to, Uri via)
                : base(factory)
        {
            Via = via;
            RemoteAddress = to;
        }

        public EndpointAddress RemoteAddress { get; }

        public Uri Via { get; private set; }

        public Message Request(Message message, TimeSpan timeout)
        {
            return ((MockChannelFactory)Manager).RequestDelegate(message);
        }

        public Message Request(Message message)
        {
            return Request(message, DefaultReceiveTimeout);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<Message>(state);
            tcs.SetResult(Request(message, DefaultReceiveTimeout));
            callback(tcs.Task);
            return tcs.Task;
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return BeginRequest(message, DefaultReceiveTimeout, callback, state);
        }


        public Message EndRequest(IAsyncResult result)
        {
            return ((Task<Message>)result).GetAwaiter().GetResult();
        }

        protected override void OnAbort() { }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state) => Task.CompletedTask;

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state) => Task.CompletedTask;

        protected override void OnClose(TimeSpan timeout) { }

        protected override void OnEndClose(IAsyncResult result) { }

        protected override void OnEndOpen(IAsyncResult result) { }

        protected override void OnOpen(TimeSpan timeout) { }
    }
}