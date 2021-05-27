using Terraria;

namespace PistonMod.Piston
{
	public class PistonPush
	{
		//Coordinates of the entity when pushed at the maximum
		public int HorizontalMax;

		//Tiles pushes in the given direction
		public int HorizontalStrength;
		public int VerticalMax;
		public int VerticalStrength;

		public PistonPush(int horizontalStrength, int verticalStrength, int horizontalMax, int verticalMax)
		{
			HorizontalStrength = horizontalStrength;
			VerticalStrength = verticalStrength;
			HorizontalMax = horizontalMax;
			VerticalMax = verticalMax;
		}

		public void Apply(bool isPlayer, int numEntity)
		{
			Entity entity = isPlayer ? new Entity(Main.player[numEntity]) : new Entity(Main.npc[numEntity]);

			entity.X += HorizontalStrength * 16;
			entity.Y += VerticalStrength * 16;
			if (HorizontalStrength < 0)
			{
				if (entity.X + entity.Width < HorizontalMax * 16)
					entity.X = HorizontalMax * 16 - entity.Width;
			}
			else if (HorizontalStrength > 0)
			{
				if (entity.X > (HorizontalMax + 1) * 16)
					entity.X = (HorizontalMax + 1) * 16;
			}

			if (VerticalStrength < 0)
			{
				if (entity.Y + entity.Height < VerticalMax * 16)
					entity.Y = VerticalStrength * 16 - entity.Height;
			}
			else if (VerticalStrength > 0)
			{
				if (entity.Y > (VerticalMax + 1) * 16)
					entity.Y = (VerticalMax + 1) * 16;
			}
		}
	}
}