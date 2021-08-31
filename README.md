# Light Messager Library

Light messaging library for C# applications.

## Description

This library provides an easy and intuitive way of sending customizable messages between classes and threads.

## Usage

### Create your own message types by derivating the abstract MessageType class.

```c#
class TestMessage :MessageType
{
  public string Msg;
  public TestMessage(object sender, string msg) : base(sender) { Msg = msg; }
}

class EmptyMessage :MessageType
{
  public EmptyMessage(object sender) : base(sender) { }
}
```

### Subscribe to messages

```c#
Messager.Default.Subscribe((TestMessage t) =>
{
  Console.WriteLine($"Received a TestMessage : {t.Msg}");
});
Messager.Default.Subscribe((EmptyMessage t) =>
{
  Console.WriteLine("Received an EmptyMessage");
});
```

### Send messages to subscribers

```c#
Messager.Default.Publish(new TestMessage(this, "a message"));
Messager.Default.Publish(new EmptyMessage(this));
```

### Subscribe to all message types

```c#
Messager.Default.Subscribe((MessageType m) =>
{
  if (m is TestMessage testMsg)
    Console.WriteLine($"Received a TestMessage : {testMsg.Msg}");
  else if (m is EmptyMessage)
    Console.WriteLine("Received an EmptyMessage");
  else
    Console.WriteLine("Received an unknown message type");
});
```

### Create multiple messagers

```c#
Messager default = new Messager();
Messager messager1 = new Messager("App messager");
Messager messager2 = new Messager("Log messager");

messager1.Subscribe(() => { /* ... */ });
```

### Unsubscribe when you don't need it anymore

```c#
void Handle(TestMessage t)
{
  // Do something
}

Messager.Default.Subscribe<TestMessage>(Handle);
// ...
Messager.Default.Unsubscribe<TestMessage>(Handle);
```
