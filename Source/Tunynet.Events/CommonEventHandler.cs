using System;
namespace Tunynet.Events
{
	public delegate void CommonEventHandler<S, A>(S sender, A eventArgs);
}
