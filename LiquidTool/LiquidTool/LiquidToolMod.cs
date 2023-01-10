using VoxelTycoon;
using VoxelTycoon.UI;
using VoxelTycoon.Modding;
using VoxelTycoon.Game.UI.ModernUI;

class LiquidToolMod : Mod
{
	protected override void OnGameStarted()
	{
		Toolbar.Current.AddButton(FontIcon.Ketizoloto(I.Steam), "Water", new ToolToolbarAction(() => new LiquidTool(Manager<AssetLibrary>.Current.Get<Item>("base/water.item").Color)));
		Toolbar.Current.AddButton(FontIcon.Ketizoloto(I.Steam), "Lava", new ToolToolbarAction(() => new LiquidTool(Manager<AssetLibrary>.Current.Get<Item>("base/lava.item").Color)));
	}
}