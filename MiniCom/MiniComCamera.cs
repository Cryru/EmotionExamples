using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiniCom
{
    public class MiniComCamera : Camera3D
    {
        private Vector2 _inputDirection;
        private Vector3 _lookAtPoint;

        public MiniComCamera(Vector3 position) : base(position, 1f, KeyListenerType.Game)
        {
            InitializeCameraAngle();
            MoveCameraTo(position);
        }

        private void InitializeCameraAngle()
        {
            const float height = 800;
            const float angle = 55;

            // The angle between lookat and position.
            float angleRadians = Maths.DegreesToRadians(angle);

            Position = new Vector3(0, 0, height);

            Vector3 oldDiff = LookAt - Position;
            float angleXY = MathF.Atan2(oldDiff.Y, oldDiff.X);
            float cosAngleXY = MathF.Cos(angleXY);
            float sinAngleXY = MathF.Sin(angleXY);

            float distanceZ = Math.Abs(oldDiff.Z);
            float distanceX = MathF.Tan(angleRadians) * distanceZ;
            float x = MathF.Sqrt(distanceX * distanceX + distanceZ * distanceZ) * MathF.Sin(angleRadians);
            float y = 0.0f;

            _lookAtPoint = Position + new Vector3(x * cosAngleXY - y * sinAngleXY, x * sinAngleXY + y * cosAngleXY, -height);
            LookAtPoint(_lookAtPoint);
        }

        public void MoveCameraTo(Vector3 position)
        {
            Vector3 camVector = _lookAtPoint - Position;
            _lookAtPoint = position;
            camVector.Z = 0;
            Position = Position - camVector;
            LookAtPoint(_lookAtPoint);
        }

        public override void Update()
        {
            const float moveSpeed = 0.5f; // Per millisecond

            // Distance covered is based on time delta since last update and move speed.
            float moveSpeedDelta = moveSpeed * Engine.DeltaTime;

            Quaternion orientation = GetCameraOrientation();
            Vector3 xAxis = Vector3.Transform(Vector3.UnitX, orientation);
            Vector3 yAxis = Vector3.Transform(Vector3.UnitY, orientation);

            // Check for input directions and calculate offset
            Vector3 offset = Vector3.Zero;
            if (_inputDirection.X != 0)
                offset += xAxis * moveSpeedDelta * _inputDirection.X;
            if (_inputDirection.Y != 0)
                offset += yAxis * moveSpeedDelta * -_inputDirection.Y;

            Position += offset;
            _lookAtPoint += offset;
            LookAtPoint(_lookAtPoint);
        }

        public override bool CameraKeyHandler(Key key, KeyStatus status)
        {
            Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
            if (keyAxisPart != Vector2.Zero)
            {
                if (status == KeyStatus.Down)
                    _inputDirection += keyAxisPart;
                else if (status == KeyStatus.Up)
                    _inputDirection -= keyAxisPart;
                return false;
            }

            return true;
        }
    }
}
