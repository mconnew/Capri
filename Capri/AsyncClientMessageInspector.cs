using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Capri
{
    internal class AsyncClientMessageInspector : IClientMessageInspector
    {
        private ClientMessageInspectorAsync _clientMessageInspector;

        public AsyncClientMessageInspector(ClientMessageInspectorAsync clientMessageInspector)
        {
            _clientMessageInspector = clientMessageInspector;
        }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var continuation = new DispatcherContinuation();
            Task<Message> task = _clientMessageInspector(request, channel, continuation.Next);
            request = continuation.WaitForCall(task);
            return continuation;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var continuation = correlationState as DispatcherContinuation;
            reply = continuation.ContinueInspector(reply);
        }
    }
}