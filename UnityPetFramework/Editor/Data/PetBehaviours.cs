namespace FynnsTools.UnityPetFramework
{
	// What the pet does, mapped onto whatever this pet calls the animation for it.
	internal static class PetBehaviours
	{
		public static readonly string[] Names = { "Idle", "Alternate idle", "Move", "Jump", "Pickup", "Click", "Petting", "Startled", "Confused", "Compile", "Success", "Failure", "Annoyed", "Sleep", "Rest", "Low energy", "Eat", "Playful", "Joke", "Special", "Turn", "Turn sitting" };
		private static readonly string[] Defaults = { "Stand 1", "Stand 2", "Move", "Jump", "Hang", "Chat", "Love", "Stunned", "What", "What", "Love", "Cry", "Angry", "Sleep", "Sit", "Hungry", "Eat", "Merong", "Dung", "Transform", PetClipSlots.TurnStanding, PetClipSlots.TurnSitting };
		public static string DefaultAnimation(PetDefinition pet, string behaviour)
		{
			for (int i = 0; i < Names.Length; i++) if (Names[i] == behaviour) return Defaults[i];
			return behaviour;
		}
	}
}
