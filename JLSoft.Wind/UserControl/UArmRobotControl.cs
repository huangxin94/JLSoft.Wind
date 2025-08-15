using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind.UserControl
{
    public class UArmRobotControl : Control
    {
        #region 状态参数
        private float _currentPosition = 0.5f;  // 水平位置 0~1
        private float _currentHeight = 0.5f;     // 垂直高度 0~1
        private bool _isGripping;
        private float _targetPosition = 0.5f;
        private float _targetHeight = 0.5f;
        private const int AnimationInterval = 16;
        private readonly Timer _animationTimer;
        #endregion

        #region 外观属性
        public Color TrackColor { get; set; } = Color.FromArgb(80, Color.Gray);
        public Color ArmColor { get; set; } = Color.SteelBlue;
        public Color UArmColor { get; set; } = Color.DimGray;
        public Color GripColor { get; set; } = Color.FromArgb(150, Color.Gold);
        public int ArmThickness { get; set; } = 10;
        #endregion

        public UArmRobotControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw, true);
            Size = new Size(600, 400);

            _animationTimer = new Timer { Interval = AnimationInterval };
            _animationTimer.Tick += (s, e) => UpdateAnimation();
            _animationTimer.Start();
        }

        #region 动画更新
        private void UpdateAnimation()
        {
            _currentPosition += (_targetPosition - _currentPosition) * 0.1f;
            _currentHeight += (_targetHeight - _currentHeight) * 0.1f;
            Invalidate();
        }
        #endregion

        #region 公开控制方法
        public void MoveTo(float? position = null, float? height = null, bool? gripping = null)
        {
            if (position.HasValue)
                _targetPosition = Math.Clamp(position.Value, 0f, 1f);

            if (height.HasValue)
                _targetHeight = Math.Clamp(height.Value, 0f, 1f);

            if (gripping.HasValue)
                _isGripping = gripping.Value;
        }
        #endregion

        #region 绘制方法
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawTrack(g);
            DrawRobot(g);
            DrawStatus(g);
        }

        private void DrawTrack(Graphics g)
        {
            // 居中轨道
            int trackY = Height / 2;
            using (var pen = new Pen(TrackColor, 8))
            {
                g.DrawLine(pen, 50, trackY, Width - 50, trackY);
            }
        }

        private void DrawRobot(Graphics g)
        {
            // 基座位置计算
            float baseX = 50 + (Width - 100) * _currentPosition;
            float baseY = Height / 2;  // 轨道居中

            // 垂直运动范围（总高度40%）
            float verticalRange = Height * 0.4f;
            float armY = baseY - verticalRange * (1 - _currentHeight); ;

            DrawVerticalArm(g, baseX, baseY, armY);
            DrawUArm(g, baseX, armY);
        }

        private void DrawVerticalArm(Graphics g, float baseX, float baseY, float armY)
        {
            // 垂直立柱
            using (var pen = new Pen(ArmColor, ArmThickness))
            {
                g.DrawLine(pen, baseX, baseY - 10, baseX, armY);
            }

            // 滑动基座
            using (var brush = new SolidBrush(ArmColor))
            {
                g.FillEllipse(brush, baseX - 15, baseY - 20, 30, 30);
            }
        }

        private void DrawUArm(Graphics g, float pivotX, float pivotY)
        {
            // U型叉参数（调整为向上弯曲）
            float armWidth = 80f;
            float armDepth = -40f; // 负值表示向上
            float curveDepth = 15f;
            float suctionRadius = 12f;

            // 左臂路径（向上弯曲）
            PointF[] leftArm = {
                        new PointF(pivotX - armWidth/2, pivotY),
                        new PointF(pivotX - armWidth/2 + curveDepth, pivotY + armDepth/2),
                        new PointF(pivotX - armWidth/2, pivotY + armDepth)
                        };

            // 右臂路径（对称结构）
            PointF[] rightArm = {
                        new PointF(pivotX + armWidth/2, pivotY),
                        new PointF(pivotX + armWidth/2 - curveDepth, pivotY + armDepth/2),
                        new PointF(pivotX + armWidth/2, pivotY + armDepth)
                        };

            using (var pen = new Pen(Color.Silver, 6f))
            {
                g.DrawCurve(pen, leftArm);
                g.DrawCurve(pen, rightArm);

                // 调整横梁绘制位置（在上方）
                g.DrawArc(pen,
                    pivotX - armWidth / 2,
                    pivotY + armDepth - 5f, // 调整Y坐标
                    armWidth,
                    10f,
                    0f, 180f); // 修改角度方向
            }

            if (_isGripping)
            {
                float suctionY = pivotY + armDepth - 8f; // 向上调整
                using (var brush = new SolidBrush(Color.FromArgb(180, Color.Gold)))
                {
                    g.FillEllipse(brush,
                        pivotX - suctionRadius,
                        suctionY - suctionRadius,
                        suctionRadius * 2f,
                        suctionRadius * 2f);
                }
            }
        }

        private void DrawStatus(Graphics g)
        {
            string status = $"Position: {_currentPosition:P0}\nHeight: {_currentHeight:P0}";
            using (var font = new Font("Arial", 9))
            using (var brush = new SolidBrush(_isGripping ? Color.Green : Color.Gray))
            {
                g.DrawString(status, font, brush, 10, 10);
            }
        }
        #endregion
    }
}