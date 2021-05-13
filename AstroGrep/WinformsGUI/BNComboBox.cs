using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System;

namespace AstroGrep
{
    public delegate void BNDroppedDownEventHandler(object sender, EventArgs e);
    public delegate void BNDrawItemEventHandler(object sender, DrawItemEventArgs e);
    public delegate void BNMeasureItemEventHandler(object sender, MeasureItemEventArgs e);

    public class HackyPanel : Panel
    {
        public TextBox _textBox;
        
        // TODO: Should be an Adorner??
        public HackyPanel() {
            SetStyle(ControlStyles.UserPaint, true);

            _textBox = new TextBox();
            _textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _textBox.Location = new System.Drawing.Point(0, 0);
            _textBox.Size = new System.Drawing.Size(60, 14);
            _textBox.TabIndex = 0;
            _textBox.WordWrap = false;
            _textBox.Margin = new Padding(0);
            _textBox.TextAlign = HorizontalAlignment.Left;
            _textBox.BackColor = Color.Orange;
            _textBox.AutoSize = false;  // TODO: Should auto-adjust Panel on resize, its not happening though whoops?
            //_textBox.Visible = false;
            // _textBox.PasswordChar = ' ';
            // this.Controls.Add(_textBox);

            // Invisible un-interactable panel - Hides TextBox text
            Bounds = _textBox.Bounds;
            Enabled = false;
            BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(15)))), ((int)(((byte)(6)))));

            //_textBox.Resize += new EventHandler(onResize);
            _textBox.TextChanged += new EventHandler(onTextChanged);
        }

        void onResize(object sender, EventArgs e)
        {
            BNComboBox c = this.Parent as BNComboBox;
            c.AdjustControls();
        }

        public void onTextChanged(object sender, EventArgs e)
        {
            //OnTextChanged(e);
            Invalidate(true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // TODO: It's drawing text @#$4 blurred for some damn reason.
            // If you disable aliasing it literally misses parts of the letters in the font??

            Rectangle rectText = Bounds;
            rectText.Location = new Point(0, 0);
            //rectText.Offset(-3, -2);
            //rectText.Size = rectOuter.Size;

            var path = new GraphicsPath();
            Font f = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            var fontObject = new FontObject(_textBox.Text, f);
            fontObject.FillColor = ForeColor;
            // fontObject.SizeInPoints = 12;

            using (var format = new StringFormat(StringFormatFlags.NoWrap))
            {
                format.Alignment = StringAlignment.Near; //As selected
                format.LineAlignment = StringAlignment.Center; //As selected

                //path.AddString(fontObject.Text, fontObject.FontFamily, (int)fontObject.FontStyle, fontObject.SizeInEms, rectText, format);

                // rectText.Offset(0,-15);
                // path.AddString(fontObject.Text, new FontFamily("Arial"), (int)FontStyle.Regular, 14, rectText, format);

                //rectText.Offset(0, -15);

                e.Graphics.SmoothingMode = SmoothingMode.None;// SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                // The composition properties are useful when drawing on a composited surface
                // when we simply draw on a Control's surface, these are useless
                e.Graphics.CompositingMode = CompositingMode.SourceOver;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                if (fontObject.Outlined)
                {
                    e.Graphics.DrawPath(fontObject.Outline, path);
                }
                using (var brush = new SolidBrush(fontObject.FillColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                e.Graphics.DrawString(_textBox.Text, f, new SolidBrush(ForeColor), rectText, format);

                // TextRenderer.DrawText(e.Graphics, _textBox.Text, f, rectText, ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

               // e.Graphics.DrawRectangle(Pens.GreenYellow, rectText);
                //e.Graphics.DrawRectangle(Pens.OrangeRed, _textBox.Bounds);

            }
        }
    }

    public class FontObject
    {
        private float currentScreenDPI = 0.0F;
        private float m_SizeInPoints = 0.0F;
        private float m_SizeInPixels = 0.0F;
        public FontObject() 
            : this(string.Empty, FontFamily.GenericSansSerif, FontStyle.Regular, 18F) { }
        public FontObject(string text, Font font) 
            : this(text, font.FontFamily, font.Style, font.SizeInPoints) { }
        public FontObject(string text, FontFamily fontFamily, FontStyle fontStyle, float FontSize)
        {
            if (FontSize < 3) FontSize = 3;
            using (Graphics g = Graphics.FromHwndInternal(IntPtr.Zero)) {
                this.currentScreenDPI = g.DpiY; 
            }
            this.Text = text;
            this.FontFamily = fontFamily;
            this.SizeInPoints = FontSize;
            this.SizeInEms = GetEmSize();
            this.FillColor = Color.Black;
            this.Outline = new Pen(Color.Black, 1);
            this.Outlined = false;
        }

        public string Text { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontFamily FontFamily { get; set; }
        public Color FillColor { get; set; }
        public Pen Outline { get; set; }
        public bool Outlined { get; set; }
        public float SizeInPoints {
            get => this.m_SizeInPoints;
            set {  this.m_SizeInPoints = value;
                   this.m_SizeInPixels = (value * 72F) / this.currentScreenDPI;
                   this.SizeInEms = GetEmSize();
            }
        }
        public float SizeInPixels {
            get => this.m_SizeInPixels;
            set {  this.m_SizeInPixels = value;
                   this.m_SizeInPoints = (value * this.currentScreenDPI) / 72F;
                   this.SizeInEms = GetEmSize();
            }
        }

        public float SizeInEms { get; private set; }
        public PointF Location { get; set; }
        public RectangleF DrawingBox { get; set; }

        private float GetEmSize()
        {
            return (this.m_SizeInPoints * 
                   (this.FontFamily.GetCellAscent(this.FontStyle) +
                    this.FontFamily.GetCellDescent(this.FontStyle))) /
                    this.FontFamily.GetEmHeight(this.FontStyle);
        }
    }

    public class BNComboBox : ListControl
    {
        #region Variables

        private bool hovered = false;
        private bool pressed = false;
        private bool resize = false;

        private Color _backColor = Color.White;        // Button shading color
        private Color _color1 = Color.Gray;            // Outer Rim color
        private Color _color2 = Color.DimGray;         // Inner Rim color + separator
        //private Color _color3 = Color.Yellow;
        private Color _color4 = Color.Gold;            // Selection edge highlight color
        private BNRadius _radius = new BNRadius();

        private int _dropDownHeight = 200;
        private int _dropDownWidth = 0;
        private int _maxDropDownItems = 30;
        
        private int _selectedIndex = -1;

        private bool _isDroppedDown = false;

        private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDown;

        private Rectangle rectBtn = new Rectangle(0, 0, 1, 1);
        private Rectangle rectContent = new Rectangle(0, 0, 1, 1);

        private ToolStripControlHost _controlHost;
        private ListBox _listBox;
        private ToolStripDropDown _popupControl;
        private HackyPanel _hackBox;

        #endregion




        #region Delegates

        [Category("Behavior"), Description("Occurs when IsDroppedDown changed to True.")]
        public event BNDroppedDownEventHandler DroppedDown;

        [Category("Behavior"), Description("Occurs when the SelectedIndex property changes.")]
        public event EventHandler SelectedIndexChanged;

        [Category("Behavior"), Description("Occurs whenever a particular item/area needs to be painted.")]
        public event BNDrawItemEventHandler DrawItem;

        [Category("Behavior"), Description("Occurs whenever a particular item's height needs to be calculated.")]
        public event BNMeasureItemEventHandler MeasureItem;

        #endregion



        
        #region Properties

        public Color Color1
        {
            get { return _color1; }
            set { _color1 = value; Invalidate(true); }
        }

        public Color Color2
        {
            get { return _color2; }
            set { _color2 = value; Invalidate(true); }
        }
        
        //public Color Color3
        //{
        //    get { return _color3; }
        //    set { _color3 = value; Invalidate(true); }
       // }

        public Color Color4
        {
            get { return _color4; }
            set { _color4 = value; Invalidate(true); }
        }

        public int DropDownHeight
        {
            get { return _dropDownHeight; }
            set { _dropDownHeight = value; }
        }

        public ListBox.ObjectCollection Items
        {
            get { return _listBox.Items; }
        }

        public int DropDownWidth
        {
            get { return _dropDownWidth; }
            set { _dropDownWidth = value; }
        }

        public int MaxDropDownItems
        {
            get { return _maxDropDownItems; }
            set { _maxDropDownItems = value; }
        }

        public new object DataSource
        {
            get { return base.DataSource; }
            set 
            { 
                _listBox.DataSource = value;
                base.DataSource = value;
                OnDataSourceChanged(System.EventArgs.Empty);
            }
        }

        public bool Soreted
        {
            get
            {
                return _listBox.Sorted;
            }
            set
            {
                _listBox.Sorted = value;
            }
        }

        [Category("Behavior"), Description("Indicates whether the code or the OS will handle the drawing of elements in the list.")]
        public DrawMode DrawMode
        {
            get { return _listBox.DrawMode; }
            set
            {
                _listBox.DrawMode = value;
            }
        }
        
        public ComboBoxStyle DropDownStyle
        {
            get { return _dropDownStyle; }
            set 
            { 
                _dropDownStyle = value; 
            
                if (_dropDownStyle == ComboBoxStyle.DropDownList)
                {
                    _hackBox._textBox.Visible = false;
                }
                else
                {
                    _hackBox._textBox.Visible = true;
                }
                Invalidate(true);
            }
        }

        public new Color BackColor
        {
            get { return _backColor; }
            set 
            { 
                this._backColor = value;
                //_hackBox._textBox.BackColor = value;
                Invalidate(true);
            }
        }

        public bool IsDroppedDown
        {
            get { return _isDroppedDown; }
            set 
            {
                if (_isDroppedDown == true && value == false )
                {
                    if (_popupControl.IsDropDown)
                    {
                        _popupControl.Close();
                    }
                }

                _isDroppedDown = value;

                if (_isDroppedDown)
                {
                    //_listBox.Refresh();

                    // TODO: Need to click drop down twice before it updates with full length properly
                    // For some reason doesn't go all the way the first time?? Weird....
                    int w = _dropDownWidth;
                    for(int i = 0; i < _listBox.Items.Count; i++)
                    {
                        int CurrentItemWidth = (int)Math.Ceiling(this.CreateGraphics().MeasureString(_listBox.Items[i].ToString(), _listBox.Font).Width) + 2;
                        if (w < CurrentItemWidth)
                            w = CurrentItemWidth;
                    }
                    _controlHost.Control.Width = w;

                    if (_listBox.Items.Count > 0) 
                    {
                        int itemCount = Math.Min(_listBox.Items.Count, _maxDropDownItems);
                        _listBox.ItemHeight = 16;// _listBox.GetItemHeight(0);
                        _listBox.Height = _listBox.ItemHeight * (itemCount+1);
                    }
                    else
                    {
                        _listBox.Height = 15;
                    }

                    _popupControl.Show(this, CalculateDropPosition(), ToolStripDropDownDirection.BelowRight);
                }

                Invalidate();
                if (_isDroppedDown)
                    OnDroppedDown(this, EventArgs.Empty);
            }
        }

        public BNRadius Radius
        {
            get { return _radius; }
        }

        #endregion




        #region Constructor
        public BNComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);

            base.BackColor = Color.Transparent;
            _radius.BottomLeft = 2;
            _radius.BottomRight = 2;
            _radius.TopLeft = 2;
            _radius.TopRight = 6;

            this.Height = 21;
            this.Width = 95;

            this.SuspendLayout();
            _hackBox = new HackyPanel();

            this.Controls.Add(_hackBox);
            this.Controls.Add(_hackBox._textBox);
            this.Controls.SetChildIndex(_hackBox, 0);

            this.ResumeLayout(false);

            AdjustControls();

            _listBox = new ListBox();
            _listBox.IntegralHeight = true;
            _listBox.BorderStyle = BorderStyle.FixedSingle;
            _listBox.SelectionMode = SelectionMode.One;
            _listBox.BindingContext = new BindingContext();
            _listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;

            _controlHost = new ToolStripControlHost(_listBox);
            _controlHost.Padding = new Padding(0);
            _controlHost.Margin = new Padding(0);
            _controlHost.AutoSize = false;

            _popupControl = new ToolStripDropDown();
            _popupControl.Padding = new Padding(0);
            _popupControl.Margin = new Padding(0);
            _popupControl.AutoSize = true;
            _popupControl.DropShadowEnabled = false;
            _popupControl.Items.Add(_controlHost);

            _dropDownWidth = this.Width;

            _listBox.MeasureItem += new MeasureItemEventHandler(_listBox_MeasureItem);
            _listBox.DrawItem += new DrawItemEventHandler(_listBox_DrawItem);
            _listBox.MouseClick += new MouseEventHandler(_listBox_MouseClick);
            _listBox.MouseMove += new MouseEventHandler(_listBox_MouseMove);

            _popupControl.Closed += new ToolStripDropDownClosedEventHandler(_popupControl_Closed);


            BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(15)))), ((int)(((byte)(6)))));
            ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
        }

        

        #endregion




        #region Overrides

        protected override void OnDataSourceChanged(EventArgs e)
        {
            this.SelectedIndex = 0;
            base.OnDataSourceChanged(e);
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            _listBox.DisplayMember = this.DisplayMember;
            this.SelectedIndex = this.SelectedIndex;
            base.OnDisplayMemberChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate(true);
            base.OnEnabledChanged(e);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            _hackBox._textBox.ForeColor = this.ForeColor;
            base.OnForeColorChanged(e);
        }

        protected override void OnFormatInfoChanged(EventArgs e)
        {
            _listBox.FormatInfo = this.FormatInfo;
            base.OnFormatInfoChanged(e);
        }

        protected override void OnFormatStringChanged(EventArgs e)
        {
            _listBox.FormatString = this.FormatString;
            base.OnFormatStringChanged(e);
        }

        protected override void OnFormattingEnabledChanged(EventArgs e)
        {
            _listBox.FormattingEnabled = this.FormattingEnabled;
            base.OnFormattingEnabledChanged(e);
        }

        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                resize = true;
                _hackBox._textBox.Font = value;
                base.Font = value;
                Invalidate(true);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
            e.Control.MouseEnter += new EventHandler(Control_MouseEnter);
            e.Control.MouseLeave += new EventHandler(Control_MouseLeave);
            e.Control.GotFocus += new EventHandler(Control_GotFocus);
            e.Control.LostFocus += new EventHandler(Control_LostFocus);
            base.OnControlAdded(e);
        }        

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            this.Invalidate(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!this.RectangleToScreen(this.ClientRectangle).Contains(MousePosition))
            {
                hovered = false;
                Invalidate(true);
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _hackBox._textBox.Focus();
            if ((this.RectangleToScreen(rectBtn).Contains(MousePosition) || (DropDownStyle == ComboBoxStyle.DropDownList)))
            {
                pressed = true;
                this.Invalidate(true);
                if (this.IsDroppedDown) 
                {
                    this.IsDroppedDown = false;
                }
                this.IsDroppedDown = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            pressed = false;

            if (! this.RectangleToScreen(this.ClientRectangle).Contains(MousePosition) )
                hovered = false;
            else
                hovered = true;

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
                this.SelectedIndex = this.SelectedIndex + 1;
            else if (e.Delta > 0)
            {
                if (this.SelectedIndex > 0)
                    this.SelectedIndex = this.SelectedIndex - 1;
            }

            base.OnMouseWheel(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate(true);
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!this.ContainsFocus)
            {
                Invalidate();
            }

            base.OnLostFocus(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if(SelectedIndexChanged!=null)
                SelectedIndexChanged(this, e);

            base.OnSelectedIndexChanged(e);
        }

        protected override void OnValueMemberChanged(EventArgs e)
        {
            _listBox.ValueMember = this.ValueMember;
            this.SelectedIndex = this.SelectedIndex;
            base.OnValueMemberChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (resize)
            {

                resize = false;
                AdjustControls();

                Invalidate(true);
            }
            else
                Invalidate(true);

            //if (DesignMode)
                _dropDownWidth = this.Width;
        }

        public override string Text
        {
            get
            {
                return _hackBox._textBox.Text;
            }
            set
            {
                _hackBox._textBox.Text = value;
                _hackBox.onTextChanged(this, EventArgs.Empty);
                //base.Text = _hackBox._textBox.Text;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //button border
            Rectangle rectButton = rectBtn;
            rectButton.X += 1;
            rectButton.Y += 1;
            rectButton.Width -= 3;
            rectButton.Height -= 3;
            GraphicsPath pathBtnBorder = CreateRoundRectangle(rectButton, 0, Radius.TopRight, Radius.BottomRight, 0);

            //outer border
            Rectangle rectOuter = rectContent;
            rectOuter.Width -= 1;
            rectOuter.Height -= 1;
            GraphicsPath pathOuterBorder = CreateRoundRectangle(rectOuter, Radius.TopLeft, Radius.TopRight, Radius.BottomRight,
                Radius.BottomLeft);

            //inner/content border
            Rectangle rectInner = rectContent;
            rectInner.X += 1;
            rectInner.Y += 1;
            rectInner.Width -= 3;
            rectInner.Height -= 3;
            GraphicsPath pathInnerBorder = CreateRoundRectangle(rectInner, Radius.TopLeft, Radius.TopRight, Radius.BottomRight,
                Radius.BottomLeft);

            //e.Graphics.DrawRectangle(new Pen(Color.Red), rectOuter);
           // e.Graphics.DrawRectangle(new Pen(Color.Blue), rectInner);

            
            //brushes and pens
            Brush brInnerBrush = new LinearGradientBrush(
                new Rectangle(rectInner.X,rectInner.Y,rectInner.Width,rectInner.Height+1), 
                (hovered || IsDroppedDown || ContainsFocus)?Color4:Color2, Color.Transparent,
                LinearGradientMode.Vertical);
            Brush brBackground;
            if (this.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                brBackground = new LinearGradientBrush(pathInnerBorder.GetBounds(), 
                    Color.FromArgb(IsDroppedDown ? 100 : 255, Color.White), 
                    Color.FromArgb(IsDroppedDown?255:100, BackColor),
                    LinearGradientMode.Vertical);
            }
            else
            {
                brBackground = new SolidBrush(BackColor);
            }
            Pen penOuterBorder = new Pen(Color1, 0);
            Pen penInnerBorder = new Pen(brInnerBrush, 0);
            LinearGradientBrush brButtonLeft = new LinearGradientBrush(rectBtn, Color1, Color2, LinearGradientMode.Vertical);
            ColorBlend blend = new ColorBlend();
            blend.Colors = new Color[] { Color.Transparent, Color2, Color.Transparent };
            blend.Positions = new float[] { 0.0f, 0.5f, 1.0f};
            brButtonLeft.InterpolationColors = blend;
            Pen penLeftButton = new Pen(brButtonLeft, 0);
            Brush brButton = new LinearGradientBrush(pathBtnBorder.GetBounds(),
                Color.FromArgb(100, IsDroppedDown? Color2:Color.White),
                    Color.FromArgb(100, IsDroppedDown ? Color.White : Color2),
                    LinearGradientMode.Vertical);

            //draw
            e.Graphics.FillPath(brBackground, pathInnerBorder);
            if (DropDownStyle != ComboBoxStyle.DropDownList)
            {
                e.Graphics.FillPath(brButton, pathBtnBorder);
            }
            e.Graphics.DrawPath(penOuterBorder, pathOuterBorder);
            e.Graphics.DrawPath(penInnerBorder, pathInnerBorder);
           
            e.Graphics.DrawLine(penLeftButton, rectBtn.Left + 1, rectInner.Top+1, rectBtn.Left + 1, rectInner.Bottom -1);

            //Glimph
            Rectangle rectGlimph = rectButton;
            rectButton.Width -= 4;
            e.Graphics.TranslateTransform(rectGlimph.Left + rectGlimph.Width / 2.0f, rectGlimph.Top + rectGlimph.Height / 2.0f);
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[3];
            points[0] = new PointF(-6 / 2.0f, -3 / 2.0f);
            points[1] = new PointF(6 / 2.0f, -3 / 2.0f);
            points[2] = new PointF(0, 6 / 2.0f);
            path.AddLine(points[0], points[1]);
            path.AddLine(points[1], points[2]);
            path.CloseFigure();
            e.Graphics.RotateTransform(0);

            // TODO: Modify the 'arrow' color
            SolidBrush br = new SolidBrush(Enabled?Color.Gray:Color.Gainsboro);
            e.Graphics.FillPath(br, path);
            e.Graphics.ResetTransform();
            br.Dispose();
            path.Dispose();

            //e.Graphics.DrawRectangle(Pens.OrangeRed, rectContent);

            /*
            Dim foreBrush As SolidBrush = New SolidBrush(color)
            If (enabled) Then
                g.DrawString(text, font, foreBrush, rect, sf)
            Else
                ControlPaint.DrawStringDisabled(g, text, font, backColor, _
                     rect, sf)
            End If
            foreBrush.Dispose()*/


            pathOuterBorder.Dispose();
            pathInnerBorder.Dispose();
            pathBtnBorder.Dispose();
/*
            penOuterBorder.Dispose();
            penInnerBorder.Dispose();
            penLeftButton.Dispose();

            brBackground.Dispose();
            brInnerBrush.Dispose();
            brButtonLeft.Dispose();
            brButton.Dispose();*/
        }

        #endregion




        #region ListControlOverrides
        public int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, 1000, 1000);
            System.Drawing.CharacterRange[] ranges = { new System.Drawing.CharacterRange(0, text.Length) };
            System.Drawing.Region[] regions = new System.Drawing.Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

        public override int SelectedIndex
        {
            get { return _selectedIndex; }
            set 
            { 
                if(_listBox != null)
                {
                    if (_listBox.Items.Count == 0)
                        return;

                    if ((this.DataSource != null) && value == -1)
                        return;

                    if (value <= (_listBox.Items.Count - 1) && value >= -1)
                    {
                        _listBox.SelectedIndex = value;
                        _selectedIndex = value;
                        Text = _listBox.GetItemText(_listBox.SelectedItem);
                        OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
            }
        }

        public object SelectedItem
        {
            get { return _listBox.SelectedItem;  }
            set 
            { 
                _listBox.SelectedItem = value;
                this.SelectedIndex = _listBox.SelectedIndex;
            }
        }

        public new object SelectedValue
        {
            get { return base.SelectedValue; }
            set
            {
                base.SelectedValue = value;
            }
        }

        protected override void RefreshItem(int index)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void RefreshItems()
        {
            //base.RefreshItems();
        }

        protected override void SetItemCore(int index, object value)
        {
            //base.SetItemCore(index, value);
        }

        protected override void SetItemsCore(System.Collections.IList items)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion




        #region NestedControlsEvents

        void Control_LostFocus(object sender, EventArgs e)
        {
            OnLostFocus(e);
        }

        void Control_GotFocus(object sender, EventArgs e)
        {
            OnGotFocus(e);
        }

        void Control_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        void Control_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        void Control_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }


        void _listBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i;
            for (i = 0; i < (_listBox.Items.Count); i++)
            {
                if (_listBox.GetItemRectangle(i).Contains(_listBox.PointToClient(MousePosition)))
                {
                    _listBox.SelectedIndex = i;
                    return;
                }
            }
        }

        void _listBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_listBox.Items.Count == 0)
            {
                return;
            }

            if (_listBox.SelectedItems.Count != 1)
            {
                return;
            }

            this.SelectedIndex = _listBox.SelectedIndex;

            if (DropDownStyle == ComboBoxStyle.DropDownList)
            {
                this.Invalidate(true);
            }

            IsDroppedDown = false;
        }

        void _listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
          //  if (e.Index >= 0)
          //  {
        //        if (DrawItem != null)
        //        {
        //            DrawItem(this, e);
       //         }
      //      }

            //ComboBox cBox = sender as ComboBox;
            e.DrawBackground();

            // A dropdownlist may initially have no item selected, so skip the highlighting:
            if (e.Index >= 0)
            {
                Brush bColor = new SolidBrush(System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(15)))), ((int)(((byte)(6))))));
                Brush text = new SolidBrush(System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(177)))), ((int)(((byte)(10))))));
                Brush back = new SolidBrush(System.Drawing.Color.Black);
                Brush brush = ((e.State & DrawItemState.Selected) > 0) ? text : new SolidBrush(ForeColor);
                //Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.FillRectangle((e.State & DrawItemState.Selected) > 0 ? back : bColor, e.Bounds);
                e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);

                text.Dispose(); back.Dispose(); brush.Dispose();
            }
            e.DrawFocusRectangle();
        }

        void _listBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (MeasureItem != null)
            {
                MeasureItem(this, e);
            }
        }


        void _popupControl_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            _isDroppedDown = false;
            pressed = false;
            if (!this.RectangleToScreen(this.ClientRectangle).Contains(MousePosition))
            {
                hovered = false;
            }
            Invalidate(true);
        }
        #endregion

        #region PrivateMethods

        public void AdjustControls()
        {
            this.SuspendLayout();

            var textBox = _hackBox._textBox;

            resize = true;
            textBox.Top = 4;
            textBox.Left = 5;

            this.Height = textBox.Top + textBox.Height + textBox.Top;
            rectContent = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width, this.Height);

            rectBtn = new System.Drawing.Rectangle(this.ClientRectangle.Width - 18, this.ClientRectangle.Top, 18, this.Height);
            textBox.Width = rectBtn.Left - 1 - textBox.Left;

            _hackBox.Bounds = textBox.Bounds;

            this.ResumeLayout();

            Invalidate(true);
        }

        private Point CalculateDropPosition()
        {
            Point point = new Point(0, this.Height);
            if ((this.PointToScreen(new Point(0, 0)).Y + this.Height + _controlHost.Height) > Screen.PrimaryScreen.WorkingArea.Height)
            {
                point.Y = -this._controlHost.Height - 7;
            }
            return point;
        }

        private Point CalculateDropPosition(int myHeight, int controlHostHeight)
        {
            Point point = new Point(0, myHeight);
            if ((this.PointToScreen(new Point(0, 0)).Y + this.Height + controlHostHeight) > Screen.PrimaryScreen.WorkingArea.Height)
            {
                point.Y = -controlHostHeight - 7;
            }
            return point;
        }

        #endregion      



        
        #region VirtualMethods

        public virtual void OnDroppedDown(object sender, EventArgs e)
        {
            if (DroppedDown != null)
            {
                DroppedDown(this, e);
            }
        }

        #endregion

        #region Render

        public static GraphicsPath CreateRoundRectangle(Rectangle rectangle, int topLeftRadius, int topRightRadius,
            int bottomRightRadius, int bottomLeftRadius)
        {
            GraphicsPath path = new GraphicsPath();
            int l = rectangle.Left;
            int t = rectangle.Top;
            int w = rectangle.Width;
            int h = rectangle.Height;

            if(topLeftRadius > 0)
            {
                path.AddArc(l, t, topLeftRadius * 2, topLeftRadius * 2, 180, 90);
            }
            path.AddLine(l + topLeftRadius, t, l + w - topRightRadius, t);
            if (topRightRadius > 0)
            {
                path.AddArc(l + w - topRightRadius * 2, t, topRightRadius * 2, topRightRadius * 2, 270, 90);
            }
            path.AddLine(l + w, t + topRightRadius, l + w, t + h - bottomRightRadius);
            if (bottomRightRadius > 0)
            {
                path.AddArc(l + w - bottomRightRadius * 2, t + h - bottomRightRadius * 2,
                    bottomRightRadius * 2, bottomRightRadius * 2, 0, 90);
            }
            path.AddLine(l + w - bottomRightRadius, t + h, l + bottomLeftRadius, t + h);
            if(bottomLeftRadius >0)
            {
                path.AddArc(l, t + h - bottomLeftRadius * 2, bottomLeftRadius * 2, bottomLeftRadius * 2, 90, 90);
            }
            path.AddLine(l, t + h - bottomLeftRadius, l, t + topLeftRadius);
            path.CloseFigure();
            return path;
        }

        #endregion
    }

    
}
