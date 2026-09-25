using System;

namespace FynnsTools.UnityPetFramework
{
	// Holds an action a button asked for, to be run once the IMGUI pass has finished laying out.
	internal sealed class PetDeferredActions
	{
		private Action _pending;

		public void Defer(Action action)
		{
			_pending = action;
		}

		// True when something ran, which the caller uses to request a repaint.
		public bool Run()
		{
			if (_pending == null)
			{
				return false;
			}

			Action action = _pending;

			// Cleared before running.
			_pending = null;
			action();
			return true;
		}
	}
}
