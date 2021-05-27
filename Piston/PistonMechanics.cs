using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PistonMod.Piston
{
	public class PistonMechanics
	{
		public static List<(int, int)> PistonsInDanger;
		public static Dictionary<(bool, int), PistonPush> EntitiesToPush;
		public static int PistonItemId;
		public static ushort PistonTileId;

		public static void ExtendPiston(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
			(int horizontalDirection, int verticalDirection) = pistonVariant.GetDirection();
			PushTiles(x, y, horizontalDirection, verticalDirection, tile, pistonVariant);
		}

		public static void PushTiles(int x, int y, int horizontalDirection, int verticalDirection, Tile tile,
			PistonVariant pistonVariant)
		{
			int iMin = 0;
			int push = pistonVariant.GetPush();
			int strength = pistonVariant.GetStrength() + push;
			int pushed = 0;

			for (int i = 1; i <= strength && pushed != push; i++)
			{
				int tmpX = x + horizontalDirection * i;
				int tmpY = y + verticalDirection * i;
				Tile tileSafely = Framing.GetTileSafely(tmpX, tmpY);
				if (!tileSafely.active() || !Main.tileSolid[tileSafely.type])
				{
					if (!PistonSettings.Entity.PistonPushable &&
					    PistonVariant.ExtractVariant(tileSafely)?.Status != PistonStatus.Retracted) break;

					WorldGen.KillTile(tmpX, tmpY);
					if (Framing.GetTileSafely(tmpX, tmpY).active())
						break;
					pushed++;
					iMin = i;
				}
			}

			if (pushed == 0) return;

			push = 0;
			for (int i = iMin; i >= 1; i--)
			{
				int tmpX = x + horizontalDirection * i;
				int tmpY = y + verticalDirection * i;
				Tile oldTile = Framing.GetTileSafely(tmpX, tmpY);
				if (!oldTile.active())
				{
					push++;
				}
				else
				{
					Tile newTile = Framing.GetTileSafely(tmpX + horizontalDirection * push,
						tmpY + verticalDirection * push);
					newTile.active(true);
					newTile.type = oldTile.type;
					newTile.frameX = oldTile.frameX;
					newTile.frameY = oldTile.frameY;
					newTile.inActive(oldTile.inActive());
					newTile.halfBrick(oldTile.halfBrick());
					newTile.slope(oldTile.slope());
					WorldGen.SquareTileFrame(tmpX + horizontalDirection * push, tmpY + verticalDirection * push, false);
					AddToPistonInDanger(tmpX, tmpY, push, horizontalDirection, verticalDirection, newTile);
				}

				PushEntities(tmpX, tmpY, x, y, horizontalDirection, verticalDirection, push, iMin);
			}

			pistonVariant.Status = PistonStatus.Extended;
			pistonVariant.SetCorrectFraming(tile);
			pistonVariant.Status = PistonStatus.Bar;
			for (int i = 1; i <= pushed; i++)
			{
				tile = Framing.GetTileSafely(x + horizontalDirection * i, y + verticalDirection * i);
				tile.active(true);
				tile.inActive(false);
				tile.type = PistonTileId;
				if (i == pushed)
					pistonVariant.Status = PistonStatus.Head;
				pistonVariant.SetCorrectFraming(tile);
			}

			WorldGen.SquareTileFrame(x + horizontalDirection, y + verticalDirection);
		}

		public static void PushEntities(int x, int y, int pistonX, int pistonY, int horizontalDirection,
			int verticalDirection, int pushEntity, int totalPushed)
		{
			int xStart = x * 16;
			int yStart = y * 16;
			int width = horizontalDirection == 0 ? 16 : 16 * horizontalDirection;
			int height = verticalDirection == 0 ? 16 : 16 * verticalDirection;
			Rectangle rectangle = new Rectangle(width < 0 ? xStart + width : xStart,
				height < 0 ? yStart + height : yStart,
				Math.Abs(width), Math.Abs(height));
			foreach (NPC npc in Main.npc)
			{
				Rectangle targetRectangle = npc.getRect();
				if (npc.active && !npc.noTileCollide && targetRectangle.Intersects(rectangle))
					SaveEntityToPush(false, npc.whoAmI, pistonX, pistonY, horizontalDirection, verticalDirection,
						pushEntity,
						totalPushed);
			}

			foreach (Player player in Main.player)
			{
				Rectangle targetRectangle = player.getRect();
				if (player.active && !player.dead && targetRectangle.Intersects(rectangle))
					SaveEntityToPush(true, player.whoAmI, pistonX, pistonY, horizontalDirection, verticalDirection,
						pushEntity,
						totalPushed);
			}
		}

		public static void SaveEntityToPush(bool isPlayer, int id, int x, int y, int horizontalDirection,
			int verticalDirection, int pushEntity, int totalPushed)
		{
			(bool isPlayer, int id) key = (isPlayer, id);
			int horizontalStrength = horizontalDirection * pushEntity;
			int verticalStrength = verticalDirection * pushEntity;
			int horizontalMax = x + horizontalDirection * totalPushed;
			int verticalMax = y + verticalDirection * totalPushed;
			if (!EntitiesToPush.TryGetValue(key, out PistonPush push))
			{
				EntitiesToPush.Add(key,
					new PistonPush(horizontalStrength, verticalStrength, horizontalMax, verticalMax));
				return;
			}

			if (push.HorizontalStrength == 0 || horizontalStrength / (float) push.HorizontalStrength > 1)
				push.HorizontalStrength = horizontalStrength;
			if (push.HorizontalMax * horizontalDirection < horizontalMax * horizontalDirection)
				push.HorizontalMax = horizontalMax;

			if (push.VerticalStrength == 0 || verticalStrength / (float) push.VerticalStrength > 1)
				push.VerticalStrength = verticalStrength;
			if (push.VerticalMax * verticalDirection < verticalMax * verticalDirection)
				push.VerticalMax = verticalMax;
		}

		public static void RetractPiston(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
			(int horizontalDirection, int verticalDirection) = pistonVariant.GetDirection();
			int pushAmount = pistonVariant.GetPush();

			int pulledTileX = x + horizontalDirection * (pushAmount + 1);
			int pulledTileY = y + verticalDirection * (pushAmount + 1);
			Tile pulledTile = Framing.GetTileSafely(pulledTileX, pulledTileY);
			bool sticky = pistonVariant.Type == PistonType.Sticky && pulledTile.active() &&
			              Main.tileSolid[pulledTile.type];
			if (sticky)
			{
				if(!PistonSettings.Entity.PistonPushable &&
				       PistonVariant.ExtractVariant(pulledTile)?.Status != PistonStatus.Retracted) return;
				if (pistonVariant.Orientation != Orientation.Down)
				{
					Tile aboveTile = Framing.GetTileSafely(pulledTileX, pulledTileY - 1);
					if (aboveTile.active() &&
					    (TileID.Sets.BasicChest[aboveTile.type] || TileLoader.IsDresser(aboveTile.type)))
					{
						WorldGen.KillTile(pulledTileX, pulledTileY - 1);
						if (Framing.GetTileSafely(pulledTileX, pulledTileY - 1).active())
							return;
					}
				}
			}


			for (int i = pushAmount; i >= 1; i--)
			{
				Tile headTile = Framing.GetTileSafely(x + horizontalDirection * i, y + verticalDirection * i);
				PistonVariant pistonVariant2 = PistonVariant.ExtractVariant(headTile);
				if (pistonVariant2 != null && pistonVariant.Orientation == pistonVariant2.Orientation &&
				    pistonVariant.Type == pistonVariant2.Type &&
				    (i == pushAmount && pistonVariant2.Status == PistonStatus.Head ||
				     i != pushAmount && pistonVariant2.Status == PistonStatus.Bar))
				{
					if (pistonVariant.Orientation != Orientation.Down)
					{
						Tile aboveTile =
							Framing.GetTileSafely(x + horizontalDirection * i, y + verticalDirection * i - 1);
						if (aboveTile.active() &&
						    (TileID.Sets.BasicChest[aboveTile.type] || TileLoader.IsDresser(aboveTile.type)))
						{
							WorldGen.KillTile(x + horizontalDirection * i, y + verticalDirection * i - 1);
							if (Framing.GetTileSafely(x + horizontalDirection * i, y + verticalDirection * i - 1)
								.active())
							{
								Tile endTile = null;
								if (sticky)
								{
									if (i > 1)
									{
										endTile = Framing.GetTileSafely(x + horizontalDirection * (i - 1),
											y + verticalDirection * (i - 1));
										headTile.active(false);
										WorldGen.SquareTileFrame(x + horizontalDirection * i,
											y + verticalDirection * i);
									}

									pushAmount = i - 1;
								}
								else
								{
									endTile = Framing.GetTileSafely(x + horizontalDirection * i,
										y + verticalDirection * i);
									pushAmount = i;
								}

								if (pushAmount != 0)
								{
									pistonVariant.Status = PistonStatus.Head;
									pistonVariant.SetCorrectFraming(endTile);
								}

								break;
							}
						}
					}

					headTile.active(false);
					WorldGen.SquareTileFrame(x + horizontalDirection * i, y + verticalDirection * i);
				}
				else
				{
					if (i == pushAmount && i > 1)
					{
						pushAmount--;
					}
					else
					{
						WorldGen.KillTile(x, y);
						return;
					}
				}

				if (i == 1)
					pushAmount = 0;
			}

			if (sticky)
			{
				Tile newPosition =
					Framing.GetTileSafely(x + horizontalDirection * (pushAmount + 1),
						y + verticalDirection * (pushAmount + 1));

				newPosition.active(true);
				newPosition.type = pulledTile.type;
				newPosition.frameX = pulledTile.frameX;
				newPosition.frameY = pulledTile.frameY;
				newPosition.inActive(pulledTile.inActive());
				newPosition.halfBrick(pulledTile.halfBrick());
				newPosition.slope(pulledTile.slope());
				pulledTile.active(false);
				AddToPistonInDanger(pulledTileX, pulledTileY, -(pushAmount + 1), horizontalDirection, verticalDirection,
					newPosition);
				WorldGen.SquareTileFrame(x + horizontalDirection, y + verticalDirection);
				WorldGen.SquareTileFrame(pulledTileX, pulledTileY);
			}

			if (pushAmount == 0)
			{
				pistonVariant.Status = PistonStatus.Retracted;
				pistonVariant.SetCorrectFraming(tile);
			}
		}

		public static void AddToPistonInDanger(int x, int y, int distance, int horizontalDirection,
			int verticalDirection, Tile pistonTile)
		{
			PistonVariant pistonVariant = PistonVariant.ExtractVariant(pistonTile);
			if (pistonVariant != null && pistonVariant.Status != PistonStatus.Retracted)
			{
				PistonsInDanger.Add((x + horizontalDirection * distance, y + verticalDirection * distance));
				(int horizontalDirection2, int verticalDirection2) = pistonVariant.GetDirection();

				if (pistonVariant.Status == PistonStatus.Extended ||
				    pistonVariant.Status == PistonStatus.Bar)
					PistonsInDanger.Add((x + horizontalDirection2, y + verticalDirection2));
				if (pistonVariant.Status == PistonStatus.Head ||
				    pistonVariant.Status == PistonStatus.Bar)
					PistonsInDanger.Add((x - horizontalDirection2, y - verticalDirection2));
			}
		}

		public static void VerifyPistonIntegrity()
		{
			foreach ((int x, int y) in PistonsInDanger)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				PistonVariant pistonVariant = PistonVariant.ExtractVariant(tile);
				if (pistonVariant == null || pistonVariant.Status == PistonStatus.Retracted) continue;
				(int horizontalDirection, int verticalDirection) = pistonVariant.GetDirection();
				Tile friendlyPistonTile;
				if (pistonVariant.Status == PistonStatus.Head || pistonVariant.Status == PistonStatus.Bar)
				{
					friendlyPistonTile = Framing.GetTileSafely(x - horizontalDirection, y - verticalDirection);
					PistonVariant pistonVariant2 = PistonVariant.ExtractVariant(friendlyPistonTile);
					if (pistonVariant2 == null || pistonVariant.Orientation != pistonVariant2.Orientation ||
					    pistonVariant.Type != pistonVariant2.Type ||
					    !(pistonVariant2.Status == PistonStatus.Extended || pistonVariant2.Status == PistonStatus.Bar))
						WorldGen.KillTile(x, y);
				}

				if (pistonVariant.Status == PistonStatus.Extended || pistonVariant.Status == PistonStatus.Bar)
				{
					friendlyPistonTile = Framing.GetTileSafely(x + horizontalDirection, y + verticalDirection);
					PistonVariant pistonVariant2 = PistonVariant.ExtractVariant(friendlyPistonTile);
					if (pistonVariant2 == null || pistonVariant.Orientation != pistonVariant2.Orientation ||
					    pistonVariant.Type != pistonVariant2.Type ||
					    !(pistonVariant2.Status == PistonStatus.Head || pistonVariant2.Status == PistonStatus.Bar))
						WorldGen.KillTile(x, y);
				}
			}

			PistonsInDanger.Clear();
		}

		public static void EffectivelyPushEntities()
		{
			foreach (KeyValuePair<(bool, int), PistonPush> keyValuePair in EntitiesToPush)
			{
				(bool isPlayer, int numEntity) = keyValuePair.Key;
				PistonPush value = keyValuePair.Value;
				value.Apply(isPlayer, numEntity);
			}

			EntitiesToPush.Clear();
		}
	}
}