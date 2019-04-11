using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Xunit;

namespace Capri.Tests
{
    public class DispatchMessageInspectorTests
    {
        [Fact]
        public void BasicHttpService()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector(async (request, channel, instanceContext, next) =>
                {
                    request.Properties.Add("foo", new object());
                    var response = await next(request);
                    if (!response.Properties.TryGetValue("bar", out object value))
                    {
                        throw new Exception("bar message property missing");
                    }

                    return response;
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                Assert.Equal("hello", client.Echo("hello"));
            }
        }

        [Fact]
        public void MultipleAwaitsInspector()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector(async (request, channel, instanceContext, next) =>
                {
                    await Task.Yield();
                    request.Properties.Add("foo", new object());
                    await Task.Yield();
                    var response = await next(request);
                    if (!response.Properties.TryGetValue("bar", out object value))
                    {
                        throw new Exception("bar message property missing");
                    }

                    await Task.Yield();
                    return response;
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                Assert.Equal("hello", client.Echo("hello"));
            }
        }

        [Fact]
        public void ExceptionThrownBeforeNextNoAwait()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector((request, channel, instanceContext, next) =>
                {
                    throw new Exception();
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                Assert.Throws<FaultException>(() => client.Echo("hello"));
            }
        }

        [Fact]
        public void ExceptionThrownBeforeNextWithAwait()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector(async (request, channel, instanceContext, next) =>
                {
                    await Task.Yield();
                    throw new Exception();
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                Assert.Throws<FaultException>(() => client.Echo("hello"));
            }
        }

        [Fact]
        public void ExceptionThrownAfterNextNoAwait()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector(async (request, channel, instanceContext, next) =>
                {
                    var response = await next(request);
                    throw new Exception();
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                ((IClientChannel)client).Open();
                Assert.Throws<FaultException>(() =>
                    {
                        client.Echo("hello");
                    });
                ((IClientChannel)client).Close();
            }
        }

        [Fact]
        public void ExceptionThrownAfterNextWithAwait()
        {
            using (var host = new ServiceHost(typeof(MyService), new Uri("http://locahost:12345")))
            {
                host.AddServiceEndpoint(typeof(IMyService), new BasicHttpBinding(), "/basicHttp");
                host.UseMessageInspector(async (request, channel, instanceContext, next) =>
                {
                    var response = await next(request);
                    await Task.Yield();
                    throw new Exception();
                });

                host.Open();
                var factory = new ChannelFactory<IMyService>(new BasicHttpBinding(), "http://localhost:12345/basicHttp");
                factory.Open();
                var client = factory.CreateChannel();
                Assert.Throws<FaultException>(() => client.Echo("hello"));
            }
        }
    }
}
