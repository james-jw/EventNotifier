using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventNotifier.UI
{
    public class PieSlice : Shape
    {
        public readonly static DependencyProperty RadiusProperty;
        public readonly static DependencyProperty RotationProperty;
        public readonly static DependencyProperty PieAngleProperty;
        public readonly static DependencyProperty CenterXProperty;
        public readonly static DependencyProperty CenterYProperty;

        private PathFigure _figure;
        private PathGeometry _geom;
        private Point startPoint;
        private Point arcStartPoint;
        private Point arcEndPoint;
        private bool _isLargeArc;

        public double CenterX {
            get {
                return (double)base.GetValue(PieSlice.CenterXProperty);
            }
            set {
                base.SetValue(PieSlice.CenterXProperty, value);
            }
        }

        public double CenterY {
            get {
                return (double)base.GetValue(PieSlice.CenterYProperty);
            }
            set {
                base.SetValue(PieSlice.CenterYProperty, value);
            }
        }

        protected override Geometry DefiningGeometry {
            get {
                this.startPoint.X = this.CenterX + this.Radius;
                this.startPoint.Y = this.CenterY + this.Radius;
                double radians = this.DegreesToRadians(90 - this.Rotation);
                this.PolarToCartesianPoint(radians, this.Radius, ref this.arcStartPoint);
                this.arcStartPoint.Offset(this.startPoint.X, this.startPoint.Y);
                radians = this.DegreesToRadians(90 - (this.Rotation + this.PieAngle));
                this.PolarToCartesianPoint(this.DegreesToRadians(90 - (this.Rotation + this.PieAngle)), this.Radius, ref this.arcEndPoint);
                this.arcEndPoint.Offset(this.startPoint.X, this.startPoint.Y);
                this._isLargeArc = (this.PieAngle < 180 ? false : true);
                this._geom.Figures.Clear();
                if (this._figure == null)
                {
                    this._figure = new PathFigure();
                }
                this._figure.Segments.Clear();
                this._figure.StartPoint = this.startPoint;
                this._figure.Segments.Add(new LineSegment(this.arcStartPoint, true));
                this._figure.Segments.Add(new ArcSegment(this.arcEndPoint, new Size(this.Radius, this.Radius), 0, this._isLargeArc, SweepDirection.Clockwise, false));
                this._figure.Segments.Add(new LineSegment(this.startPoint, false));
                this._geom.Figures.Add(this._figure);
                return this._geom;
            }
        }

        public double PieAngle {
            get {
                return (double)base.GetValue(PieSlice.PieAngleProperty);
            }
            set {
                base.SetValue(PieSlice.PieAngleProperty, value);
            }
        }

        public double Radius {
            get {
                return (double)base.GetValue(PieSlice.RadiusProperty);
            }
            set {
                base.SetValue(PieSlice.RadiusProperty, value);
            }
        }

        public double Rotation {
            get {
                return (double)base.GetValue(PieSlice.RotationProperty);
            }
            set {
                base.SetValue(PieSlice.RotationProperty, value);
            }
        }

        static PieSlice()
        {
            PieSlice.RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(PieSlice), new UIPropertyMetadata(new PropertyChangedCallback(PieSlice.RenderDefiningPropertyChanged)));
            PieSlice.RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(PieSlice), new UIPropertyMetadata(new PropertyChangedCallback(PieSlice.RenderDefiningPropertyChanged)));
            PieSlice.PieAngleProperty = DependencyProperty.Register("PieAngle", typeof(double), typeof(PieSlice), new UIPropertyMetadata(new PropertyChangedCallback(PieSlice.RenderDefiningPropertyChanged)));
            PieSlice.CenterXProperty = DependencyProperty.Register("CenterX", typeof(double), typeof(PieSlice), new UIPropertyMetadata(new PropertyChangedCallback(PieSlice.RenderDefiningPropertyChanged)));
            PieSlice.CenterYProperty = DependencyProperty.Register("CenterY", typeof(double), typeof(PieSlice), new UIPropertyMetadata(new PropertyChangedCallback(PieSlice.RenderDefiningPropertyChanged)));
        }

        public PieSlice()
        {
            this.startPoint = new Point();
            this.arcStartPoint = new Point();
            this.arcEndPoint = new Point();
            this._geom = new PathGeometry();
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * 0.0174532925199433;
        }

        private void PolarToCartesianPoint(double Angle, double Radius, ref Point pointIn)
        {
            pointIn.X = Math.Cos(Angle) * Radius;
            pointIn.Y = Math.Sin(Angle) * -Radius;
        }

        private double RadiansToDegrees(double radians)
        {
            return radians * 57.2957795130823;
        }

        private static void RenderDefiningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null)
            {
                return;
            }
            ((PieSlice)d).InvalidateVisual();
        }
    }
}
