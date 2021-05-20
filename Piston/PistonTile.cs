using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PistonMod.Piston
{
	public class PistonTile : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;

			AddMapEntry(new Color(20, 20, 20));
		}

		public override bool Drop(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
			if (pistonVariant == null) return true;
			tile.active(false);
			(int horizontalDirection, int verticalDirection) = pistonVariant.GetDirection();
			Tile otherTile;
			PistonVariant pistonVariant2;
			switch (pistonVariant.Status)
			{
				case PistonStatus.Extended:
					otherTile = Framing.GetTileSafely(x + horizontalDirection, y + verticalDirection);
					pistonVariant2 = PistonVariant.ExtractVariant(otherTile);
					if (pistonVariant2 != null && pistonVariant2.Orientation == pistonVariant.Orientation &&
					    pistonVariant2.Type == pistonVariant.Type &&
					    (pistonVariant2.Status == PistonStatus.Bar || pistonVariant2.Status == PistonStatus.Head))
						WorldGen.KillTile(x + horizontalDirection, y + verticalDirection);
					goto case PistonStatus.Retracted;
				case PistonStatus.Retracted:
					drop = (int) (PistonMechanics.PistonItemId + pistonVariant.Type);
					return true;
				case PistonStatus.Bar:
					otherTile = Framing.GetTileSafely(x + horizontalDirection, y + verticalDirection);
					pistonVariant2 = PistonVariant.ExtractVariant(otherTile);
					if (pistonVariant2 != null && pistonVariant2.Orientation == pistonVariant.Orientation &&
					    pistonVariant2.Type == pistonVariant.Type &&
					    (pistonVariant2.Status == PistonStatus.Bar || pistonVariant2.Status == PistonStatus.Head))
						WorldGen.KillTile(x + horizontalDirection, y + verticalDirection);
					goto case PistonStatus.Head;
				case PistonStatus.Head:
					otherTile = Framing.GetTileSafely(x - horizontalDirection, y - verticalDirection);
					pistonVariant2 = PistonVariant.ExtractVariant(otherTile);
					if (pistonVariant2 != null && pistonVariant2.Orientation == pistonVariant.Orientation &&
					    pistonVariant2.Type == pistonVariant.Type &&
					    (pistonVariant2.Status == PistonStatus.Bar || pistonVariant2.Status == PistonStatus.Extended))
						WorldGen.KillTile(x - horizontalDirection, y - verticalDirection);

					return false;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void PlaceInWorld(int x, int y, Item item)
		{
			Tile tile = Main.tile[x, y];
			Orientation orientation = Main.LocalPlayer.direction == 1 ? Orientation.Right : Orientation.Left;

			PistonType type = (PistonType) (item.type - PistonMechanics.PistonItemId);
			PistonVariant pistonVariant = new PistonVariant(PistonStatus.Retracted, orientation, type);
			pistonVariant.SetCorrectFraming(tile);

			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1);
		}

		public override bool Slope(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
			if (pistonVariant.Status != PistonStatus.Retracted) return false;
			pistonVariant.Turn();
			pistonVariant.SetCorrectFraming(tile);
			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1);

			return false;
		}

		public override void HitWire(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);

			// Wiring.CheckMech checks if the wiring cooldown has been reached. Put a longer number here for less frequent projectile spawns. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
			if (Wiring.CheckMech(x, y, 30))
			{
				PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
				switch (pistonVariant.Status)
				{
					case PistonStatus.Retracted:
						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket modPacket = mod.GetPacket();
							modPacket.Write((byte) NetCodes.PistonPush);
							modPacket.Write(x);
							modPacket.Write(y);
							modPacket.Send();
						}

						PistonMechanics.ExtendPiston(x, y);
						break;
					case PistonStatus.Extended:
						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket modPacket = mod.GetPacket();
							modPacket.Write((byte) NetCodes.PistonRetract);
							modPacket.Write(x);
							modPacket.Write(y);
							modPacket.Send();
						}

						PistonMechanics.RetractPiston(x, y);
						break;
					case PistonStatus.Head:
						break;
					case PistonStatus.Bar:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}