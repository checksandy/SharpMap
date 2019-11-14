using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;

namespace SharpMap.Forms.Tools
{
    public partial class frm_Distance : Form
    {
        /// <summary>
        /// Window to display Distance
        /// </summary>
        public frm_Distance()
        {
            InitializeComponent();
            cmbUnit.Items.Add("Meters");
            cmbUnit.Items.Add("Kilometers");
            cmbUnit.Items.Add("Feets");
            cmbUnit.Items.Add("Yards");
            cmbUnit.Items.Add("Miles");
            cmbUnit.Items.Add("Nautical Miles");
            cmbUnit.Items.Add("Centimeters");
            cmbUnit.Items.Add("Millimeters");
            cmbUnit.SelectedIndex = 0;
        }
        private List<Coordinate> _pDist = new List<Coordinate>();
        private List<double> segDist=new List<double>();
        public void UpdateCoordinate(Coordinate mapPosition, bool lAdd)
        {
            if (lAdd) _pDist.Add(mapPosition);
            else _pDist[_pDist.Count - 1] = mapPosition;
            if (_pDist.Count>1) refreshDisplay();
        }

        private void refreshDisplay()
        {
            double dist = Math.Sqrt(Math.Pow((_pDist[_pDist.Count - 2].X - _pDist[_pDist.Count - 1].X), 2) + Math.Pow((_pDist[_pDist.Count - 2].Y - _pDist[_pDist.Count - 1].Y), 2));
            if (LV.Items.Count<_pDist.Count-1)
            {
                segDist.Add(dist);
                ListViewItem curDValue=new ListViewItem();
                switch (cmbUnit.SelectedIndex)
                {
                    case 0:
                        curDValue.Text = dist.ToString("0.0###") + " m";
                        txtDist.Text = TotalDistance().ToString("0.0###") + " m";
                        break;
                    case 1:
                        curDValue.Text = dist.ToString("0.0###") + " m";
                        break;
                    case 2:
                        curDValue.Text = dist.ToString("0.0###") + " m";
                        break;
                }
                
                LV.Items.Add(curDValue);
                LV.EnsureVisible(LV.Items.Count - 1);
            }
            else
            {
                segDist[segDist.Count-1]=dist;
                switch (cmbUnit.SelectedIndex)
                {
                    case 0:
                        LV.Items[LV.Items.Count - 1].Text= dist.ToString("0.0###") + " m";
                        txtDist.Text = TotalDistance().ToString("0.0###") + " m";
                        break;
                    case 1:
                        LV.Items[LV.Items.Count - 1].Text = dist.ToString("0.0###") + " m";
                        break;
                    case 2:
                        LV.Items[LV.Items.Count - 1].Text = dist.ToString("0.0###") + " m";
                        break;
                }
            }

            double TotalDistance()
            {
                double t_dist=0;
                for (int i=0;i<segDist.Count;i++)
                {
                    t_dist += segDist[i];
                }
                return t_dist;
            }
        }
    }
}
