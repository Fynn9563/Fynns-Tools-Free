namespace FynnsTools.UnityPetFramework
{
	// Recognised animation names.
	internal static class PetClipSlots
	{
		internal sealed class Slot
		{
			public Slot(string name, string label, string description, bool required, bool mirrorByDefault)
			{
				Name = name;
				Label = label;
				Description = description;
				Required = required;
				MirrorByDefault = mirrorByDefault;
			}

			// Stored in pet.json and asked for by the runtime.
			public string Name { get; }

			public string Label { get; }

			public string Description { get; }

			// A pet without one of these cannot walk around, so it is not usable.
			public bool Required { get; }

			// What "Allow mirroring" starts as for a newly filled slot.
			public bool MirrorByDefault { get; }
		}

		public const string Idle = "Idle";
		public const string Walk = "Walk";
		public const string Jump = "Jump";
		public const string TurnStanding = "TurnStanding";
		public const string TurnSitting = "TurnSitting";
		public const string Thinking = "Thinking";
		public const string Sleeping = "Sleeping";
		public const string Alert = "Alert";
		public const string Happy = "Happy";
		public const string Celebrate = "Celebrate";
		public const string PetIcon = "Pet Icon";
		public const string Stand1 = "Stand 1", Stand2 = "Stand 2", Move = "Move", Hang = "Hang";
		public const string Chat = "Chat", Eat = "Eat", Playful = "Playful", Stunned = "Stunned", What = "What";
		public const string Dung = "Dung", Transform = "Transform", Angry = "Angry", Cry = "Cry";
		public const string Sleep = "Sleep", Sit = "Sit", Love = "Love", Hungry = "Hungry";

		public static readonly Slot[] All =
		{
			new Slot(Stand1, Stand1, "Primary idle.", false, true),
			new Slot(Stand2, Stand2, "Alternate idle.", false, true),
			new Slot(Move, Move, "Movement; mirrored for direction.", false, true),
			new Slot(Jump, Jump, "Jump pose; the framework moves the pet.", false, true),
			new Slot(Hang, Hang, "Optional pickup pose.", false, true),
			new Slot(Chat, Chat, "Social / click reaction.", false, true),
			new Slot(Eat, Eat, "Eating.", false, true),
			new Slot(Playful, Playful, "Tongue-out playful reaction.", false, true),
			new Slot(Stunned, Stunned, "Startled reaction.", false, true),
			new Slot(What, What, "Confused reaction.", false, true),
			new Slot(Dung, Dung, "Rare joke behaviour.", false, true),
			new Slot(Transform, Transform, "Special transformation.", false, true),
			new Slot(Angry, Angry, "Repeated annoying interaction.", false, true),
			new Slot(Cry, Cry, "Strong negative reaction.", false, true),
			new Slot(Sleep, Sleep, "Sleeping.", false, true),
			new Slot(Sit, Sit, "Resting.", false, true),
			new Slot(Love, Love, "Successful petting.", false, true),
			new Slot(Hungry, Hungry, "Low-energy warning.", false, true),

			// Optional, and not part of the Kino set.
			new Slot(TurnStanding, "Turn Standing", "Optional standing turn. Played backwards for the other direction.", false, false),
			new Slot(TurnSitting, "Turn Sitting", "Optional sitting turn, used when the pet rolls over in its sleep.", false, false)
		};

		// The animated pet icon.
		public static readonly Slot Icon = new Slot(
			PetIcon,
			"Pet Icon",
			"The pet's picture in the list, picked from its own sheet like any other animation. One frame for a still thumbnail, several for an animated one.",
			false,
			false);

		public static Slot Find(string name)
		{
			if (name == PetIcon) return Icon;

			foreach (Slot slot in All)
			{
				if (slot.Name == name)
				{
					return slot;
				}
			}

			return null;
		}

		public static bool IsKnown(string name)
		{
			return Find(name) != null;
		}
	}
}
