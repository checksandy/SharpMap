using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Forms.Tools
{
    /// <summary>
    /// Area Measurement
    /// </summary>
    public class AreaMeasureTool : MapTool
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AreaMeasureTool(MapBox pMB)
            : base("AreaMeasureTool", "A tool to measure area")
        {
            MB = pMB;
            Map = MB.Map;
        }
        
        /// <summary>
        /// Map Box Handle
        /// </summary>
        public MapBox MB;

        byte lMPause = 0;
        private List<Coordinate> _pointArray = new List<Coordinate>();
        private frm_Area f_area;
        private Point _dragStartPoint;
        private Point _dragEndPoint;

        /// <summary>
        /// Function to perform some action when a mouse button was "downed" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse button was downed</param>
        /// <param name="e">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        public override bool DoMouseDown(Coordinate mapPosition, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) //dragging
            {
                _dragStartPoint = e.Location;
                _dragEndPoint = e.Location;
                if (lMPause >= 1)
                {
                    _pointArray.Clear();
                    f_area.ClearValues();
                    MB.Invalidate(new Region(MB.ClientRectangle));
                    lMPause = 0;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (_pointArray.Count > 3)
                {
                    _pointArray.RemoveAt(_pointArray.Count - 1);
                    f_area.SetForAdd();
                    MB.Invalidate(new Region(MB.ClientRectangle));
                }
                else
                {
                    f_area.ClearValues();
                    _pointArray.Clear();
                    MB.Invalidate(new Region(MB.ClientRectangle));
                }
                lMPause += 1;
            }
            return false;
        }

        /// <summary>
        /// Function to perform some action when a mouse button was "uped" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        public override bool DoMouseUp(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            if (lMPause == 1)
                lMPause += 1;
            else if (lMPause == 2)
            {
                f_area.ClearValues();
                _pointArray.Clear();
                lMPause = 0;
            }
            else
            {
                if (_pointArray == null)
                {
                    _pointArray = new List<Coordinate>(2);
                    _pointArray.Add(mapPosition);
                    f_area.UpdateCoordinate(mapPosition, true);
                    _pointArray.Add(mapPosition);
                    f_area.UpdateCoordinate(mapPosition, true);
                }
                else if (_pointArray.Count == 0)
                {
                    _pointArray = new List<Coordinate>(2);
                    _pointArray.Add(mapPosition);
                    f_area.UpdateCoordinate(mapPosition, true);
                    _pointArray.Add(mapPosition);
                    f_area.UpdateCoordinate(mapPosition, true);
                }
                else
                {
                    //var temp = new Coordinate[_pointArray.Count + 2];
                    _pointArray.Add(mapPosition);
                    f_area.UpdateCoordinate(mapPosition, true);
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
                if (_pointArray.Count > 1)
                {
                    if (_pointArray.Count == 2)
                    {
                        var p1 = Map.WorldToImage(_pointArray[0]);
                        var p2 = Map.WorldToImage(_pointArray[1]);
                        e.Graphics.DrawLine(new Pen(Color.YellowGreen, 2F), p1, p2);
                    }
                    else
                    {
                        var pts = new PointF[_pointArray.Count];
                        for (int i = 0; i < pts.Length; i++)
                            pts[i] = Map.WorldToImage(_pointArray[i]);
                        if (pts.Length > 0)
                        {
                            Color c = Color.FromArgb(127, Color.Gray);
                            e.Graphics.FillPolygon(new SolidBrush(c), pts);
                            e.Graphics.DrawPolygon(new Pen(Color.Gray, 2F), pts);
                        }
                        //e.Graphics.DrawLines(new Pen(Color.YellowGreen, 2F), pts);
                    }
                    for (int i = 0; i < _pointArray.Count - 1; i++)
                    {
                        PointF curpts = Map.WorldToImage(_pointArray[i]);
                        e.Graphics.DrawEllipse(new Pen(Color.Red, 2F), new Rectangle((int)curpts.X - 2, (int)curpts.Y - 2, 4, 4));
                    }
                }
            }
        }

        /// <summary>
        /// Event invoker for the <see cref="OnEnabledChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            if (base.Enabled)
            {
                base.Cursor = Cursors.Cross;
                if (_pointArray != null) _pointArray.Clear();
                f_area = new frm_Area(this);
                f_area.Show(MB);
            }
            else
            {
                MB.Invalidate(new Region(MB.ClientRectangle));
                if (f_area != null) f_area.Close();
                if (_pointArray != null) _pointArray.Clear();
                f_area = null;
                lMPause = 0;
            }
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
                if (lMPause == 0)
                {
                    if (_pointArray.Count > 1)
                    {
                        _pointArray[_pointArray.Count - 1] = mapPosition;
                        f_area.UpdateCoordinate(mapPosition, false);
                        //_rectangle = GenerateRectangle(_dragStartPoint, ClipPoint(e.Location));
                        MB.Invalidate(new Region(MB.ClientRectangle));
                    }
                }
            }
            return Enabled;
        }

        /// <summary>
        /// Function to Clear
        /// </summary>
        public void Clear()
        {
            _pointArray.Clear();
            MB.Invalidate(new Region(MB.ClientRectangle));
            lMPause = 0;
        }
    }
}
