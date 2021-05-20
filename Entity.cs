using Terraria;

namespace PistonMod
{
	public class Entity
	{
		public object LinkedEntity;

		public Entity(object linkedEntity)
		{
			LinkedEntity = linkedEntity;
		}

		public float X
		{
			get
			{
				if (LinkedEntity is Player player)
					return player.position.X;
				else
					return ((NPC) LinkedEntity).position.X;
			}
			set
			{
				if (LinkedEntity is Player player)
					player.position.X = value;
				else
					((NPC) LinkedEntity).position.X = value;
			}
		}

		public float Y
		{
			get
			{
				if (LinkedEntity is Player player)
					return player.position.Y;
				else
					return ((NPC) LinkedEntity).position.Y;
			}
			set
			{
				if (LinkedEntity is Player player)
					player.position.Y = value;
				else
					((NPC) LinkedEntity).position.Y = value;
			}
		}

		public float Width
		{
			get
			{
				if (LinkedEntity is Player player)
					return player.width;
				else
					return ((NPC) LinkedEntity).width;
			}
		}

		public float Height
		{
			get
			{
				if (LinkedEntity is Player player)
					return player.height;
				else
					return ((NPC) LinkedEntity).height;
			}
		}
	}
}