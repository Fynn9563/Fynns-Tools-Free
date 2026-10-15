using System;

namespace FynnsTools.UnityPetFramework
{
	internal sealed class PetDeferredActions
	{
		private Action _pending;

		public void Defer(Action action)
		{
			_pending = action;
		}

		public bool Run()
		{
			if (_pending == null)
			{
				return false;
			}

			Action action = _pending;

			_pending = null;
			action();
			return true;
		}
	}
}
