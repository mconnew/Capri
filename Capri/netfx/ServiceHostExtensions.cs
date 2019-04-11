using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Capri
{
    public delegate Task<Message> DispatchMessageInspectorAsync(Message request, IClientChannel channel, InstanceContext instanceContext, MessageInspectorContinuation next);
    public static class ServiceHostExtensions
    {
        public static ServiceHost UseMessageInspector(this ServiceHost serviceHost, DispatchMessageInspectorAsync dispatchMessageInspector)
        {
            DispatchMessageInspectorBehavior behavior;
            if (serviceHost.Description.Behaviors.Contains(typeof(DispatchMessageInspectorBehavior)))
            {
                behavior = serviceHost.Description.Behaviors[typeof(DispatchMessageInspectorBehavior)] as DispatchMessageInspectorBehavior;
            }
            else
            {
                behavior = new DispatchMessageInspectorBehavior();
                serviceHost.Description.Behaviors.Add(behavior);
            }

            behavior.AddMessageInspector(dispatchMessageInspector);
            return serviceHost;
        }
    }
}
