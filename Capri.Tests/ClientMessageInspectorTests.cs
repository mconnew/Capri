using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Xunit;

namespace Capri.Tests
{
    public class ClientMessageInspectorTests
    {
        [Fact]
        public void BasicHttpService()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector(async (request, channel, next) =>
            {
                request.Properties.Add("foo", new object());
                var response = await next(request);
                if (!response.Properties.TryGetValue("bar", out object value))
                {
                    throw new Exception("bar message property missing");
                }
                return response;
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Equal("hello", client.Echo("hello"));
        }


        [Fact]
        public void MultipleAwaitsInspector()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector(async (request, channel, next) =>
            {
                await Task.Yield();
                request.Properties.Add("foo", new object());
                await Task.Yield();
                var response = await next(request);
                await Task.Yield();
                if (!response.Properties.TryGetValue("bar", out object value))
                {
                    throw new Exception("bar message property missing");
                }
                await Task.Yield();
                return response;
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Equal("hello", client.Echo("hello"));
        }

        [Fact]
        public void ExceptionThrownBeforeNextNoAwait()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector((request, channel, next) =>
            {
                throw new Exception();
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Throws<Exception>(() => client.Echo("hello"));
        }

        [Fact]
        public void ExceptionThrownBeforeNextWithAwait()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector(async (request, channel, next) =>
            {
                await Task.Yield();
                throw new Exception();
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Throws<Exception>(() => client.Echo("hello"));
        }

        [Fact]
        public void ExceptionThrownAfterNextNoAwait()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector(async (request, channel, next) =>
            {
                var response = await next(request);
                throw new Exception();
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Throws<Exception>(() => client.Echo("hello"));
        }

        [Fact]
        public void ExceptionThrownAfterNextWithAwait()
        {
            var binding = new CustomBinding(new TextMessageEncodingBindingElement { MessageVersion = MessageVersion.Soap12WSAddressing10 },
                                            new MockTransportBindingElement("http") { RequestDelegate = TestHelper.HandleRequest });
            var factory = new ChannelFactory<IMyService>(binding, new EndpointAddress("http://localhost:12345/basicHttp"));
            factory.UseMessageInspector(async (request, channel, next) =>
            {
                var response = await next(request);
                await Task.Yield();
                throw new Exception();
            });
            factory.Open();
            var client = factory.CreateChannel();
            Assert.Throws<Exception>(() => client.Echo("hello"));
        }
    }

    internal class TestHelper
    {
        internal static Message HandleRequest(Message request)
        {
            if (!request.Properties.TryGetValue("foo", out object value))
            {
                throw new Exception("foo message property missing");
            }

            Message response = Message.CreateMessage(request.Version, "http://tempuri.org/IMyService/EchoResponse", new EchoResponse { Echo = "hello" });
            response.Properties.Add("bar", new object());
            return response;
        }

        [DataContract(Name = "EchoResponse", Namespace = "http://tempuri.org/")]
        private class EchoResponse
        {
            [DataMember(Name = "EchoResult")]
            public string Echo { get; set; }
        }
    }
}
