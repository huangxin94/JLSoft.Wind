using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind.UserControl
{
    public class RobotArmControl2 : Control
    {
        #region 控制参数
        private float _currentBaseAngle;
        private float _targetBaseAngle;
        private float _currentElbowAngle = 90f;
        private float _targetElbowAngle = 90f;
        private float _currentGripOpening = 0.4f;
        private float _targetGripOpening = 0.4f;
        private float _currentNormalizedPosition = 0.5f;
        private float _targetNormalizedPosition = 0.5f;
        private const int DEPTH = 20;
        private const int ARM_LENGTH = 100;
        private const float ANIMATION_SPEED = 0.1f;
        private const int ANIMATION_INTERVAL = 16;
        private readonly Timer _animationTimer;
        #endregion

        #region 外观属性
        public Color BaseColor { get; set; } = Color.SteelBlue;
        public Color ArmColor { get; set; } = Color.DimGray;
        public Color JointColor { get; set; } = Color.Gold;
        public Color GripColor { get; set; } = Color.Firebrick;
        public Color RailwayColor { get; set; } = Color.FromArgb(80, Color.DimGray);
        public int ArmThickness { get; set; } = 8;
        #endregion

        public RobotArmControl2()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            Size = new Size(400, 300);
            BackColor = Color.Transparent;

            _animationTimer = new Timer { Interval = ANIMATION_INTERVAL };
            _animationTimer.Tick += AnimationTick;
            _animationTimer.Start();
        }

        #region 动画控制
        private void AnimationTick(object sender, EventArgs e)
        {
            bool needsUpdate = false;
            needsUpdate |= UpdateValue(ref _currentBaseAngle, _targetBaseAngle);
            needsUpdate |= UpdateValue(ref _currentElbowAngle, _targetElbowAngle);
            needsUpdate |= UpdateValue(ref _currentGripOpening, _targetGripOpening);
            needsUpdate |= UpdateValue(ref _currentNormalizedPosition, _targetNormalizedPosition);

            if (needsUpdate) Invalidate();
        }

        private static bool UpdateValue(ref float current, float target)
        {
            if (Math.Abs(current - target) < 0.01f) return false;
            current += (target - current) * ANIMATION_SPEED;
            return true;
        }
        #endregion

        #region 公开控制方法
        public void MoveTo(float? position = null, float? baseAngle = null,
                          float? elbowAngle = null, float? grip = null)
        {
            if (position.HasValue)
                _targetNormalizedPosition = Math.Clamp(position.Value, 0f, 1f);

            if (baseAngle.HasValue)
                _targetBaseAngle = Math.Clamp(baseAngle.Value, -45f, 45f);

            if (elbowAngle.HasValue)
                _targetElbowAngle = Math.Clamp(elbowAngle.Value, 0f, 180f);

            if (grip.HasValue)
                _targetGripOpening = Math.Clamp(grip.Value, 0f, 1f);
        }

        public float TrackPosition
        {
            get => _currentNormalizedPosition;
            set => _targetNormalizedPosition = Math.Clamp(value, 0f, 1f);
        }
        #endregion

        #region 绘制方法
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawRailway(g);
            DrawArmStructure(g);
            DrawStatusText(g);
        }

        private void DrawArmStructure(Graphics g)
        {
            // 计算基座位置
            int railWidth = Width - 2 * DEPTH;
            float baseX = DEPTH + _currentNormalizedPosition * railWidth;
            var basePoint = new PointF(baseX, Height - 50);

            // 绘制基座
            using (var baseBrush = new SolidBrush(BaseColor))
            {
                g.FillEllipse(baseBrush, basePoint.X - 30, basePoint.Y - 15, 60, 30);
            }

            // 计算关节位置
            var shoulder = basePoint;
            var elbow = CalculatePoint(shoulder, _currentBaseAngle, ARM_LENGTH);
            var wrist = CalculatePoint(elbow, _currentBaseAngle + _currentElbowAngle, ARM_LENGTH * 0.8f);

            // 绘制机械臂
            DrawArmSegment(g, shoulder, elbow);
            DrawArmSegment(g, elbow, wrist);
            DrawJoint(g, shoulder);
            DrawJoint(g, elbow);
            DrawJoint(g, wrist);
            DrawGripper(g, wrist, _currentBaseAngle + _currentElbowAngle, _currentGripOpening);
        }

        private void DrawRailway(Graphics g)
        {
            int railY = Height - 40;
            using var railBrush = new SolidBrush(RailwayColor);
            using var railPen = new Pen(Color.Silver, 3) { EndCap = System.Drawing.Drawing2D.LineCap.Round };

            // 轨道基础
            g.FillRectangle(railBrush, 0, railY - 15, Width, 15);

            // 轨道线
            g.DrawLine(railPen, DEPTH, railY, Width - DEPTH, railY);
        }

        private void DrawStatusText(Graphics g)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);
            g.DrawString($"Position: {_currentNormalizedPosition:P0}", font, brush, 10, 10);
            g.DrawString($"Base: {_currentBaseAngle:F1}°", font, brush, 10, 25);
            g.DrawString($"Elbow: {_currentElbowAngle:F1}°", font, brush, 10, 40);
            g.DrawString($"Grip: {_currentGripOpening:P0}", font, brush, 10, 55);
        }
        #endregion

        #region 绘图辅助方法
        private PointF CalculatePoint(PointF origin, float angle, float length)
        {
            float radians = angle * (float)Math.PI / 180f;
            return new PointF(
                origin.X + length * (float)Math.Cos(radians),
                origin.Y - length * (float)Math.Sin(radians));
        }

        private void DrawArmSegment(Graphics g, PointF start, PointF end)
        {
            using var armPen = new Pen(ArmColor, ArmThickness)
            {
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            g.DrawLine(armPen, start, end);
        }

        private void DrawJoint(Graphics g, PointF center)
        {
            int size = ArmThickness + 6;
            using var brush = new SolidBrush(JointColor);
            g.FillEllipse(brush, center.X - size / 2f, center.Y - size / 2f, size, size);
        }

        private void DrawGripper(Graphics g, PointF wrist, float angle, float opening)
        {
            float gripWidth = 30 * opening;
            float gripLength = ARM_LENGTH * 0.3f;

            var left = CalculatePoint(wrist, angle - 90, gripWidth);
            var right = CalculatePoint(wrist, angle + 90, gripWidth);
            var tip = CalculatePoint(wrist, angle, gripLength);

            using var gripPen = new Pen(GripColor, ArmThickness - 2)
            {
                EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor
            };

            g.DrawLine(gripPen, wrist, left);
            g.DrawLine(gripPen, wrist, right);
            g.DrawLine(gripPen, left, tip);
            g.DrawLine(gripPen, right, tip);
        }
        #endregion
    }
}