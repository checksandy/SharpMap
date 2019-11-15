using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

namespace SharpMap.Forms.Tools
{
    public partial class frm_Distance : Form
    {
        /// <summary>
        /// Window to display Distance
        /// </summary>
        public frm_Distance(DistanceMeasureTool cpD)
        {
            InitializeComponent();
            cpDist = cpD;
            cmbUnit.Items.Add("Meters");
            cmbUnit.Items.Add("Kilometers");
            cmbUnit.Items.Add("Feets");
            cmbUnit.Items.Add("Yards");
            cmbUnit.Items.Add("Miles");
            cmbUnit.Items.Add("Nautical Miles");
            cmbUnit.Items.Add("Centimeters");
            cmbUnit.Items.Add("Millimeters");
            cmbUnit.SelectedIndex = 0;
            btAdd.Enabled = false;
        }
        private List<Coordinate> _pDist = new List<Coordinate>();
        private List<Coordinate> _UTMDist = new List<Coordinate>();
        private List<double> segDist=new List<double>();
        private DistanceMeasureTool cpDist;

        private int nZone;
        private bool lNorth;
        /// <summary>
        /// Updates Coordinate
        /// </summary>
        public void UpdateCoordinate(Coordinate mapPosition, bool lAdd)
        {
            var mathTransform = LayerTools.GoogleMercatorToWgs84.MathTransform;
            GeoAPI.Geometries.Coordinate geom = GeometryTransform.TransformCoordinate(new Coordinate(mapPosition.X, mapPosition.Y), mathTransform);
            if (lAdd)
            {
                double X, Y;
                if (_pDist.Count==0)
                {
                    NETGeographicLib.UTMUPS.Forward(geom.Y, geom.X, out nZone, out lNorth, out X, out Y, -1, true);
                    _pDist.Add(mapPosition);
                    _UTMDist.Add(new Coordinate(X,Y));
                }
                else
                {
                    try
                    {
                        int nZ;
                        bool lN;
                        NETGeographicLib.UTMUPS.Forward(geom.Y, geom.X, out nZ, out lN, out X, out Y, nZone, false);
                        _pDist.Add(mapPosition);
                        _UTMDist.Add(new Coordinate(X, Y));
                    }
                    catch{}
                }
            }
            else
            {
                try
                {
                    int nZ;
                    bool lN;
                    double X, Y;
                    NETGeographicLib.UTMUPS.Forward(geom.Y, geom.X, out nZ, out lN, out X, out Y, nZone, false);
                    _pDist[_pDist.Count - 1] = mapPosition;
                    _UTMDist[_UTMDist.Count - 1] = new Coordinate(X, Y);                   
                }
                catch { }
            }
            if (_pDist.Count>1) refreshDisplay();
        }

        private string FormattedDistanceValue(double cV)
        {
            double mF = 1;
            string suff = " m";
            switch (cmbUnit.SelectedIndex)
            {
                case 0:
                    mF = 1;
                    suff = " m";
                    break;
                case 1:
                    mF = .001;
                    suff = " Km";
                    break;
                case 2:
                    mF = 1 / 3.2808398950134;
                    suff = "ft";
                    break;
                case 3:
                    mF = 1.09361329835;
                    suff = "yd";
                    break;
                case 4:
                    mF = 0.00062137119224;
                    suff = "mi";
                    break;
                case 5:
                    mF = 0.0005399568035;
                    suff = "NM";
                    break;
                case 6:
                    mF = 100;
                    suff = "cm";
                    break;
                case 7:
                    mF = 1000;
                    suff = "mm";
                    break;
            }
            double fVal = cV * mF;
            return fVal.ToString("0.0###") + suff;
        }
        private void refreshDisplay()
        {
            double dist = Math.Sqrt(Math.Pow((_UTMDist[_UTMDist.Count-1].X - _UTMDist[_UTMDist.Count-2].X), 2) + Math.Pow(((_UTMDist[_UTMDist.Count - 1].Y - _UTMDist[_UTMDist.Count - 2].Y)), 2));
            if (LV.Items.Count<_pDist.Count-1)
            {
                segDist.Add(dist);
                ListViewItem curDValue=new ListViewItem();
                curDValue.Text = FormattedDistanceValue(dist);
                txtDist.Text = FormattedDistanceValue(TotalDistance());
                LV.Items.Add(curDValue);
                LV.EnsureVisible(LV.Items.Count - 1);
            }
            else
            {
                segDist[segDist.Count-1]=dist;
                LV.Items[LV.Items.Count - 1].Text = FormattedDistanceValue(dist);
                txtDist.Text = FormattedDistanceValue(TotalDistance());
            }
        }

        double TotalDistance()
        {
            double t_dist = 0;
            for (int i = 0; i < segDist.Count; i++)
            {
                t_dist += segDist[i];
            }
            return t_dist;
        }

        private void btNew_Click(object sender, EventArgs e)
        {
            ClearValues();
            cpDist.Clear();
        }

        /// <summary>
        /// Clear Values
        /// </summary>
        public void ClearValues()
        {
            btAdd.Enabled = false;
            LV.Items.Clear();
            segDist.Clear();
            _pDist.Clear();
            _UTMDist.Clear();
            txtDist.Text = "";
        }
        /// <summary>
        /// Add options
        /// </summary>
        public void SetForAdd()
        {
            if (segDist.Count>1)
            {
                segDist.RemoveAt(segDist.Count - 1);
                _pDist.RemoveAt(_pDist.Count - 1);
                LV.Items.RemoveAt(LV.Items.Count - 1);
                _UTMDist.RemoveAt(_UTMDist.Count - 1);
                txtDist.Text = FormattedDistanceValue(TotalDistance());
                btAdd.Enabled = true;
            }
            else
                btNew_Click(null, null);
        }

        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (segDist.Count>0)
            {
                for (int i=0;i<segDist.Count;i++)
                {
                    LV.Items[i].Text = FormattedDistanceValue(segDist[i]);
                }
                txtDist.Text= FormattedDistanceValue(TotalDistance());
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            if (segDist.Count>0)
                cpDist.MB.DefineLinearGeometry(_pDist);
            btNew_Click(null, null);
        }
    }
}
