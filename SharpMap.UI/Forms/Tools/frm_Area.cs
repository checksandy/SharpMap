using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

namespace SharpMap.Forms.Tools
{

    public partial class frm_Area : Form
    {
        private AreaMeasureTool cpArea;
        private List<Coordinate> _pDist = new List<Coordinate>();
        private List<Coordinate> _UTMDist = new List<Coordinate>();

        public frm_Area(AreaMeasureTool cpA)
        {
            InitializeComponent();
            cpArea = cpA;
            cmbUnit.Items.Add("square meters");
            cmbUnit.Items.Add("square kilometers");
            cmbUnit.Items.Add("square feet");
            cmbUnit.Items.Add("square meters");
            cmbUnit.Items.Add("square yards");
            cmbUnit.Items.Add("square miles");
            cmbUnit.Items.Add("hectares");
            cmbUnit.Items.Add("acres");
            cmbUnit.Items.Add("acre gunta");
            cmbUnit.Items.Add("square centimeters");
            cmbUnit.Items.Add("square millimeters");
            cmbUnit.Items.Add("square nautical kilometers");
            cmbUnit.SelectedIndex = 0;
            btAdd.Enabled = false;
        }

        int nZone;
        bool lNorth;
        public void UpdateCoordinate(Coordinate mapPosition, bool lAdd)
        {
            var mathTransform = LayerTools.GoogleMercatorToWgs84.MathTransform;
            GeoAPI.Geometries.Coordinate geom = GeometryTransform.TransformCoordinate(new Coordinate(mapPosition.X, mapPosition.Y), mathTransform);
            if (lAdd)
            {
                double X, Y;
                if (_pDist.Count == 0)
                {
                    NETGeographicLib.UTMUPS.Forward(geom.Y, geom.X, out nZone, out lNorth, out X, out Y, -1, true);
                    _pDist.Add(mapPosition);
                    _UTMDist.Add(new Coordinate(X, Y));
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
                    catch { }
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
            if (_pDist.Count > 1) refreshDisplay();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            if (_pDist.Count > 2)
                cpArea.MB.DefineLinearGeometry(_pDist);
            bt_New_Click(null, null);
        }

        private void bt_New_Click(object sender, EventArgs e)
        {
            ClearValues();
            cpArea.Clear();
        }

        private string FormattedAreaValue(double cV)
        {
            double mF = 1;
            string suff = " m";
            switch (cmbUnit.SelectedIndex)
            {
                case 0:
                    mF = 1;
                    suff = " m²";
                    break;
                case 1:
                    mF = Math.Pow(.001,2);
                    suff = " Km²";
                    break;
                case 2:
                    mF = Math.Pow(1 / 3.2808398950134,2);
                    suff = " ft²";
                    break;
                case 3:
                    mF = Math.Pow(1.09361329835,2);
                    suff = " yd²";
                    break;
                case 4:
                    mF = Math.Pow(0.00062137119224,2);
                    suff = " mi²";
                    break;
                case 5:
                    mF = 1 / 10000;
                    suff = " ha";
                    break;
                case 6:
                    mF = 1 / 4046.87261063789;
                    suff = " acres";
                    break;
                case 7:
                    int PAcre = (int)(cV / 4046.87261063789 - 0.5);
                    double PGunta = (cV / 4046.87261063789 - PAcre) * 40;
                    return PAcre + " acre " + PGunta.ToString("0.00##") + " gunta"; 
                case 8:
                    mF = Math.Pow(100,2);
                    suff = "cm²";
                    break;
                case 9:
                    mF = Math.Pow(1000,2);
                    suff = "mm²";
                    break;
                case 10:
                    mF = Math.Pow(0.0005399568035, 2);
                    suff = "NM²";
                    break;
            }
            double fVal = cV * mF;
            return fVal.ToString("0.0###") + suff;
        }

        private void refreshDisplay()
        {
            txtArea.Text = FormattedAreaValue(TotalArea());
        }

        private double TotalArea()
        {
            if (_UTMDist.Count < 3) return 0;
            int i, j;
            double aTot=0;
            i = _UTMDist.Count - 1;
            for (j=0;i<_UTMDist.Count;i++)
            {
                double aI = _UTMDist[i].X * _UTMDist[j].Y - _UTMDist[j].X * _UTMDist[i].Y;
                aTot += aI;
                j = i;
            }
            return Math.Abs(aTot)/2;
        }
        /// <summary>
        /// Clear Values
        /// </summary>
        public void ClearValues()
        {
            btAdd.Enabled = false;
            _pDist.Clear();
            _UTMDist.Clear();
            txtArea.Text = "";
        }
        /// <summary>
        /// Add options
        /// </summary>
        public void SetForAdd()
        {
            if (_pDist.Count > 3)
            {
                _pDist.RemoveAt(_pDist.Count - 1);
                _UTMDist.RemoveAt(_UTMDist.Count - 1);
                refreshDisplay();
                btAdd.Enabled = true;
            }
            else
                bt_New_Click(null, null);
        }

        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDisplay();
        }
    }
}
