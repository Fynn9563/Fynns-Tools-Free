#if VRC_SDK_VRCSDK3
using UnityEngine;
using VRC.SDKBase;

namespace FynnsTools.GoGoLocoPoseChanger
{
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class GoGoLocoPoseChangerComponent : MonoBehaviour, IEditorOnly
	{
		public Motion avatarThumbnail;

		public Motion stand;

		public Motion crouch;

		public Motion prone;

		public Motion fall;

		public Motion afk;

		public Motion afkInit;

		public Motion afkLoop;

		public Motion afkStop;

		public bool isExtended;

		public bool debug;
	}
}
#endif
