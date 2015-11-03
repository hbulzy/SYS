using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tunynet.Logging;
namespace Tunynet.Events
{
	public class EventBus<S, T> : IEventBus<S, T> where T : CommonEventArgs
	{
		private static volatile EventBus<S, T> _instance = null;
		private static readonly object lockObject = new object();
		private static EventHandlerList Handlers = new EventHandlerList();
		private object EventHandlerKey_Before = new object();
		private object EventHandlerKey_After = new object();
		private object EventHandlerKey_BeforeWithHistory = new object();
		private object EventHandlerKey_AfterWithHistory = new object();
		private object EventHandlerKey_BatchBefore = new object();
		private object EventHandlerKey_BatchAfter = new object();
		public event CommonEventHandler<S, T> Before
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_Before, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_Before, value);
			}
		}
		public event CommonEventHandler<S, T> After
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_After, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_After, value);
			}
		}
		public event EventHandlerWithHistory<S, T> BeforeWithHistory
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_BeforeWithHistory, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_BeforeWithHistory, value);
			}
		}
		public event EventHandlerWithHistory<S, T> AfterWithHistory
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_AfterWithHistory, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_AfterWithHistory, value);
			}
		}
		public event BatchEventHandler<S, T> BatchBefore
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_BatchBefore, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_BatchBefore, value);
			}
		}
		public event BatchEventHandler<S, T> BatchAfter
		{
			add
			{
				EventBus<S, T>.Handlers.AddHandler(this.EventHandlerKey_BatchAfter, value);
			}
			remove
			{
				EventBus<S, T>.Handlers.RemoveHandler(this.EventHandlerKey_BatchAfter, value);
			}
		}
		public static EventBus<S, T> Instance()
		{
			if (EventBus<S, T>._instance == null)
			{
				lock (EventBus<S, T>.lockObject)
				{
					if (EventBus<S, T>._instance == null)
					{
						EventBus<S, T>._instance = new EventBus<S, T>();
					}
				}
			}
			return EventBus<S, T>._instance;
		}
		protected EventBus()
		{
		}
		public void OnBefore(S sender, T eventArgs)
		{
			CommonEventHandler<S, T> commonEventHandler = EventBus<S, T>.Handlers[this.EventHandlerKey_Before] as CommonEventHandler<S, T>;
			if (commonEventHandler != null)
			{
				System.Delegate[] invocationList = commonEventHandler.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					CommonEventHandler<S, T> commonEventHandler2 = (CommonEventHandler<S, T>)array[i];
					try
					{
						commonEventHandler2.BeginInvoke(sender, eventArgs, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发操作执行前事件时发生异常");
					}
				}
			}
		}
		public void OnAfter(S sender, T eventArgs)
		{
			CommonEventHandler<S, T> commonEventHandler = EventBus<S, T>.Handlers[this.EventHandlerKey_After] as CommonEventHandler<S, T>;
			if (commonEventHandler != null)
			{
				System.Delegate[] invocationList = commonEventHandler.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					CommonEventHandler<S, T> commonEventHandler2 = (CommonEventHandler<S, T>)array[i];
					try
					{
						commonEventHandler2.BeginInvoke(sender, eventArgs, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发操作执行后事件时发生异常");
					}
				}
			}
		}
		public void OnBeforeWithHistory(S sender, T eventArgs, S historyData)
		{
			EventHandlerWithHistory<S, T> eventHandlerWithHistory = EventBus<S, T>.Handlers[this.EventHandlerKey_BeforeWithHistory] as EventHandlerWithHistory<S, T>;
			if (eventHandlerWithHistory != null)
			{
				System.Delegate[] invocationList = eventHandlerWithHistory.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					EventHandlerWithHistory<S, T> eventHandlerWithHistory2 = (EventHandlerWithHistory<S, T>)array[i];
					try
					{
						eventHandlerWithHistory2.BeginInvoke(sender, eventArgs, historyData, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发含历史数据操作执行前事件时发生异常");
					}
				}
			}
		}
		public void OnAfterWithHistory(S sender, T eventArgs, S historyData)
		{
			EventHandlerWithHistory<S, T> eventHandlerWithHistory = EventBus<S, T>.Handlers[this.EventHandlerKey_AfterWithHistory] as EventHandlerWithHistory<S, T>;
			if (eventHandlerWithHistory != null)
			{
				System.Delegate[] invocationList = eventHandlerWithHistory.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					EventHandlerWithHistory<S, T> eventHandlerWithHistory2 = (EventHandlerWithHistory<S, T>)array[i];
					try
					{
						eventHandlerWithHistory2.BeginInvoke(sender, eventArgs, historyData, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发含历史数据操作执行后事件时发生异常");
					}
				}
			}
		}
		public void OnBatchBefore(System.Collections.Generic.IEnumerable<S> senders, T eventArgs)
		{
			BatchEventHandler<S, T> batchEventHandler = EventBus<S, T>.Handlers[this.EventHandlerKey_BatchBefore] as BatchEventHandler<S, T>;
			if (batchEventHandler != null)
			{
				System.Delegate[] invocationList = batchEventHandler.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					BatchEventHandler<S, T> batchEventHandler2 = (BatchEventHandler<S, T>)array[i];
					try
					{
						batchEventHandler2.BeginInvoke(senders, eventArgs, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发批量操作执行前事件时发生异常");
					}
				}
			}
		}
		public void OnBatchAfter(System.Collections.Generic.IEnumerable<S> senders, T eventArgs)
		{
			BatchEventHandler<S, T> batchEventHandler = EventBus<S, T>.Handlers[this.EventHandlerKey_BatchAfter] as BatchEventHandler<S, T>;
			if (batchEventHandler != null)
			{
				System.Delegate[] invocationList = batchEventHandler.GetInvocationList();
				System.Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					BatchEventHandler<S, T> batchEventHandler2 = (BatchEventHandler<S, T>)array[i];
					try
					{
						batchEventHandler2.BeginInvoke(senders, eventArgs, null, null);
					}
					catch (System.Exception exception)
					{
						LoggerFactory.GetLogger().Log(LogLevel.Error, exception, "执行触发批量操作执行后事件时发生异常");
					}
				}
			}
		}
	}
	public class EventBus<S> : EventBus<S, CommonEventArgs>
	{
		private static volatile EventBus<S> _instance = null;
		private static readonly object lockObject = new object();
		public new static EventBus<S> Instance()
		{
			if (EventBus<S>._instance == null)
			{
				lock (EventBus<S>.lockObject)
				{
					if (EventBus<S>._instance == null)
					{
						EventBus<S>._instance = new EventBus<S>();
					}
				}
			}
			return EventBus<S>._instance;
		}
	}
}
