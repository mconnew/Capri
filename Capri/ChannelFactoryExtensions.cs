using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Capri
{
    public delegate Task<Message> MessageInspectorContinuation(Message message);
    public delegate Task<Message> ClientMessageInspectorAsync(Message request, IClientChannel channel, MessageInspectorContinuation next);

    public static class ChannelFactoryExtensions
    {
        public static ChannelFactory<TChannel> UseMessageInspector<TChannel>(this ChannelFactory<TChannel> channelFactory, ClientMessageInspectorAsync clientMessageInspector)
        {
            ClientMessageInspectorBehavior behavior;
            if (channelFactory.Endpoint.EndpointBehaviors.Contains(typeof(ClientMessageInspectorBehavior)))
            {
                behavior = channelFactory.Endpoint.EndpointBehaviors[typeof(ClientMessageInspectorBehavior)] as ClientMessageInspectorBehavior;
            }
            else
            {
                behavior = new ClientMessageInspectorBehavior();
                channelFactory.Endpoint.EndpointBehaviors.Add(behavior);
            }

            behavior.AddMessageInspector(clientMessageInspector);
            return channelFactory;
        }
    }
}
