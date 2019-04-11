using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Capri
{
    internal class DispatchMessageInspectorBehavior : IServiceBehavior
    {
        private List<DispatchMessageInspectorAsync> _messageInspectors = new List<DispatchMessageInspectorAsync>();

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher eDispatcher in cDispatcher.Endpoints)
                {
                    foreach (var mi in _messageInspectors)
                    {
                        eDispatcher.DispatchRuntime.MessageInspectors.Add(new AsyncDispatchMessageInspector(mi));
                    }
                }
            }

        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

        internal void AddMessageInspector(DispatchMessageInspectorAsync dispatchMessageInspector)
        {
            _messageInspectors.Add(dispatchMessageInspector);
        }
    }
}
