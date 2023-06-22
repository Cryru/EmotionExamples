#region Using

using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;

#endregion

#nullable enable

namespace BeachPong_World2D.Objects;

public class BackgroundImage : GameObject2D
{
	[AssetFileName] public string? AssetFile;

	private TextureAsset? _asset;

	public override async Task LoadAssetsAsync()
	{
		if (string.IsNullOrEmpty(AssetFile)) return;

		_asset = await Engine.AssetLoader.GetAsync<TextureAsset>(AssetFile);
	}

	protected override void RenderInternal(RenderComposer c)
	{
		c.RenderSprite(Position, Size, Color.White, _asset?.Texture);
	}
}