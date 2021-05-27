using PistonMod.Piston;
using Terraria.ModLoader;

namespace PistonMod
{
	public class PistonedWorld : ModWorld
	{
		public override void PostUpdate()
		{
			if(PistonSettings.Entity.PistonPushable)
				PistonMechanics.VerifyPistonIntegrity();
			PistonMechanics.EffectivelyPushEntities();
		}
	}
}