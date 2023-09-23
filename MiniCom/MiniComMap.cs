using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
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
        public Vector3 TileSize = new Vector3(100, 100, 1);

        protected override Task InitAsyncInternal()
        {
            GenericObject3D playerCharacter = new GenericObject3D();
            playerCharacter.EntityPath = "person.em3";
            playerCharacter.ObjectName = "Player";
            playerCharacter.SetAnimation("Idle");
            playerCharacter.Position = new Vector3(0, 0, 10);
            AddObject(playerCharacter);

            var halfMapSize = (MapSize / 2f).ToVec3() * TileSize;

            for (int x = 0; x < MapSize.X; x++)
            {
                for (int y = 0; y < MapSize.Y; y++)
                {
                    GenericObject3D tile = new GenericObject3D();
                    tile.ObjectName = $"Tile {x}x{y}";
                    tile.EntityPath = "kenney-furniture-kit/floorFull.em3";
                    tile.Position = (new Vector3(x, y, 5) * TileSize) - halfMapSize;

                    // Center
                    tile.X -= TileSize.X / 2f;
                    tile.Y -= TileSize.Y / 2f;
                    AddObject(tile);
                }
            }

            return base.InitAsyncInternal();
        }
    }
}
