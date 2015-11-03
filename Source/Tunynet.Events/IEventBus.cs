using System;
using System.Collections.Generic;
namespace Tunynet.Events
{
	public interface IEventBus<S, A>
	{
		event CommonEventHandler<S, A> Before;
		event CommonEventHandler<S, A> After;
		event EventHandlerWithHistory<S, A> BeforeWithHistory;
		event EventHandlerWithHistory<S, A> AfterWithHistory;
		event BatchEventHandler<S, A> BatchBefore;
		event BatchEventHandler<S, A> BatchAfter;
		void OnBefore(S sender, A eventArgs);
		void OnAfter(S sender, A eventArgs);
		void OnBeforeWithHistory(S sender, A eventArgs, S historyData);
		void OnAfterWithHistory(S sender, A eventArgs, S historyData);
		void OnBatchBefore(System.Collections.Generic.IEnumerable<S> senders, A eventArgs);
		void OnBatchAfter(System.Collections.Generic.IEnumerable<S> senders, A eventArgs);
	}
}
