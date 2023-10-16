using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiniCom
{
    public class MiniComMap : Map3D
    {
        public Vector3 TileSize = new Vector3(80, 80, 1);

        [DontSerialize]
        public Quad3D TileSelectorMesh;

        public Vector3 TileToWorldPos(Vector2 tilePos)
        {
            var halfTileSize = (MapSize.ToVec3() / 2f).IntCastRound();
            return new Vector3(tilePos.X - halfTileSize.X, tilePos.Y - halfTileSize.Y, 5) * TileSize;
        }

        protected override async Task PostMapLoad()
        {
            RenderShadowMap = true;

            var grid = new InfiniteGrid();
            grid!.TileSize = TileSize.X;
            grid.Z = 10.2f;
            AddObject(grid);

            GenericObject3D playerCharacter = new GenericObject3D();
            playerCharacter.EntityPath = "person.em3";
            playerCharacter.ObjectName = "Player";
            playerCharacter.SetAnimation("Idle");
            playerCharacter.Position = new Vector3(0, 0, 9f);
            AddObject(playerCharacter);
            await AwaitAllObjectsLoaded();
            playerCharacter.Entity.Forward = new Vector3(0, -1, 0); // todo: add to emotion mesh asset

            for (int x = 0; x < MapSize.X; x++)
            {
                for (int y = 0; y < MapSize.Y; y++)
                {
                    GroundTile tile = new GroundTile();
                    tile.ObjectName = $"Tile {x}x{y}";
                    tile.EntityPath = "kenney-furniture-kit/floorFull.em3";
                    tile.TileCoord = new Vector2(x, y);
                    tile.Position = TileToWorldPos(new Vector2(x, y));
                    tile.Size3D = new Vector3(TileSize.X / 10f, TileSize.Y / 10f, 10f);

                    // Center
                    //tile.X += TileSize.X / 2f;
                    //tile.Y += TileSize.Y / 2f;
                    AddObject(tile);
                }
            }

            TileSelectorMesh = new Quad3D();
            TileSelectorMesh.Size3D = new Vector3(TileSize.X, TileSize.Y, 1f);
            TileSelectorMesh.Tint = Color.PrettyGreen;
            TileSelectorMesh.ObjectFlags &= ~Emotion.Game.World.ObjectFlags.Map3DDontReceiveAmbient;
            AddObject(TileSelectorMesh);

            await base.PostMapLoad();
        }
    }
}
