using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiniCom
{
    public class GameScene : World3DBaseScene<MiniComMap>
    {
        public override async Task LoadAsync()
        {
            var cam3D = new MiniComCamera(Vector3.Zero);
            Engine.Renderer.Camera = cam3D;

            var gameMap = await Engine.AssetLoader.GetAsync<XMLAsset<MiniComMap>>("game_map.xml");
            if (gameMap?.Content != null) await ChangeMapAsync(gameMap.Content);

            Engine.Host.OnKey.AddListener(PlayerInput);
        }

        public override void Update()
        {
            var currentMap = CurrentMap as MiniComMap;
            if (currentMap != null && currentMap.Initialized && !currentMap.EditorMode)
            {
                var player = currentMap.GetObjectByName("Player") as GameObject3D;
                //player.Position2 += (movementInput * new Vector2(-1, 1) * 0.3f) * Engine.DeltaTime;

                //bool isMoving = movementInput != Vector2.Zero;
                //if (isMoving && !wasMoving)
                //{
                //    player.SetAnimation("Run");
                //}
                //else if (!isMoving && wasMoving)
                //{
                //    player.SetAnimation("Idle");
                //}
                //wasMoving = isMoving;

                var newTileUnderMouse = new Vector2(-1);
                var mouseRay = Engine.Renderer.Camera.GetCameraMouseRay();
                var enumerator = currentMap.GetObjectsByType<GroundTile>();
                while (enumerator.MoveNext())
                {
                    var tile = enumerator.Current;
                    if (mouseRay.IntersectWithObject(tile, out Mesh? _, out Vector3 _, out Vector3 _, out int _))
                    {
                        newTileUnderMouse = tile.TileCoord;
                        break;
                    }
                }

                if (newTileUnderMouse != _mouseUnderTile)
                {
                    currentMap.TileSelectorMesh.Position = currentMap.TileToWorldPos(newTileUnderMouse) + new Vector3(1, 1, 0);
                    currentMap.TileSelectorMesh.Z = 10.25f;
                    _mouseUnderTile = newTileUnderMouse;
                }
            }

            base.Update();
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            base.Draw(composer);
        }

        private Vector2 _mouseUnderTile;

        private Vector2 movementInput;
        private bool wasMoving = false;

        private Coroutine _playerAction;

        private bool PlayerInput(Key key, KeyStatus status)
        {
            if (_playerAction != null && !_playerAction.Finished) return true;

            Vector2 partOfAxis = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
            if (status == KeyStatus.Down)
            {
                movementInput += partOfAxis;
            }
            else
            {
                movementInput -= partOfAxis;
            }

            if (key == Key.MouseKeyLeft && status == KeyStatus.Up)
            {
                if (_mouseUnderTile != new Vector2(-1))
                {
                    var player = CurrentMap.GetObjectByName("Player") as Unit;
                    Vector3 tileWorldPos = CurrentMap.TileToWorldPos(_mouseUnderTile);

                    _playerAction = Engine.CoroutineManager.StartCoroutine(player.GoTo(tileWorldPos));
                }
            }

            return true;
        }
    }
}
