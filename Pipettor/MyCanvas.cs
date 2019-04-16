using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pipettor
{
    class MyCanvas :Canvas
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MyVisualHost visualHost = new MyVisualHost();
        List<System.Drawing.PointF> tubePositions = new List<System.Drawing.PointF>();
        System.Drawing.PointF jointPostion;
        System.Drawing.PointF ptIntersect1, ptIntersect2;
        Point dispensePt;
        public float arm1Angle;
        public float arm2Angle;

        int currentTubeIndex = 0;

        public MyCanvas()
        {
            this.Children.Add(visualHost);
            ptIntersect1.X = ptIntersect2.X = -1;
            dispensePt.X = -1;
            jointPostion = GetJointPosition();
           
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            int carrierCnt = GlobalVars.carrierCnt;
            tubePositions.Clear();
            for (int i = 0; i< carrierCnt; i++)
            {
                Draw16Tubes(i, dc);
            }
            DrawBaseJoint(dc);
            Pen redPen = new Pen(Brushes.Red, 3);
            if (ptIntersect1.X != -1)
            {
                dc.DrawEllipse(Brushes.Black, new Pen(Brushes.Black, 3), new Point(ptIntersect1.X, ptIntersect1.Y), 5, 5);
            }
            if (ptIntersect2.X != -1)
            {
                dc.DrawEllipse(Brushes.Transparent, redPen, new Point(ptIntersect2.X, ptIntersect2.Y), 5, 5);
            }
            if(dispensePt.X != -1)
            {
                DrawCrossLine(dispensePt,dc);
                DrawCrossLine(new Point(jointPostion.X,jointPostion.Y),dc);
                //dc.DrawLine()
            }

        }

        private void DrawCrossLine(Point pt,DrawingContext dc)
        {
            Pen redPen = new Pen(Brushes.Red, 3);
            Point ptStart = new Point(pt.X - 10, pt.Y);
            Point ptEnd = new Point(pt.X + 10, pt.Y);
            dc.DrawLine(redPen, ptStart, ptEnd);
            ptStart = new Point(pt.X, pt.Y - 10);
            ptEnd = new Point(pt.X, pt.Y + 10);
            dc.DrawLine(redPen, ptStart, ptEnd);
        }

        System.Drawing.PointF GetJointPosition()
        {
            int x = (int)GlobalVars.GetXPosition(GlobalVars.carrierCnt/2 - 0.5);
            int y = (GlobalVars.carrierLength + GlobalVars.yMargin + GlobalVars.jointSafeDelta) * GlobalVars.ratio;
            return new System.Drawing.PointF(x, y);
        }
        private void DrawBaseJoint(DrawingContext dc)
        {
            int jointRadius = GlobalVars.jointRadius * GlobalVars.ratio;
            jointPostion = GetJointPosition();
            int x = (int)jointPostion.X;
            int y = (int)jointPostion.Y;
            dc.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Blue, 3), new Point(x,y), jointRadius, jointRadius);
            dc.DrawEllipse(Brushes.Transparent, new Pen(Brushes.LightGreen, 3), new Point(x, y), MyVisualHost.firstArmLen * GlobalVars.ratio, MyVisualHost.firstArmLen * GlobalVars.ratio);
        }

        private void Draw16Tubes(int carrierIndex, DrawingContext dc)
        {
            int xPosition = (int)GlobalVars.GetXPosition(carrierIndex);
            int startY = (GlobalVars.tubeDistance * GlobalVars.tubeCntPerCarrier + GlobalVars.yMargin)*GlobalVars.ratio;
            for( int i = 0; i < GlobalVars.tubeCntPerCarrier; i++)
            {
                int tubeGlobalIndex = carrierIndex * GlobalVars.tubeCntPerCarrier + i;
                int y = startY - i * GlobalVars.tubeDistance * GlobalVars.ratio;
                tubePositions.Add(new System.Drawing.PointF(xPosition, y));
                DrawTube(xPosition, y, dc, tubeGlobalIndex== currentTubeIndex);
            }
        }

        //public void RoateArm1(float degrees)
        //{
        //    visualHost.RotateArm1(degrees);
        //    InvalidateVisual();
        //}
      

        private void DrawTube(int x, int y, DrawingContext dc, bool isCurrentTube)
        {
            double tubeRadius = 6.5* GlobalVars.ratio;
            Brush brush = isCurrentTube ? Brushes.Red : Brushes.Blue;
            dc.DrawEllipse(Brushes.Transparent, new Pen(brush, 3), new System.Windows.Point(x, y), tubeRadius, tubeRadius);

            if(isCurrentTube)
            {
                int r = MyVisualHost.secondArmLen * GlobalVars.ratio;
                dc.DrawEllipse(Brushes.Transparent, new Pen(Brushes.LightCoral, 3), new System.Windows.Point(x, y), r, r);
            }
        }

        internal float GetArm1Angle() {return arm1Angle;}
        internal float GetArm2Angle() {return arm2Angle;}

        void Move2Point(System.Drawing.PointF dstPostion)
        {
           
            int cnt = CircleIntersection.FindIntersections(jointPostion, dstPostion, MyVisualHost.firstArmLen * GlobalVars.ratio, MyVisualHost.secondArmLen * GlobalVars.ratio, out ptIntersect1, out ptIntersect2);
            if (cnt == 2)
                SelectRightOneAsFirst(ref ptIntersect1, ref ptIntersect2);

            //move arms
            visualHost.MoveArm1(ptIntersect1);
            Thread.Sleep(200);
            visualHost.MoveArm2(ptIntersect1, dstPostion);
            InvalidateVisual();
        }

        void Refresh()
        {
            Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
               new Action(() =>
               {
                    // Do something here.
                }));
        }
       

        internal void Move2Tube(int tubeID)
        {
            int tubeIndex = tubeID - 1;
            currentTubeIndex = tubeIndex;
            if (tubeID > 72 || tubeID < 1)
            {
                MessageBox.Show("TubeID Greater than 72 or Less than 1", "ERROR");
               
                return;
            }
                
            System.Drawing.PointF pt1 = tubePositions[tubeIndex];
            dispensePt = new Point(pt1.X,pt1.Y);
            Move2Point(new System.Drawing.PointF((float)dispensePt.X, (float)dispensePt.Y));
        }

        private void SelectRightOneAsFirst(ref System.Drawing.PointF ptIntersect1, ref System.Drawing.PointF ptIntersect2)
        {
            double distance2 = GetDistance(ptIntersect2, visualHost.ptArm2Axis);
            if(ptIntersect2.X > ptIntersect1.X)
            {
                System.Drawing.PointF tmp = ptIntersect1;
                ptIntersect1 = ptIntersect2;
                ptIntersect2 = tmp;
            }

        }

        private double GetDistance(System.Drawing.PointF pt, System.Windows.Point dispensePt)
        {
            double xDiff = pt.X - dispensePt.X;
            double yDiff = pt.Y - dispensePt.Y;
            return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }
    }


    public static class VectorExt
    {
        private const double DegToRad = Math.PI / 180;

        public static Vector Rotate(this Vector v, double degrees)
        {
            return v.RotateRadians(degrees * DegToRad);
        }

        public static Vector RotateRadians(this Vector v, double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }


    class MyVisualHost: FrameworkElement
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private VisualCollection _children;
        public Point ptArm1Axis;
        public Point ptArm2Axis;
        public Point ptArm2AxisOrg;//原点
        public Point ptDispense;

        public const int secondArmLen = 120;
        public const int firstArmLen = 165;
        public const int firstJointMotorRadius = 45;
        public MyVisualHost()
        {
          
            _children = new VisualCollection(this);
           //Point ptSecondArmBottomLeft;
            _children.Add(CreateFirstArm(out ptArm2Axis));
            ptArm2AxisOrg = ptArm2Axis;
            _children.Add(CreateSecondArm(ptArm2Axis, new Vector(0,0)));
        }

        public void RotateArm1(float degrees)
        {
            //drawingVisual.Transform = new RotateTransform(30, 100, 100);
            var drawingVisual = _children[0] as DrawingVisual;
            drawingVisual.Transform = new RotateTransform(degrees, ptArm1Axis.X, ptArm1Axis.Y);
            
        }

        internal void MoveArm2(System.Drawing.PointF ptAxisNew, System.Drawing.PointF ptNewDispense)
        {
            _children.RemoveAt(1);
            var drawingVisual = CreateSecondArm(ptArm2Axis,new Vector(ptAxisNew.X - ptArm2Axis.X,ptAxisNew.Y - ptArm2Axis.Y));

            Vector vecNew = new Vector(ptNewDispense.X - ptAxisNew.X, ptNewDispense.Y - ptAxisNew.Y);//use the new axis
            Vector vecOrg = new Vector(ptDispense.X - ptArm2Axis.X, ptDispense.Y - ptArm2Axis.Y);//this vector is fixed, never change
            float angle = (float)Vector.AngleBetween(vecNew, vecOrg);
            if (ptNewDispense.X < ptDispense.X + ptAxisNew.X - ptArm2Axis.X) //rotate ccw
            {
                angle = -angle;
            }
        
            log.InfoFormat("Arm2RotateInfo  X:{0},Y:{1},Angle:{2}", ptAxisNew.X, ptAxisNew.Y,angle);
            MotorController.Instance.Rotate2ABSAngle(2,angle);

            var rotateTransform = new RotateTransform(angle, ptAxisNew.X, ptAxisNew.Y);
            drawingVisual.Transform = rotateTransform;
            _children.Add(drawingVisual);

        }


        public void MoveArm1(System.Drawing.PointF ptNew)
        {
            Vector vecNew = new Vector(ptNew.X - ptArm1Axis.X, ptNew.Y - ptArm1Axis.Y);
            Vector vecOrg = new Vector(ptArm2AxisOrg.X - ptArm1Axis.X, ptArm2AxisOrg.Y - ptArm1Axis.Y);

            float angle = (float)Vector.AngleBetween(vecNew, vecOrg);
            
            angle = (float)Math.Abs(angle);
            if (ptNew.X < ptArm2AxisOrg.X) //rotate ccw
            {
                RotateArm1(-angle);
                angle = - angle;
            }
            else
            {
                RotateArm1(angle);

            }

            log.InfoFormat("Arm1RotateInfo: X:{0},Y:{1},Angle:{2}", ptNew.X, ptNew.Y, angle);
            MotorController.Instance.Rotate2ABSAngle(1, angle);

        }
       



        private DrawingVisual CreateSecondArm(Point ptSecondArmAxis,Vector offset)
        {
            int armWidth = 20;
          
            Point pt1 = new Point(ptSecondArmAxis.X - (armWidth / 2) * GlobalVars.ratio, ptSecondArmAxis.Y);
            Point pt2 = new Point(pt1.X, pt1.Y - secondArmLen * GlobalVars.ratio);
            Point pt3 = new Point(pt2.X + armWidth * GlobalVars.ratio, pt2.Y);
            Point pt4 = new Point(ptSecondArmAxis.X + (armWidth / 2) * GlobalVars.ratio, ptSecondArmAxis.Y);
            List<Point> pts = new List<Point>();
            pt1 += offset;
            pt2 += offset;
            pt3 += offset;
            pt4 += offset;
            pts.Add(pt1);
            pts.Add(pt2);
            pts.Add(pt3);
            pts.Add(pt4);
            ptArm2Axis = new Point((pt1.X + pt4.X) / 2, pt1.Y);
            ptDispense = new Point((pt1.X + pt4.X) / 2, pt2.Y);
            
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            for (int i = 0; i < pts.Count; i++)
            {
                Point ptStart = pts[i];
                Point ptEnd = pts[(i + 1) % pts.Count];
                drawingContext.DrawLine(new Pen(Brushes.Orange, 3), ptStart, ptEnd);
            }
            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1), ptArm2Axis, 3, 3);
            drawingContext.DrawEllipse(null, new Pen(Brushes.Red, 1), ptArm2Axis, GlobalVars.ratio* armWidth/2, GlobalVars.ratio * armWidth /2);
            drawingContext.DrawEllipse(Brushes.Green, new Pen(Brushes.Green, 1), ptDispense, 6,6);
            drawingContext.Close();
            return drawingVisual;
        }
       
        private DrawingVisual CreateFirstArm(out Point ptSecondArmAxis)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            
            int armWidth = 40;
           
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            int x = (int)GlobalVars.GetXPosition(2.5);
            int y = (GlobalVars.carrierLength + GlobalVars.yMargin + GlobalVars.jointSafeDelta) * GlobalVars.ratio;
            
            List<Point> pts = new List<Point>();
            Point pt1 = new Point(x + GlobalVars.jointRadius * GlobalVars.ratio, y );
            Point pt2 = new Point(x - GlobalVars.jointRadius * GlobalVars.ratio, y);
            Point pt3 = new Point(x, y -GlobalVars.jointRadius * GlobalVars.ratio);
           

            ptSecondArmAxis = new Point(x,y - firstArmLen*GlobalVars.ratio) ;
            Point pt4 = new Point(x - armWidth/2 * GlobalVars.ratio, ptSecondArmAxis.Y);
            Point pt5 = new Point(x + armWidth/2 * GlobalVars.ratio, ptSecondArmAxis.Y);
            pts.Add(pt1);
            pts.Add(pt2);
            //pts.Add(pt3);
            pts.Add(pt4);
            pts.Add(pt5);

            Point ptCircle = new Point(x, y);
            
            for (int i = 0; i< pts.Count; i++)
            {
                Point ptStart = pts[i];
                Point ptEnd = pts[(i + 1) % pts.Count];
                drawingContext.DrawLine(new Pen(Brushes.DarkOliveGreen, 3), ptStart, ptEnd);
            }
            ptArm1Axis = ptCircle;
            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1), ptCircle, 3, 3);
            drawingContext.Close();

            return drawingVisual;
        }

        public VisualCollection Visuals
        {
            get
            {
                return _children;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }


        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

       
    }


}
