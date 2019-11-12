// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using GeoAPI.Geometries;
using SharpMap.Rendering.Symbolizer;
using Common.Logging;
using System.ComponentModel;

#if NETSTANDARD2_0
using SmColor = SharpMap.Drawing.Color;
using Color = System.Drawing.Color;
using KnownColor = SharpMap.Drawing.KnownColor;
#else
using SmColor = System.Drawing.Color;
#endif
namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering vector data
    /// </summary>
    [Serializable]
    public class VectorStyle : Style, ICloneable
    {
        private static readonly Random _rnd = new Random();

        static ILog logger = LogManager.GetLogger(typeof(VectorStyle));
        /// <summary>
        /// Default Symbol
        /// </summary>
        public static readonly Image DefaultSymbol;

        /// <summary>
        /// Static constructor
        /// </summary>
        static VectorStyle()
        {
            var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
            if (rs != null)
                DefaultSymbol = Image.FromStream(rs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VectorStyle Clone()
        {
            VectorStyle vs;
            lock (this)
            {
                try
                {
                    vs = (VectorStyle)MemberwiseClone();// new VectorStyle();

                    if (_fillStyle != null)
                        vs._fillStyle = _fillStyle.Clone() as Brush;

                    if (_lineStyle != null)
                        vs._lineStyle = _lineStyle.Clone() as Pen;

                    if (_outlineStyle != null)
                        vs._outlineStyle = _outlineStyle.Clone() as Pen;

                    if (_pointBrush != null)
                        vs._pointBrush = _pointBrush.Clone() as Brush;

                    vs._symbol = (_symbol != null ? _symbol.Clone() as Image : null);
                    vs._symbolRotation = _symbolRotation;
                    vs._symbolScale = _symbolScale;
                    vs.PointSymbolizer = PointSymbolizer;
                    vs.LineSymbolizer = LineSymbolizer;
                    vs.PolygonSymbolizer = PolygonSymbolizer;
                }
                catch (Exception ee)
                {
                    logger.Error("Exception while creating cloned style", ee);
                    /* if we got an exception, set the style to null and return since we don't know what we got...*/
                    vs = null;
                }
            }
            return vs;
        }
        
        object ICloneable.Clone()
        {
            return Clone();
        }

#region Privates

        private Brush _fillStyle;
        private Pen _lineStyle;
        private bool _outline;
        private Pen _outlineStyle;
        private Image _symbol;
        private float _lineOffset;

#endregion

        /// <summary>
        /// Initializes a new VectorStyle and sets the default values
        /// </summary>
        /// <remarks>
        /// Default style values when initialized:<br/>
        /// *LineStyle: 1px solid black<br/>
        /// *FillStyle: Solid black<br/>
        /// *Outline: No Outline
        /// *Symbol: null-reference
        /// </remarks>
        public VectorStyle()
        {
            _transparency = _rnd.Next(67, 256);
            _color = CreateRandomKnownColor(255);
            _linewidth = 1f;
            _outlinewidth = 1f;
            
            Outline = new Pen(_color, _outlinewidth);
            //Line = new Pen(_color, 1);
            Fill = new SolidBrush (_color);
            EnableOutline = false;
            SymbolScale = 1f;
            PointColor = new SolidBrush(_color);
            PointSize = 10f;
            LineOffset = 0;

            _bgcolor = CreateRandomKnownColor(255);
            _polgonfilltype = 0;
            _hatchstyle = HatchStyle.Horizontal;
            _gradientp1 = new Point(0, 0);
            _gradientp2 = new Point(0, 10);
        }

#region Properties
        public enum PolyFillStyle
        {
            Solid=0,
            Hatch=1,
            Gradient=2
        }
        private PointF _symbolOffset;
        private float _symbolRotation;
        private float _symbolScale;
        private bool _usesymbol;

        private float _pointSize;
        private Brush _pointBrush;

        private int _transparency;
        private Color _outlinecolor;
        private float _outlinewidth;
        private Color _color;
        private float _linewidth;

        private PolyFillStyle _polgonfilltype;
        private Color _bgcolor;
        private HatchStyle _hatchstyle;
        private Point _gradientp1;
        private Point _gradientp2;

        /// <summary>
        /// BG Fill Color
        /// </summary>
        [Category("Polygon"),DisplayName("Background Color"),Description("Specify Background Color for Polygon"),Browsable(true)]
        public Color BGColor
        {
            get {return _bgcolor; }
            set { _bgcolor=value; }
        }

        /// <summary>
        /// Fill Type
        /// </summary>
        [Category("Polygon"), DisplayName("Fill Type"), Description("Specify Fill Type for Polygon"), Browsable(true)]
        public PolyFillStyle FillType
        {
            get { return _polgonfilltype; }
            set { _polgonfilltype = value; }
        }

        /// <summary>
        /// Hatch Style
        /// </summary>
        [Category("Polygon"), DisplayName("Hatch Style"), Description("Specify Hatch Style for Polygon"), Browsable(true)]
        public HatchStyle HStyle
        {
            get { return _hatchstyle; }
            set { _hatchstyle = value; }
        }


        /// <summary>
        /// Gradient Point 1
        /// </summary>
        [Category("Polygon"), DisplayName("Gradient Point 1"), Description("Gradient Point 1 Location"), Browsable(true)]
        public Point GradientPoint1
        {
            get { return _gradientp1; }
            set { _gradientp1 = value; }
        }

        /// <summary>
        /// Gradient Point 2
        /// </summary>
        [Category("Polygon"), DisplayName("Gradient Point 2"), Description("Gradient Point 2 Location"), Browsable(true)]
        public Point GradientPoint2
        {
            get { return _gradientp2; }
            set { _gradientp2 = value; }
        }

        /// <summary>
        /// Linestyle for line geometries
        /// </summary>
        [Browsable(false)]
        public Pen Line
        {
            get { return new Pen(Color.FromArgb(_transparency,_color.R,_color.G,_color.B), _linewidth); }
            set { _lineStyle = value; }
        }

        /// <summary>
        /// Color for Outline
        /// </summary>
        [Category("General")]
        [DisplayName("Color")]
        [Description("Specify Fore Color to use")]
        [Browsable(true)]
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Color for Outline
        /// </summary>
        [Category("General")]
        [DisplayName("Outline Color")]
        [Description("Specify Color to use for outline")]
        [Browsable(true)]
        public Color OutlineColor
        {
            get { return _outlinecolor; }
            set { _outlinecolor = value; }
        }

        /// <summary>
        /// Tranparency
        /// </summary>
        [Category("General")]
        [DisplayName("Tranparency")]
        [Description("Specify Transparency from 0 to 255")]
        [Browsable(true)]
        public int Transparency
        {
            get { return _transparency; }
            set
            {
                if (value < 0) value = 0;
                if (value > 255) value = 255;
                _transparency = value;
            }
        }

        /// <summary>
        /// Outline Width
        /// </summary>
        [Category("General")]
        [DisplayName("Outline Width")]
        [Description("Specify Width for outline")]
        [Browsable(true)]
        public float OutlineWidth
        {
            get { return _outlinewidth; }
            set { _outlinewidth = value; }
        }

        /// <summary>
        /// Outline style for line and polygon geometries
        /// </summary>
        [Browsable(false)]
        public Pen Outline
        {
            get { return new Pen(_outlinecolor,_outlinewidth); }
            set { _outlineStyle = value; }
        }

        /// <summary>
        /// Specified whether the objects are rendered with or without outlining
        /// </summary>
        [Category("General")]
        [DisplayName("Enable Outline")]
        [Description("Use for line and polygon objects to display outline")]
        public bool EnableOutline
        {
            get { return _outline; }
            set { _outline = value; }
        }

        /// <summary>
        /// Specified whether the objects are rendered with or without outlining
        /// </summary>
        [Category("Line")]
        [DisplayName("Width")]
        [Description("Use for line Width")]
        public float LineWidth
        {
            get { return _linewidth; }
            set { _linewidth = value; }
        }


        /// <summary>
        /// Fillstyle for Polygon geometries
        /// </summary>
        [Browsable(false)]
        public Brush Fill
        {
            get
            {
                switch (FillType)
                {
                    case PolyFillStyle.Solid:
                        _fillStyle = new SolidBrush(Color.FromArgb(_transparency, _color.R, _color.G, _color.B));
                        break;
                    case PolyFillStyle.Hatch:
                        _fillStyle = new HatchBrush(_hatchstyle,_color, Color.FromArgb(_transparency, _bgcolor.R, _bgcolor.G, _bgcolor.B));
                        break;
                    case PolyFillStyle.Gradient:
                        Fill = new LinearGradientBrush(_gradientp1, _gradientp2, Color.FromArgb(_transparency, _color.R, _color.G, _color.B), Color.FromArgb(_transparency, _bgcolor.R, _bgcolor.G, _bgcolor.B));
                        break;
                }
                return _fillStyle;
            }
            set { _fillStyle = value; }
        }

        /// <summary>
        /// Fillstyle for Point geometries (will be used if no Symbol is set)
        /// </summary>
        [Browsable(false)]
        public Brush PointColor
        {
            get { return new SolidBrush(Color.FromArgb(_transparency,_color.R,_color.G,_color.B)); }
            set { _pointBrush = value; }
        }

        /// <summary>
        /// Size for Point geometries (if drawn with PointColor), will not have affect for Points drawn with Symbol
        /// </summary>
        /// 
        [Category("Point")]
        [DisplayName("Point Size")]
        [Description("Specify Point Size")]
        public float PointSize
        {
            get { return _pointSize; }
            set { _pointSize= value; }
        }

        /// <summary>
        /// Symbol used for rendering points
        /// </summary>
        [Category("Point")]
        [DisplayName("Symbol Name")]
        [Description("Specify Symbol Name")]
        public Image Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// Scale of the symbol (defaults to 1)
        /// </summary>
        /// <remarks>
        /// Setting the symbolscale to '2.0' doubles the size of the symbol, where a scale of 0.5 makes the scale half the size of the original image
        /// </remarks>
        [Category("Point")]
        [DisplayName("Symbol Scale")]
        [Description("Specify Symbol Scale")]
        public float SymbolScale
        {
            get { return _symbolScale; }
            set { _symbolScale = value; }
        }

        /// <summary>
        /// Gets or sets the offset in pixels of the symbol.
        /// </summary>
        /// <remarks>
        /// The symbol offset is scaled with the <see cref="SymbolScale"/> property and refers to the offset af <see cref="SymbolScale"/>=1.0.
        /// </remarks>
        [Browsable(false)]
        public PointF SymbolOffset
        {
            get { return _symbolOffset; }
            set { _symbolOffset = value; }
        }

        /// <summary>
        /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        [Category("Point")]
        [DisplayName("Symbol Rotation")]
        [Description("Specify Symbol Rotation")]
        public float SymbolRotation
        {
            get { return _symbolRotation; }
            set { _symbolRotation = value; }
        }


        /// <summary>
        /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        [Category("Point")]
        [DisplayName("Use Symbol")]
        [Description("Use Symbol")]
        public bool UseSymbol
        {
            get { return _usesymbol; }
            set { _usesymbol = value; }
        }

        /// <summary>
        /// Gets or sets the offset (in pixel units) by which line will be offset from its original posision (perpendicular).
        /// </summary>
        /// <remarks>
        /// A positive value offsets the line to the right
        /// A negative value offsets to the left
        /// </remarks>
        [Category("Line"),DisplayName("Line Offset"),Description("Specify Line Offset")]
        public float LineOffset
        {
            get { return _lineOffset; }
            set { _lineOffset = value; }
        }

        /// <summary>
        /// Gets or sets the symbolizer for puntal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="IPuntal"/> related style settings</remarks>
        [Browsable(false)]
        public IPointSymbolizer PointSymbolizer { get; set; }

        /// <summary>
        /// Gets or sets the symbolizer for lineal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="ILineal"/> related style settings</remarks>
        [Browsable(false)]
        public ILineSymbolizer LineSymbolizer { get; set; }

        /// <summary>
        /// Gets or sets the symbolizer for polygonal geometries
        /// </summary>
        /// <remarks>Setting this property will lead to ignorance towards all <see cref="IPolygonal"/> related style settings</remarks>
        [Browsable(false)]
        public IPolygonSymbolizer PolygonSymbolizer { get; set; }

#endregion

        /// <summary>
        /// Releases managed resources
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (IsDisposed)
                return;

            if (_fillStyle != null)
            {
                _fillStyle.Dispose();
                _fillStyle = null;
            }

            if (_lineStyle != null)
            {
                _lineStyle.Dispose();
                _lineStyle = null;
            }


            if (_outlineStyle != null)
            {
                _outlineStyle.Dispose();
                _outlineStyle = null;
            }

            if (_pointBrush != null)
            {
                _pointBrush.Dispose();
                _pointBrush = null;
            }


            if (_symbol != null)
            {
                _symbol.Dispose();
                _symbol = null;
            }


            base.ReleaseManagedResources();
        }

        /// <summary>
        /// Utility function to create a random style
        /// </summary>
        /// <returns>A vector style</returns>
        public static VectorStyle CreateRandomStyle()
        {
            var res = new VectorStyle();
            RandomizePuntalStyle(res);
            RandomizeLinealStyle(res);
            RandomizePolygonalStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomPuntalStyle()
        {
            var res = new VectorStyle();
            ClearLinealStyle(res);
            ClearPolygonalStyle(res);
            RandomizePuntalStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomLinealStyle()
        {
            var res = new VectorStyle();
            ClearPuntalStyle(res);
            ClearPolygonalStyle(res);
            RandomizeLinealStyle(res);
            return res;
        }

        /// <summary>
        /// Factory method to create a random puntal style
        /// </summary>
        /// <returns>A puntal vector style</returns>
        public static VectorStyle CreateRandomPolygonalStyle()
        {
            var res = new VectorStyle();
            ClearPuntalStyle(res);
            ClearLinealStyle(res);
            RandomizePolygonalStyle(res);
            return res;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearPuntalStyle(VectorStyle style)
        {
            style.PointColor = Brushes.Transparent;
            style.PointSize = 0f;
            style.Symbol = null;
            style.PointSymbolizer = null;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearLinealStyle(VectorStyle style)
        {
            style.EnableOutline = false;
            style.Line = Pens.Transparent;
            style.Outline = Pens.Transparent;
        }

        /// <summary>
        /// Utility function to modify <paramref name="style"/> in order to prevent drawing of any puntal components
        /// </summary>
        /// <param name="style">The style to modify</param>
        private static void ClearPolygonalStyle(VectorStyle style)
        {
            style.EnableOutline = false;
            style.Line = Pens.Transparent;
            style.Outline = Pens.Transparent;
            style.Fill = Brushes.Transparent;
        }

        /// <summary>
        /// Utility function to randomize puntal settings
        /// </summary>
        /// <param name="res">The style to randomize</param>
        private static void RandomizePuntalStyle(VectorStyle res)
        {
            switch (_rnd.Next(2))
            {
                case 0:
                    res.Symbol = DefaultSymbol;
                    res.SymbolScale = 0.01f * _rnd.Next(80, 200);
                    break;
                case 1:
                    res.Symbol = null;
                    res.PointColor = new SolidBrush(CreateRandomKnownColor(_rnd.Next(67, 256)));
                    res.PointSize = 0.1f * _rnd.Next(5, 20);
                    break;
            }
        }

        /// <summary>
        /// Utility function to randomize lineal settings
        /// </summary>
        /// <param name="res">The style to randomize</param>
        private static void RandomizeLinealStyle(VectorStyle res)
        {
            res.Line = new Pen(CreateRandomKnownColor(_rnd.Next(67, 256)), _rnd.Next(1, 3));
            res.EnableOutline = _rnd.Next(0, 2) == 1;
            if (res.EnableOutline)
                res.Outline = new Pen(CreateRandomKnownColor(_rnd.Next(67, 256)), _rnd.Next((int)res.Line.Width, 5));
        }

        /// <summary>
        /// Utility function to randomize polygonal settings
        /// </summary>
        /// <param name="res"></param>
        private static void RandomizePolygonalStyle(VectorStyle res)
        {
            switch (_rnd.Next(3))
            {
                case 0:
                    res.Fill = new SolidBrush(CreateRandomKnownColor(_rnd.Next(67, 256)));
                    break;
                case 1:
                    res.Fill = new HatchBrush((HatchStyle)_rnd.Next(0, 53),
                        CreateRandomKnownColor(), CreateRandomKnownColor(_rnd.Next(67, 256)));
                    break;
                case 2:
                    var alpha = _rnd.Next(67, 256);
                    res.Fill = new LinearGradientBrush(new Point(0, 0), new Point(_rnd.Next(5, 10), _rnd.Next(5, 10)),
                        CreateRandomKnownColor(alpha), CreateRandomKnownColor(alpha));
                    break;
            }
        }

        /// <summary>
        /// Factory method to create a random color from the <see cref="KnownColor"/>s enumeration
        /// </summary>
        /// <param name="alpha">An optional alpha value.</param>
        /// <returns></returns>
        public static Color CreateRandomKnownColor(int alpha = 255)
        {
            var kc = (KnownColor)_rnd.Next(28, 168);
            return alpha == 255 
                ? SmColor.FromKnownColor(kc) 
                : Color.FromArgb(alpha, SmColor.FromKnownColor(kc));
        }
    }
}
