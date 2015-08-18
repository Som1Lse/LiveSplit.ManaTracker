using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LiveSplit;
using LiveSplit.UI;
using LiveSplit.Model;
using LiveSplit.Options;

namespace LiveSplit.ManaTracker {
    public partial class ManaTrackerSettings:UserControl {
        public bool p_HeaderShow { get; set; }
        public bool p_HeaderManaShow { get; set; }
        public bool p_HeaderRegenShow { get; set; }
        public bool p_HeaderPotionsShow { get; set; }
        public bool p_HeaderTextColorEnabled { get; set; }
        public bool p_HeaderTextFontEnabled { get; set; }

        public bool p_GraphShow { get; set; }
        public bool p_GraphTimespanEnabled { get; set; }
        public bool p_GraphManaShow { get; set; }
        public bool p_GraphRegenShow { get; set; }
        public bool p_GraphDrinkLineShow { get; set; }
        public bool p_GraphDrinkImageShow { get; set; }
        public bool p_GraphPickupLineShow { get; set; }
        public bool p_GraphPickupImageShow { get; set; }
        public bool p_GraphBlinkLineShow { get; set; }
        public bool p_GraphBlinkImageShow { get; set; }
        public bool p_GraphLoadLineShow { get; set; }
        public bool p_GraphLoadImageShow { get; set; }
        public bool p_GraphSplitLineShow { get; set; }
        public bool p_GraphSplitImageShow { get; set; }
        public bool p_GraphFlip { get; set; }
        public bool p_GraphIconsOnBottom { get; set; }
        public bool p_GraphBloodOxHeart { get; set; }
        
        public string p_HeaderText { get; set; }
        public string p_HeaderManaText { get; set; }
        public string p_HeaderRegenText { get; set; }
        public string p_HeaderPotionsText { get; set; }
        
        public int p_GraphWidth { get; set; }
        public int p_GraphHeight { get; set; }
        public int p_GraphTimespan { get; set; }
        public int p_GraphIconPadding { get; set; }
        public int p_GraphLineWidth { get; set; }

        public Font p_HeaderTextFont { get; set; }
        public string p_HeaderTextFontName { get {
            return String.Format("{0} {1}",p_HeaderTextFont.FontFamily.Name,p_HeaderTextFont.Style);
        } }
        
        public Color p_HeaderBackgroundColor { get; set; }
        public Color p_HeaderTextColor { get; set; }

        public Color p_GraphBackgroundColor { get; set; }
        public Color p_GraphManaColor { get; set; }
        public Color p_GraphRegenColor { get; set; }
        public Color p_GraphDrinkLineColor { get; set; }
        public Color p_GraphPickupLineColor { get; set; }
        public Color p_GraphBlinkLineColor { get; set; }
        public Color p_GraphLoadLineColor { get; set; }
        public Color p_GraphSplitLineColor { get; set; }

        public Image p_GraphDrinkImage {
            get { return GraphDrinkImage.Image; }
            set { GraphDrinkImage.Image = value; }
        }
        public Image p_GraphPickupImage {
            get { return GraphPickupImage.Image; }
            set { GraphPickupImage.Image = value; }
        }
        public Image p_GraphBlinkImage {
            get { return GraphBlinkImage.Image; }
            set { GraphBlinkImage.Image = value; }
        }
        public Image p_GraphLoadImage {
            get { return GraphLoadImage.Image; }
            set { GraphLoadImage.Image = value; }
        }
        public Image p_GraphSplitImage {
            get { return GraphSplitImage.Image; }
            set { GraphSplitImage.Image = value; }
        }
        
