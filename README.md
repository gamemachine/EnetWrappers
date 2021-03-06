# EnetWrappers

Simple wrappers over https://github.com/nxrighthere/ENet-CSharp.

Adds a bit more abstraction to keep things that need to be customized for client and server separate from the core.
This uses System.Buffers, so if you are on Unity you will need to add that.

Connection management uses a flow where the server sets a client as authenticated (ConnectionManager.SetAuthenticated).  Say via
an out of band https request.  Client connect sends an auth token, and then on connect that's authenticated against the clientid/authtoken pair
set previously by the server.

There is a separate client id in addition to the peer id, the connection manager allows for looking up the connection by either.

IEnetEventHandler for event handling.
IEnetLogger for logging.

EnetChannel is 3 types that are used by EnetMessageSender.  So channel usage is opinionated to some extent.  

EnetMessageSender has built in handling for the following

- Raw messages are byte[] or byte*.

- Values types send any unmanaged type using unsafe pointers as is.  For say IPC where you are optimizing for time rather then space.

- Protocol buffer support.  Abstractions that reuse the same stream and prepend a message header.


EnetRawMessage/EnetValueMessage<T> are for when you want to create messages in a different flow, say in a bursted job.  And then later
 submit them to enet to send.
 
 EnetRawMessage you will likely want to modify to use a different allocator. This came from a game using RpMalloc, I subsituted that for Marshal.AllocHGlobal.
 Unity's NativeArray<byte> works well here also.
 
 ExampleProtoStream shows how to implement IEnetProtoStream.



 
