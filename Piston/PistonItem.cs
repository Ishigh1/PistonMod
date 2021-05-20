using Terraria.ID;
using Terraria.ModLoader;

namespace PistonMod.Piston
{
	public class PistonItem : ModItem
	{
		public override bool Autoload(ref string name)
		{
			PistonItem pistonItem = new PistonItem();
			mod.AddItem("BasicPiston", pistonItem);
			PistonMechanics.PistonItemId = pistonItem.item.type;
			mod.AddItem("StickyPiston", new PistonItem());
			mod.AddItem("LongPiston", new PistonItem());
			mod.AddItem("StrongPiston", new PistonItem());
			return false;
		}

		public override void SetDefaults()
		{
			item.width = 24;
			item.height = 22;
			item.maxStack = 999;
			item.value = 0;
			item.rare = ItemRarityID.Green;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.mech = true;
			item.consumable = true;
			item.createTile = PistonMechanics.PistonTileId;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod) {anyIronBar = true};
			recipe.AddIngredient(ItemID.Wire, 5);
			recipe.AddIngredient(ItemID.IronBar, Name == "LongPiston" ? 20 : 5);
			switch (Name)
			{
				case "StickyPiston":
					recipe.AddIngredient(ItemID.Gel, 20);
					break;
				case "StrongPiston":
					recipe.AddIngredient(ItemID.Cog, 5);
					break;
			}

			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}