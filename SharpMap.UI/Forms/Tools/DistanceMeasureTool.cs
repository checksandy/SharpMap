using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpMap.Forms.Tools
{
    public class DistanceMeasureTool: MapTool
    {
        public DistanceMeasureTool()
            : base("DistanceMeasureTool", "A tool to measure distance")
        {
            base.Cursor = Cursors.Cross;
            _pointArray.Clear();
        }
        Control Parent;
        public void EnableDisableForm(Control MB,bool lRemove)
        {
            if (lRemove)
            {
                Parent.Invalidate(new Region(Parent.ClientRectangle));
                f_dist.Close();
                f_dist = null;
            }
            else
            {
                f_dist = new frm_Distance();
                Parent = MB;
                f_dist.Show(MB);
            }
        }

        public override bool DoMouseDown(Coordinate mapPosition, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) //dragging
            {
                _dragStartPoint = e.Location;
                _dragEndPoint = e.Location;
            }
            return false;
        }
        bool lMeasureContinue = false;

        /// <summary>
        /// Function to perform some action when a mouse button was "uped" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        public override bool DoMouseUp(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            if (lMeasureContinue)
                lMeasureContinue = false;
            else
            {
                if (_pointArray == null)
                {
                    _pointArray = new List<Coordinate>(2);
                    _pointArray.Add(mapPosition);
                    f_dist.UpdateCoordinate(mapPosition, true);
                    _pointArray.Add(mapPosition);
                    f_dist.UpdateCoordinate(mapPosition, true);
                }
                else if (_pointArray.Count==0)
                {
                    _pointArray = new List<Coordinate>(2);
                    _pointArray.Add(mapPosition);
                    f_dist.UpdateCoordinate(mapPosition, true);
                    _pointArray.Add(mapPosition);
                    f_dist.UpdateCoordinate(mapPosition, true);
                }
                else
                {
                    //var temp = new Coordinate[_pointArray.Count + 2];
                    _pointArray.Add(mapPosition);
                    f_dist.UpdateCoordinate(mapPosition, true);
                }
            }
            return false;
        }

        /// <summary>
        /// Some drawing operation of the tool
        /// </summary>
        /// <param name="e">The event's arguments</param>
        public override void DoPaint(PaintEventArgs e)
        {
            //Draws current line or polygon (Draw Line or Draw Polygon tool)
            if (_pointArray != null)
            {
                if (_pointArray.Count == 2)
                {
                    var p1 = Map.WorldToImage(_pointArray[0]);
                    var p2 = Map.WorldToImage(_pointArray[1]);
                    e.Graphics.DrawLine(new Pen(Color.Gray, 2F), p1, p2);
                }
                else
                {
                    var pts = new PointF[_pointArray.Count];
                    for (int i = 0; i < pts.Length; i++)
                        pts[i] = Map.WorldToImage(_pointArray[i]);
                        if (pts.Length > 0)
                            e.Graphics.DrawLines(new Pen(Color.Aqua, 4F), pts);
                }
            }
        }

        private List<Coordinate> _pointArray = new List<Coordinate>();
        private frm_Distance f_dist;
        private Point _dragStartPoint;
        private Point _dragEndPoint;
        private Rectangle _rectangle = Rectangle.Empty;

        /// <summary>
        /// Event invoker for the <see cref="OnEnabledChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            if (base.Enabled)
            {
                //if (f_dist != null)
                //{
                //    f_dist.Close();
                //    f_dist = null;
                //}
                //f_dist = new frm_Distance();
                //f_dist.Show();
            }
            else
            {
                if (f_dist != null)
                {
                    f_dist.Close();
                    f_dist = null;
                }
            }
            //var h = EnabledChanged;
            //if (h != null) h(this, e);
        }

        /// <summary>
        /// Function to perform some action when a mouse wheel was scrolled on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        public override bool DoMouseWheel(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return false;
        }


        //private Point ClipPoint(Point p)
        //{
        //    var x = p.X < 0 ? 0 : (p.X > Parent.ClientSize.Width ? Parent.ClientSize.Width : p.X);
        //    var y = p.Y < 0 ? 0 : (p.Y > Parent.ClientSize.Height ? Parent.ClientSize.Height : p.Y);
        //    return new Point(x, y);
        //}

        //private static Rectangle GenerateRectangle(Point p1, Point p2)
        //{
        //    var x = Math.Min(p1.X, p2.X);
        //    var y = Math.Min(p1.Y, p2.Y);
        //    var width = Math.Abs(p2.X - p1.X);
        //    var height = Math.Abs(p2.Y - p1.Y);

        //    return new Rectangle(x, y, width, height);
        //}

        /// <summary>
        /// Function to perform some action when a mouse button was moved on the map
        /// </summary>
        /// <param name="mapPosition">The position to which the mouse moved</param>
        /// <param name="e">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        public override bool DoMouseMove(Coordinate mapPosition, MouseEventArgs e)
        {
            _dragEndPoint = new Point(0, 0);
            if (_pointArray != null)
            {
                if (_pointArray.Count>1)
                {
                    _pointArray[_pointArray.Count - 1] = mapPosition;
                    f_dist.UpdateCoordinate(mapPosition, false);
                    //_rectangle = GenerateRectangle(_dragStartPoint, ClipPoint(e.Location));
                    Parent.Invalidate(new Region(Parent.ClientRectangle));
                }
            }
            return Enabled;
        }

    }

}
