using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PistonMod
{
	public class PistonSettings : ModConfig
	{
		public static PistonSettings Entity;
		
		[Label("Pushable pistons")]
		[Tooltip("Set to true so extended pistons can be pushed, but can be destroyed if all its parts are not moved at the same time")]
		[DefaultValue(false)]
		public bool PistonPushable;

		public override ConfigScope Mode => ConfigScope.ServerSide;
	}
}