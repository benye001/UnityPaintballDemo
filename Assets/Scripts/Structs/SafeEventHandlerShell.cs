using System;
using System.Linq;

public struct SafeEventHandlerShell<T>
{
	private EventHandler<T> containedEvent;

	public event EventHandler<T> ContainedEventProperty
	{
		add
		{
			if (containedEvent == null)
			{
				containedEvent += value;
			}
			else if (!containedEvent.GetInvocationList().Contains(value))
			{
				containedEvent += value;
			}
		}
		remove
		{
			containedEvent -= value;
		}
	}

	public void Invoke(object sender, T e)
	{
		containedEvent?.Invoke(sender, e);
	}
}
