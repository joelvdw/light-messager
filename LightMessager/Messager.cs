/*
Copyright 2021, Joel VON DER WEID

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightMessager
{
  /// <summary>
  /// Messager class managing MessageType subscription and message publishing
  /// 
  /// The user should create a MessageType subclass for each message type he want to use
  /// He can next subscribe to this specific MessageType and publish messages of this type.
  /// 
  /// Different Messager can be created to separate usages
  /// or the Default static Messager can be used to simplify the usage and the access of the Messager from everywhere
  /// </summary>
  public class Messager
  {
    private class TypedAction
    {
      public Type Type { get; }
      public Delegate Action;

      private TypedAction(Type t, Delegate action)
      {
        Type = t;
        Action = action;
      }

      public override bool Equals(object obj)
      {
        if (obj == null || !(obj is TypedAction))
          return false;

        TypedAction other = (TypedAction)obj;
        return (Type == other.Type && Action == other.Action);
      }

      public override int GetHashCode() => Action.GetHashCode();

      public static TypedAction Create<T>(Type t, MessageAction<T> action) where T : MessageType
      {
        if (GetTypeFromAction(action) != t)
          throw new ArgumentException("Given type doesn't match the MessageAction type");

        return new TypedAction(t, action);
      }
    }

    /// <summary>
    /// Delegate used to pass an action for a specific MessageType
    /// </summary>
    /// <typeparam name="T">MessageType</typeparam>
    /// <param name="message">Message to send</param>
    public delegate void MessageAction<T>(T message) where T : MessageType;

    /// <summary>
    /// Indicate if each sent message should be logged in the console
    /// </summary>
    public static bool LogEnabled = true;

    /// <summary>
    /// Default Message created on first access
    /// </summary>
    public static Messager Default {
      get
      {
        if (_default == null)
          _default = new Messager("default");
        return _default;
      }
    }
    private static Messager _default = null;

    /// <summary>
    /// Messager name, can be used to îdentify multiple Messagers
    /// </summary>
    public string Name { get; }

    private BlockingList<TypedAction> subscribedActions;

    /// <summary>
    /// Create a new independant Messager
    /// </summary>
    /// <param name="name">Messager name</param>
    public Messager(string name)
    {
      Name = name;
      subscribedActions = new BlockingList<TypedAction>();
    }
    /// <summary>
    /// Create a new independant Messager, with an empty name
    /// </summary>
    public Messager(): this("") { }

    /// <summary>
    /// Subscribe to a specific message type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public void Subscribe<T>(MessageAction<T> action) where T : MessageType
    {
      Type t = GetTypeFromAction(action);
      subscribedActions.Add(TypedAction.Create(t, action));
    }

    /// <summary>
    /// Unsubscribe a previously subscribed action from a MessageType
    /// </summary>
    /// <typeparam name="T">MessageType to subscribe to</typeparam>
    /// <param name="action">Action to unsubscribe</param>
    public void Unsubscribe<T>(MessageAction<T> action) where T : MessageType
    {
      Type t = GetTypeFromAction(action);
      subscribedActions.Remove(TypedAction.Create(t, action));
    }

    /// <summary>
    /// Publish a message
    /// 
    /// All subscribed actions for this type or any super type will received this message
    /// </summary>
    /// <typeparam name="T">MessageType of the message</typeparam>
    /// <param name="message">Message to publish</param>
    public void Publish<T>(T message) where T : MessageType
    {
      List<TypedAction> actions = subscribedActions.Where(a => a.Type.IsAssignableFrom(message.GetType())).ToList();

      if (LogEnabled)
        Console.WriteLine($"Messager \"{Name}\" : {message.GetType()} message sent by {message.Sender} to {actions.Count()} subscribers");

      foreach (TypedAction a in actions)
          a.Action.DynamicInvoke(message);
    }

    /// <summary>
    /// Get the MessageType from a given MessageAction
    /// </summary>
    /// <typeparam name="T">Type of the MessageAction</typeparam>
    /// <param name="action">Action to analyze</param>
    /// <returns>MessageType of the Action</returns>
    private static Type GetTypeFromAction<T>(MessageAction<T> action) where T : MessageType
      => action.Method.GetParameters()[0].ParameterType;
  }
}
