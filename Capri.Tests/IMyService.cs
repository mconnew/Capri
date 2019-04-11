using System.ServiceModel;

namespace Capri.Tests
{
    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        string Echo(string echo);

        [OperationContract]
        void EchoFooHeader();
    }
}
