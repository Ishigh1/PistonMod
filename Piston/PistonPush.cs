using Terraria;

namespace PistonMod.Piston
{
	public class PistonPush
	{
		//Coordinates of the entity when pushed at the maximum
		public int horizontalMax;

		//Tiles pushes in the given direction
		public int horizontalStrength;
		public int verticalMax;
		public int verticalStrength;

		public PistonPush(int horizontalStrength, int verticalStrength, int horizontalMax, int verticalMax)
		{
			this.horizontalStrength = horizontalStrength;
			this.verticalStrength = verticalStrength;
			this.horizontalMax = horizontalMax;
			this.verticalMax = verticalMax;
		}

		public void Apply(bool isPlayer, int numEntity)
		{
			Entity entity = isPlayer ? new Entity(Main.player[numEntity]) : new Entity(Main.npc[numEntity]);

			entity.X += horizontalStrength * 16;
			entity.Y += verticalStrength * 16;
			if (horizontalStrength < 0)
			{
				if (entity.X + entity.Width < horizontalMax * 16)
					entity.X = horizontalMax * 16 - entity.Width;
			}
			else if (horizontalStrength > 0)
			{
				if (entity.X > (horizontalMax + 1) * 16)
					entity.X = (horizontalMax + 1) * 16;
			}

			if (verticalStrength < 0)
			{
				if (entity.Y + entity.Height < verticalMax * 16)
					entity.Y = verticalStrength * 16 - entity.Height;
			}
			else if (verticalStrength > 0)
			{
				if (entity.Y > (verticalMax + 1) * 16)
					entity.Y = (verticalMax + 1) * 16;
			}
		}
	}
}