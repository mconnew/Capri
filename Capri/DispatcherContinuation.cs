using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace Capri
{
    internal class DispatcherContinuation
    {
        private Message _request;
        private bool _requestSet = false;
        private ManualResetEventSlim _resetEvent;
        private Task<Message> _asyncInspectorTask;
        private TaskCompletionSource<Message> _nextTcs;
        private object _lock = new object();

        public DispatcherContinuation()
        {

        }

        internal Message WaitForCall(Task<Message> task)
        {
            lock (_lock)
            {
                _asyncInspectorTask = task;
                if (task.IsCompleted)
                {
                    // If an exception was thrown before calling Next, getting the result will throw that exception.
                    // If the task completed with a result, it's a message inspector with only an implementation for BeforeSendRequest as it's not going to call Next()
                    // In which case we want to return the Message result to replace the ref parameter.
                    RequestMessage = task.GetAwaiter().GetResult();
                }

                if (!_requestSet)
                {
                    _resetEvent = new ManualResetEventSlim(_requestSet);
                    task.ContinueWith((antecedant) => _resetEvent.Set(), TaskContinuationOptions.OnlyOnFaulted);
                }
            }

            _resetEvent?.Wait();
            // _resetEvent may have been set via fault continuation so need to throw if that's the case
            if (task.IsFaulted)
            {
                task.GetAwaiter().GetResult();
            }

            return RequestMessage;
        }

        internal Message ContinueInspector(Message reply)
        {
            if (_asyncInspectorTask.IsCompleted)
            {
                // This is the scenario where the message inspector only implements the BeforeSendRequest part. There is no code to modify the reply message.
                return reply;
            }

            _nextTcs.SetResult(reply);
            // Unless there's an await after the call to Next(), returning from SetResult should complete when _asyncInspectorTask completes so this should
            // simply return the final reply Message. If it isn't complete, then this will block until it is.
            return _asyncInspectorTask.GetAwaiter().GetResult();
        }

        internal Task<Message> Next(Message message)
        {
            lock (_lock)
            {
                RequestMessage = message;
            }

            _nextTcs = new TaskCompletionSource<Message>();
            return _nextTcs.Task;
        }

        internal Message RequestMessage
        {
            get
            {
                return _request;
            }
            set
            {
                _request = value;
                _requestSet = true;
                _resetEvent?.Set();
            }
        }
    }
}
