using Emotion.Common;
using Emotion.Game.AStar;
using Emotion.Game.World3D.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiniCom
{
    public class Unit : GenericObject3D
    {
        public Unit()
        {
            EntityPath = "person.em3";
            SetAnimation("Idle");
            Z = 8f;
        }

        protected override void Moved()
        {
            _z = 8f; // Model offset
            base.Moved();
        }

        public IEnumerator GoTo(Vector3 pos)
        {
            var miniComMap = Map as MiniComMap;
            if (miniComMap == null) yield break;

            Vector2 myTilePos = miniComMap.WorldToTilePos(Position);
            Vector2 goToPos = miniComMap.WorldToTilePos(pos);
            var path = miniComMap.AStarPathing.FindPath(myTilePos, goToPos, true);

            SetAnimation("Run");
            for (int i = 0; i < path.Count; i++)
            {
                var nextTile = path[i];

                var nextTileWorldPos = miniComMap.TileToWorldPos(nextTile);
                nextTileWorldPos.Z = Z;

                RotateZToFacePoint(nextTileWorldPos);

                Vector3 currentPos = Position;
                while (currentPos != nextTileWorldPos)
                {
                    Vector3 diff = nextTileWorldPos - Position;
                    diff = Vector3.Normalize(diff);

                    Position += (diff * 0.3f) * Engine.DeltaTime;
                    if ((nextTileWorldPos - Position).Length() < 5f)
                    {
                        Position = nextTileWorldPos;
                        break;
                    }

                    yield return null;
                }
            }
            SetAnimation("Idle");
        }

        protected override void OnSetEntity()
        {
            // todo: add to emotion mesh asset
            if (Entity != null && Entity.Name == "Person")
                Entity.Forward = new Vector3(0, -1, 0);

            base.OnSetEntity();
        }
    }
}
