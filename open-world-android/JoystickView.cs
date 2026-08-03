using System;
using Android.Content;
using Android.Graphics;
using Android.Views;

namespace open_world
{
    // Kreslí se přes Android Canvas (View.OnDraw), ne přes MonoGame SpriteBatch -
    // proto ho žádný GL/GLES state z DuckManager nemůže ovlivnit.
    public class JoystickView : View
    {
        private const float MaxRadius = 90f;

        private Paint _basePaint;
        private Paint _stickPaint;

        private bool _active;
        private PointF _center;
        private PointF _stickPos;

        public JoystickView(Context context) : base(context)
        {
            _basePaint = new Paint(PaintFlags.AntiAlias) { Color = new Color(255, 255, 255, 60) };
            _stickPaint = new Paint(PaintFlags.AntiAlias) { Color = new Color(255, 255, 255, 150) };
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    _active = true;
                    _center = new PointF(e.GetX(), e.GetY());
                    _stickPos = _center;
                    break;

                case MotionEventActions.Move:
                    if (_active)
                    {
                        _stickPos = new PointF(e.GetX(), e.GetY());
                        UpdateMovementVector();
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    _active = false;
                    NativeInput.MovementVector = Microsoft.Xna.Framework.Vector2.Zero;
                    break;
            }

            Invalidate(); // vynutí překreslení
            return true;
        }

        private void UpdateMovementVector()
        {
            float dx = _stickPos.X - _center.X;
            float dy = _stickPos.Y - _center.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist > MaxRadius)
            {
                dx = dx / dist * MaxRadius;
                dy = dy / dist * MaxRadius;
                _stickPos = new PointF(_center.X + dx, _center.Y + dy);
            }

            // Obrazovkové Y roste dolů -> tažení nahoru (záporné dy) = dopředu (kladné Y).
            var vec = new Microsoft.Xna.Framework.Vector2(dx / MaxRadius, -dy / MaxRadius);
            if (vec.LengthSquared() > 1f) vec.Normalize();

            NativeInput.MovementVector = vec;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!_active) return;

            canvas.DrawCircle(_center.X, _center.Y, MaxRadius, _basePaint);
            canvas.DrawCircle(_stickPos.X, _stickPos.Y, 30f, _stickPaint);
        }
    }
}