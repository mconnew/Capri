using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Capri
{
    internal class AsyncDispatchMessageInspector : IDispatchMessageInspector
    {
        private DispatchMessageInspectorAsync _dispatchMessageInspector;

        public AsyncDispatchMessageInspector(DispatchMessageInspectorAsync dispatchMessageInspector)
        {
            _dispatchMessageInspector = dispatchMessageInspector;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var continuation = new DispatcherContinuation();
            Task<Message> task = _dispatchMessageInspector(request, channel, instanceContext, continuation.Next);
            request = continuation.WaitForCall(task);
            return continuation;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var continuation = (DispatcherContinuation)correlationState;
            reply = continuation.ContinueInspector(reply);
        }
    }
}