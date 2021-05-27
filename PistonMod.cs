using System.Collections.Generic;
using System.IO;
using PistonMod.Piston;
using Terraria;
using Terraria.ModLoader;

namespace PistonMod
{
	public class PistonMod : Mod
	{
		
		public override void Load()
		{
			PistonMechanics.PistonsInDanger = new List<(int, int)>();
			PistonMechanics.EntitiesToPush = new Dictionary<(bool, int), PistonPush>();
			PistonMechanics.PistonTileId = GetTile("PistonTile").Type;
			PistonSettings.Entity = (PistonSettings) GetConfig("PistonSettings");
		}

		public override void Unload()
		{
			PistonMechanics.PistonsInDanger = null;
			PistonMechanics.EntitiesToPush = null;
			PistonSettings.Entity = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			switch (reader.ReadByte())
			{
				case (byte) NetCodes.PistonPush:
					PistonMechanics.ExtendPiston(reader.ReadInt32(), reader.ReadInt32());
					break;
				case (byte) NetCodes.PistonRetract:
					PistonMechanics.RetractPiston(reader.ReadInt32(), reader.ReadInt32());
					break;
			}
		}
	}
}