        public ManaTrackerSettings(LiveSplitState State){
            InitializeComponent();

            p_HeaderShow = false;
            p_HeaderManaShow = false;
            p_HeaderRegenShow = false;
            p_HeaderPotionsShow = false;
            p_HeaderTextColorEnabled = false;
            p_HeaderTextFontEnabled = false;

            p_GraphShow = true;
            p_GraphTimespanEnabled = true;
            p_GraphManaShow = true;
            p_GraphRegenShow = true;
            p_GraphDrinkLineShow = true;
            p_GraphDrinkImageShow = false;
            p_GraphPickupLineShow = true;
            p_GraphPickupImageShow = false;
            p_GraphBlinkLineShow = true;
            p_GraphBlinkImageShow = false;
            p_GraphLoadLineShow = true;
            p_GraphLoadImageShow = false;
            p_GraphSplitLineShow = true;
            p_GraphSplitImageShow = false;
            p_GraphFlip = false;
            p_GraphIconsOnBottom = false;
            p_GraphBloodOxHeart = true;
        
            p_HeaderText = "Mana Tracker: ";
            p_HeaderManaText = "";
            p_HeaderRegenText = "";
            p_HeaderPotionsText = "";
        
            p_GraphWidth = 200;
            p_GraphHeight = 100;
            p_GraphTimespan = 30;
            p_GraphIconPadding = 16;
            p_GraphLineWidth = 1;
        
            p_HeaderBackgroundColor = Color.Transparent;
            p_HeaderTextColor = Color.White;
            p_HeaderTextFont = State.LayoutSettings.TextFont;

            p_GraphBackgroundColor = Color.Transparent;
            p_GraphManaColor = Color.FromArgb(90,182,239);
            p_GraphRegenColor = Color.FromArgb(128,p_GraphManaColor);
            p_GraphDrinkLineColor = Color.FromArgb(173,223,239);
            p_GraphPickupLineColor = Color.FromArgb(8,40,99);
            p_GraphBlinkLineColor = Color.FromArgb(227,243,214);
            p_GraphLoadLineColor = Color.DimGray;
            p_GraphSplitLineColor = Color.White;

            CheckBoxAssociates = new Dictionary<object,Control>{
                {HeaderShow,             HeaderLayout},
                {HeaderManaShow,         HeaderManaText},
                {HeaderRegenShow,        HeaderRegenText},
                {HeaderPotionsShow,      HeaderPotionsText},
                {HeaderTextColorEnabled, HeaderTextColor},
                {HeaderTextFontEnabled,  HeaderTextFontLayout},

                {GraphShow,              GraphLayout},
                {GraphTimespanEnabled,   GraphTimespanLayout},
                {GraphManaShow,          GraphManaColor},
                {GraphRegenShow,         GraphRegenColor},
                {GraphDrinkLineShow,     GraphDrinkLineColor},
                {GraphDrinkImageShow,    GraphDrinkImage},
                {GraphPickupLineShow,    GraphPickupLineColor},
                {GraphPickupImageShow,   GraphPickupImage},
                {GraphBlinkLineShow,     GraphBlinkLineColor},
                {GraphBlinkImageShow,    GraphBlinkImage},
                {GraphLoadLineShow,      GraphLoadLineColor},
                {GraphLoadImageShow,     GraphLoadImage},
                {GraphSplitLineShow,     GraphSplitLineColor},
                {GraphSplitImageShow,    GraphSplitImage},
            };

            TrackBarAssociates = new Dictionary<object,NumericUpDown>{
                {GraphSizeTrack,       GraphSizeNumber},
                {GraphTimespanTrack,   GraphTimespanNumber},
                {GraphIconPaddingTrack,GraphIconPaddingNumber},
                {GraphLineWidthTrack,  GraphLineWidthNumber},
            };

            NumericUpDownAssociates = new Dictionary<object,TrackBar>{
                {GraphSizeNumber,       GraphSizeTrack},
                {GraphTimespanNumber,   GraphTimespanTrack},
                {GraphIconPaddingNumber,GraphIconPaddingTrack},
                {GraphLineWidthNumber,  GraphLineWidthTrack},
            };

            ImageDialogTitles = new Dictionary<object,string>{
                {GraphDrinkImage,"Set Potion Drink Image..."},
                {GraphPickupImage,"Set Potion Drink Image..."},
                {GraphBlinkImage,"Set Blink Image..."},
                {GraphLoadImage,"Set Load Image..."},
                {GraphSplitImage,"Set Split Image..."},
            };

            HeaderShow.DataBindings.Add("Checked",this,"p_HeaderShow",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderManaShow.DataBindings.Add("Checked",this,"p_HeaderManaShow",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderRegenShow.DataBindings.Add("Checked",this,"p_HeaderRegenShow",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderPotionsShow.DataBindings.Add("Checked",this,"p_HeaderPotionsShow",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderTextColorEnabled.DataBindings.Add("Checked",this,"p_HeaderTextColorEnabled",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderTextFontEnabled.DataBindings.Add("Checked",this,"p_HeaderTextFontEnabled",false,DataSourceUpdateMode.OnPropertyChanged);

            GraphShow.DataBindings.Add("Checked",this,"p_GraphShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphTimespanEnabled.DataBindings.Add("Checked",this,"p_GraphTimespanEnabled",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphManaShow.DataBindings.Add("Checked",this,"p_GraphManaShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphRegenShow.DataBindings.Add("Checked",this,"p_GraphRegenShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphDrinkLineShow.DataBindings.Add("Checked",this,"p_GraphDrinkLineShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphDrinkImageShow.DataBindings.Add("Checked",this,"p_GraphDrinkImageShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphPickupLineShow.DataBindings.Add("Checked",this,"p_GraphPickupLineShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphPickupImageShow.DataBindings.Add("Checked",this,"p_GraphPickupImageShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphBlinkLineShow.DataBindings.Add("Checked",this,"p_GraphBlinkLineShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphBlinkImageShow.DataBindings.Add("Checked",this,"p_GraphBlinkImageShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphLoadLineShow.DataBindings.Add("Checked",this,"p_GraphLoadLineShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphLoadImageShow.DataBindings.Add("Checked",this,"p_GraphLoadImageShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphSplitLineShow.DataBindings.Add("Checked",this,"p_GraphSplitLineShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphSplitImageShow.DataBindings.Add("Checked",this,"p_GraphSplitImageShow",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphFlip.DataBindings.Add("Checked",this,"p_GraphFlip",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphIconsOnBottom.DataBindings.Add("Checked",this,"p_GraphIconsOnBottom",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphBloodOxHeart.DataBindings.Add("Checked",this,"p_GraphBloodOxHeart",false,DataSourceUpdateMode.OnPropertyChanged);
            
            HeaderTextFontName.DataBindings.Add("Text",this,"p_HeaderTextFontName",false,DataSourceUpdateMode.OnPropertyChanged);
            
            HeaderText.DataBindings.Add("Text",this,"p_HeaderText",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderManaText.DataBindings.Add("Text",this,"p_HeaderManaText",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderRegenText.DataBindings.Add("Text",this,"p_HeaderRegenText",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderPotionsText.DataBindings.Add("Text",this,"p_HeaderPotionsText",false,DataSourceUpdateMode.OnPropertyChanged);
            
            SetLayoutMode(LayoutMode.Vertical);
            GraphTimespanNumber.DataBindings.Add("Value",this,"p_GraphTimespan",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphIconPaddingNumber.DataBindings.Add("Value",this,"p_GraphIconPadding",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphLineWidthNumber.DataBindings.Add("Value",this,"p_GraphLineWidth",false,DataSourceUpdateMode.OnPropertyChanged);
            
            HeaderBackgroundColor.DataBindings.Add("BackColor",this,"p_HeaderBackgroundColor",false,DataSourceUpdateMode.OnPropertyChanged);
            HeaderTextColor.DataBindings.Add("BackColor",this,"p_HeaderTextColor",false,DataSourceUpdateMode.OnPropertyChanged);

            GraphBackgroundColor.DataBindings.Add("BackColor",this,"p_GraphBackgroundColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphManaColor.DataBindings.Add("BackColor",this,"p_GraphManaColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphRegenColor.DataBindings.Add("BackColor",this,"p_GraphRegenColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphDrinkLineColor.DataBindings.Add("BackColor",this,"p_GraphDrinkLineColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphPickupLineColor.DataBindings.Add("BackColor",this,"p_GraphPickupLineColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphBlinkLineColor.DataBindings.Add("BackColor",this,"p_GraphBlinkLineColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphLoadLineColor.DataBindings.Add("BackColor",this,"p_GraphLoadLineColor",false,DataSourceUpdateMode.OnPropertyChanged);
            GraphSplitLineColor.DataBindings.Add("BackColor",this,"p_GraphSplitLineColor",false,DataSourceUpdateMode.OnPropertyChanged);
        }
            
        public void SetLayoutMode(LayoutMode Mode){
            const int SizeTrackWidthMin  = 30,SizeTrackWidthMax  = 400,
                      SizeTrackHeightMin = 15,SizeTrackHeightMax = 200;
            
            GraphSizeTrack.Minimum = Math.Min(SizeTrackWidthMin,SizeTrackHeightMin);
            GraphSizeTrack.Maximum = Math.Max(SizeTrackWidthMax,SizeTrackHeightMax);

            GraphSizeNumber.DataBindings.Clear();
            if(Mode == LayoutMode.Vertical){
                GraphSizeLabel.Text = "Height: ";
                GraphSizeNumber.DataBindings.Add("Value",this,"p_GraphHeight",false,DataSourceUpdateMode.OnPropertyChanged);
                GraphSizeTrack.Minimum = SizeTrackHeightMin;
                GraphSizeTrack.Maximum = SizeTrackHeightMax;
            }else{
                GraphSizeLabel.Text = "Width: ";
                GraphSizeNumber.DataBindings.Add("Value",this,"p_GraphWidth",false,DataSourceUpdateMode.OnPropertyChanged);
                GraphSizeTrack.Minimum = SizeTrackWidthMin;
                GraphSizeTrack.Maximum = SizeTrackWidthMax;
            }
        }

        public void UpdateEnabledControls(){ 
            foreach(var e in CheckBoxAssociates) e.Value.Enabled = ((CheckBox)e.Key).Checked;
        }

        Dictionary<object,Control> CheckBoxAssociates;
        Dictionary<object,NumericUpDown> TrackBarAssociates;
        Dictionary<object,TrackBar> NumericUpDownAssociates;
        Dictionary<object,string> ImageDialogTitles;

        private void CheckedChanged(object Sender,EventArgs e){
            CheckBoxAssociates[Sender].Enabled = ((CheckBox)Sender).Checked;
        }

        private void TrackScroll(object Sender,EventArgs e){
            TrackBarAssociates[Sender].Value = ((TrackBar)Sender).Value;
        }

        private void ValueChanged(object Sender,EventArgs e){
            var TrackBar = NumericUpDownAssociates[Sender];
            TrackBar.Value = Math.Max(Math.Min((int)((NumericUpDown)Sender).Value,
                                               TrackBar.Maximum),TrackBar.Minimum);
        }

        private void ColorButtonClick(object Sender,EventArgs e){
            SettingsHelper.ColorButtonClick((Button)Sender,this);
        }

        private void ChooseFontClick(object sender,EventArgs e) {
            p_HeaderTextFont = SettingsHelper.ChooseFont(this,p_HeaderTextFont,7,20);
        }

        private void ImageButtonClick(object Sender,EventArgs e){
            OpenImageDialog.Title = ImageDialogTitles[Sender];
            if(OpenImageDialog.ShowDialog() == DialogResult.OK){
                try {
                    var LoadedImage = Image.FromFile(OpenImageDialog.FileName);
                    
                    var Width = LoadedImage.Width;
                    var Height = LoadedImage.Height;
                    const int BorderSize = 2;
                    if(Width >= Height){ 
                        if(Width>((PictureBox)Sender).Width-BorderSize){
                            Height = Height*(((PictureBox)Sender).Width-BorderSize)/Width;
                            Width = ((PictureBox)Sender).Width-BorderSize;
                        }
                    }else{
                        if(Height>((PictureBox)Sender).Height-BorderSize){
                            Width = Width*(((PictureBox)Sender).Height-BorderSize)/Height;
                            Height = ((PictureBox)Sender).Height-BorderSize;
                        }
                    }

                    if(Width != LoadedImage.Width || Height != LoadedImage.Height){
                        var ScaledBitmap = new Bitmap(Width,Height);
                        ScaledBitmap.SetResolution(LoadedImage.HorizontalResolution,LoadedImage.VerticalResolution);

                        using(var GraphicsCanvas = Graphics.FromImage(ScaledBitmap)){ 
                            GraphicsCanvas.CompositingMode = CompositingMode.SourceCopy;
                            GraphicsCanvas.CompositingQuality = CompositingQuality.HighQuality;
                            GraphicsCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            GraphicsCanvas.SmoothingMode = SmoothingMode.HighQuality;
                            GraphicsCanvas.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            using(var Attributes = new ImageAttributes()){
                                Attributes.SetWrapMode(WrapMode.TileFlipXY);
                                
                                GraphicsCanvas.DrawImage(LoadedImage,new Rectangle(0,0,Width,Height),
                                                         0,0,LoadedImage.Width,LoadedImage.Height,
                                                         GraphicsUnit.Pixel,Attributes);
                            }
                        }
                        LoadedImage = ScaledBitmap;
                    }
                    ((PictureBox)Sender).Image = LoadedImage;
                }catch (Exception ex){
                    Log.Error(ex);
                    MessageBox.Show("Could not load image: "+ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}
