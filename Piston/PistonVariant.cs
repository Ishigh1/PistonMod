using System;
using Terraria;

namespace PistonMod.Piston
{
	public enum PistonStatus
	{
		Retracted,
		Extended,
		Head,
		Bar
	}

	public enum Orientation
	{
		Left,
		Right,
		Down,
		Up
	}

	public enum PistonType
	{
		Normal,
		Sticky,
		Long,
		Strong
	}

	public class PistonVariant
	{
		public Orientation Orientation;

		public PistonStatus Status;
		public PistonType Type;

		public PistonVariant(PistonStatus status, Orientation orientation, PistonType type)
		{
			Status = status;
			Orientation = orientation;
			Type = type;
		}

		public static PistonVariant ExtractVariant(Tile tile)
		{
			if (!tile.active() || tile.type != PistonMechanics.PistonTileId) return null;
			short frameX = tile.frameX;
			short frameY = tile.frameY;
			PistonStatus pistonStatus = (PistonStatus) (frameX / 80);
			Orientation orientation = (Orientation) (frameX % 80 / 18);
			PistonType type = (PistonType) (frameY / 18);
			return new PistonVariant(pistonStatus, orientation, type);
		}

		public void SetCorrectFraming(Tile tile)
		{
			tile.frameX = (short) ((short) Orientation * 18 + (short) Status * 80);
			tile.frameY = (short) ((short) Type * 18);
		}

		public (int horizontalDirection, int verticalDirection) GetDirection()
		{
			switch (Orientation)
			{
				case Orientation.Left:
					return (-1, 0);
				case Orientation.Right:
					return (1, 0);
				case Orientation.Up:
					return (0, -1);
				case Orientation.Down:
					return (0, 1);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Turn()
		{
			Orientation = (Orientation) (((int) Orientation + 1) % 4);
		}

		public int GetStrength()
		{
			switch (Type)
			{
				case PistonType.Normal:
				case PistonType.Sticky:
					return 10;
				case PistonType.Long:
					return 15;
				case PistonType.Strong:
					return 100;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public int GetPush()
		{
			switch (Type)
			{
				case PistonType.Normal:
				case PistonType.Sticky:
				case PistonType.Strong:
					return 1;
				case PistonType.Long:
					return 2;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}