# Capri
A lighter shade of Indigo - collection of utility classes to make using and extending WCF more light-weight.  

WCF has incredibly powerful extensibility capabilities, but they can be a little challenging to use sometimes. This project is a collection of utility classes to make extending WCF easier.  

This set of classes will work on .NET Framework 4.6 or later and netstandard2.0 or later. The netstandard version will only have the helpers for WCF Client based extensibility. The nuget package can be found [here](https://www.nuget.org/packages/Capri/). 

## Capri.ChannelFactoryExtensions
This is a collection of extension methods on `ChannelFactory<TChannel>` to make implementing various extensibility scenarios easier.

`ClientMessageInspectorAsync`  
This is a named delegate type defining the prototype of an asynchronous client message inspector delegate. It is defined as:
```c#
public delegate Task<Message> ClientMessageInspectorAsync(Message request, IClientChannel channel, MessageInspectorContinuation next);
```
When called, the outgoing request message is passed as the first parameter. The `IClientChannel` instance that is being used to send the message is passed as the second parameter. The third parameter is a delegate you call to tell WCF to continue sending the message. This delegate is defined as:
```c#
public delegate Task<Message> MessageInspectorContinuation(Message message);
```
The message parameter is the request message that is to be sent. If the message inspector did not replace the request message, simply pass in the request message that was originally passed into the async client message inspector. If the message is to be replaced, pass the replacement message instance and that will be used as the request message. The return value is a `Task<Message>` and should be awaited. This `Task` completes with the reponse message returned to the client. At this point you can examine and/or modify the response message in the same way you would in the `IClientMessageInspector.AfterReceiveReply` method. If the response message has not been replaced, simply return the message returned from the `MessageInspectorContinuation` delegate. If a new response message instance has been created to replace the original response message, then return that.

`ChannelFactoryExtensions.UseMessageInspector`  
This method enables usage of an async client message inspector delegate. You provide a `ClientMessageInspectorAsync` delegate. Example:
```csharp
channelFactory.UseMessageInspector(async (request, channel, next) =>
    {
        // Equivalent to IClientMessageInspector.BeforeSendRequest
        Logger.LogRequest(message);
        var response = await next(request);
        // Equivalent to IClientMessageInspector.AfterReceiveReply
        Logger.LogResponse(message);
        return response;
    });
```
While you can `await` other method calls before and after the `await` of the `next` function, this will cause the underlying message inspector to block the thread waiting for you to call `next`. The underlying implementation of `IClientMessageInspector` is still synchronous and is unable to yield the thread until the request message has been passed to `next`.