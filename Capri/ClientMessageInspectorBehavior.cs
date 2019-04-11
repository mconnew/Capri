using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Capri
{
    internal class ClientMessageInspectorBehavior : IEndpointBehavior
    {
        private List<ClientMessageInspectorAsync> _messageInspectors = new List<ClientMessageInspectorAsync>();

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach (var mi in _messageInspectors)
            {
                clientRuntime.ClientMessageInspectors.Add(new AsyncClientMessageInspector(mi));
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }

        internal void AddMessageInspector(ClientMessageInspectorAsync clientMessageInspector)
        {
            _messageInspectors.Add(clientMessageInspector);
        }
    }
}
