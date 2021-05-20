using PistonMod.Piston;
using Terraria.ModLoader;

namespace PistonMod
{
	public class PistonedWorld : ModWorld
	{
		public override void PostUpdate()
		{
			PistonMechanics.VerifyPistonIntegrity();
			PistonMechanics.EffectivelyPushEntities();
		}
	}
}