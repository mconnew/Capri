using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Capri.Tests
{
    [ServiceBehavior(UseSynchronizationContext = false)]
    class MyService : IMyService
    {
        public string Echo(string echo)
        {
            if (!OperationContext.Current.IncomingMessageProperties.TryGetValue("foo", out object value))
            {
                throw new Exception("foo message property missing");
            }

            OperationContext.Current.OutgoingMessageProperties.Add("bar", new object());
            return echo;
        }

        public void EchoFooHeader()
        {
            var fooHeaderValue = GetHttpRequestHeader("foo");
            AddHttpResponseHeader("foo", fooHeaderValue);
        }

        internal static void AddHttpResponseHeader(string name, string value)
        {
            HttpResponseMessageProperty httpResponseMessageProperty;
            if (!OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                httpResponseMessageProperty = new HttpResponseMessageProperty();
                OperationContext.Current.OutgoingMessageProperties.Add(HttpResponseMessageProperty.Name, httpResponseMessageProperty);
            }
            else
            {
                httpResponseMessageProperty = OperationContext.Current.OutgoingMessageProperties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            }

            httpResponseMessageProperty.Headers[name] = value;
        }

        internal static string GetHttpRequestHeader(string name)
        {
            if (OperationContext.Current.IncomingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                var httpRequestMessageProperty = OperationContext.Current.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                return httpRequestMessageProperty.Headers.Get(name);
            }

            return null;
        }
    }
}
