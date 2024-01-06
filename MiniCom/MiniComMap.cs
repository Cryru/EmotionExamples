using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.AStar;
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
        public PathingGrid PathingGrid = null!;

        [DontSerialize]
        public AStarContext AStarPathing = null!;

        [DontSerialize]
        public Quad3D TileSelectorMesh = null!;

        public Vector3 TileToWorldPos(Vector2 tilePos)
        {
            var halfTileSize = (MapSize.ToVec3() / 2f).IntCastRound();
            return new Vector3(tilePos.X - halfTileSize.X, tilePos.Y - halfTileSize.Y, 5) * TileSize;
        }

        public Vector2 WorldToTilePos(Vector3 worldPos)
        {
            var halfTileSize = (MapSize.ToVec3() / 2f).IntCastRound() * TileSize;
            return ((worldPos + halfTileSize) / TileSize).Floor().ToVec2();
        }

        protected override async Task PostMapLoad()
        {
            RenderShadowMap = true;

            var grid = new SquareGrid3D();
            grid!.TileSize = TileSize.X;
            grid.Z = 10.5f;
            grid.Tint = Color.PrettyPurple;
            grid.Size3D = (MapSize * TileSize.ToVec2()).ToVec3(1f);
            AddObject(grid);

            Unit playerCharacter = new Unit();
            playerCharacter.ObjectName = "Player";
            playerCharacter.Tint = Color.PrettyBlue;
            playerCharacter.Position = TileToWorldPos(new Vector2(15, 15));
            AddObject(playerCharacter);

            Unit enemyCharacter = new Unit();
            enemyCharacter.ObjectName = "Enemy";
            enemyCharacter.Position = TileToWorldPos(new Vector2(20, 15));
            AddObject(enemyCharacter);

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

            PathingGrid = new PathingGrid(MapSize, TileSize.ToVec2());
            AStarPathing = new AStarContext(PathingGrid);

            TileSelectorMesh = new Quad3D();
            TileSelectorMesh.Size3D = new Vector3(TileSize.X, TileSize.Y, 1f);
            TileSelectorMesh.Tint = Color.PrettyGreen;
            TileSelectorMesh.ObjectFlags &= ~Emotion.Game.World.ObjectFlags.Map3DDontReceiveAmbient;
            AddObject(TileSelectorMesh);

            await base.PostMapLoad();
        }
    }
}
