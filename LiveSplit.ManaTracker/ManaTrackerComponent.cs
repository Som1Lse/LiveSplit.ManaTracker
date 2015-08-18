using System;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

using LiveSplit.UI;
using LiveSplit.UI.Components;
using LiveSplit.Model;
using LiveSplit.TimeFormatters;

namespace LiveSplit.ManaTracker {

struct ManaSnapshot {
    public int Mana,Regen,Potions;

    public ManaSnapshot(int Mana,int Regen,int Potions) {
        this.Mana = Mana;
        this.Regen = Regen;
        this.Potions = Potions;
    }
}

enum SpecialEventType {
    Blink,Pickup,Drink,Split,Load,Empty,
}

struct SpecialEvent {
    public SpecialEventType Type;
    public int Time,Length;

    public SpecialEvent(SpecialEventType Type,int Time,int Length = 0){ 
        this.Type = Type;
        this.Time = Time;
        this.Length = Length;
    }
}

public class ManaTrackerComponent:IComponent {
    public ManaTrackerComponent(LiveSplitState State){
        Settings = new ManaTrackerSettings(State);

        this.State = State;
        
        State.OnStart += OnRunStart;
        State.OnSplit += OnRunSplit;
        State.OnSkipSplit += OnRunSplit;
        State.OnUndoSplit += OnUndoRunSplit;

        GameProcess = GetGameProcess();

        CurrentSplitIndex = -1;
        
        ManaSnapshots = new List<ManaSnapshot>();
        SpecialEvents = new List<SpecialEvent>();

        IsInLoadingScreen = false;

        HeaderLabel = new SimpleLabel();
    }

    public Control GetSettingsControl(LayoutMode Mode){ Settings.SetLayoutMode(Mode); return Settings; }
    public XmlNode GetSettings(XmlDocument Document){
        var Element = Document.CreateElement("Settings");

        Element.AppendChild(SettingsHelper.ToElement(Document,"Version","1.0.0.0"));

        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderShow",Settings.p_HeaderShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderManaShow",Settings.p_HeaderManaShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderRegenShow",Settings.p_HeaderRegenShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderPotionsShow",Settings.p_HeaderPotionsShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderTextColorEnabled",Settings.p_HeaderTextColorEnabled));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderTextFontEnabled",Settings.p_HeaderTextFontEnabled));

        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphShow",Settings.p_GraphShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphTimespanEnabled",Settings.p_GraphTimespanEnabled));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphManaShow",Settings.p_GraphManaShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphRegenShow",Settings.p_GraphRegenShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphDrinkLineShow",Settings.p_GraphDrinkLineShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphDrinkImageShow",Settings.p_GraphDrinkImageShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphPickupLineShow",Settings.p_GraphPickupLineShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphPickupImageShow",Settings.p_GraphPickupImageShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphBlinkLineShow",Settings.p_GraphBlinkLineShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphBlinkImageShow",Settings.p_GraphBlinkImageShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphLoadLineShow",Settings.p_GraphLoadLineShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphLoadImageShow",Settings.p_GraphLoadImageShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphSplitLineShow",Settings.p_GraphSplitLineShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphSplitImageShow",Settings.p_GraphSplitImageShow));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphFlip",Settings.p_GraphFlip));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphIconsOnBottom",Settings.p_GraphIconsOnBottom));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphBloodOxHeart",Settings.p_GraphBloodOxHeart));

        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderText",Settings.p_HeaderText));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderManaText",Settings.p_HeaderManaText));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderRegenText",Settings.p_HeaderRegenText));
        Element.AppendChild(SettingsHelper.ToElement(Document,"HeaderPotionsText",Settings.p_HeaderPotionsText));
        
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphWidth",Settings.p_GraphWidth));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphHeight",Settings.p_GraphHeight));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphTimespan",Settings.p_GraphTimespan));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphIconPadding",Settings.p_GraphIconPadding));
        Element.AppendChild(SettingsHelper.ToElement(Document,"GraphLineWidth",Settings.p_GraphLineWidth));

        Element.AppendChild(SettingsHelper.CreateFontElement(Document,"HeaderTextFont",Settings.p_HeaderTextFont));

        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_HeaderBackgroundColor,"HeaderBackgroundColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_HeaderTextColor,"HeaderTextColor"));

        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphBackgroundColor,"GraphBackgroundColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphManaColor,"GraphManaColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphRegenColor,"GraphRegenColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphDrinkLineColor,"GraphDrinkLineColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphPickupLineColor,"GraphPickupLineColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphBlinkLineColor,"GraphBlinkLineColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphLoadLineColor,"GraphLoadLineColor"));
        Element.AppendChild(SettingsHelper.ToElement(Document,Settings.p_GraphSplitLineColor,"GraphSplitLineColor"));

        Element.AppendChild(SettingsHelper.CreateImageElement(Document,"GraphDrinkImage",Settings.p_GraphDrinkImage));
        Element.AppendChild(SettingsHelper.CreateImageElement(Document,"GraphPickupImage",Settings.p_GraphPickupImage));
        Element.AppendChild(SettingsHelper.CreateImageElement(Document,"GraphBlinkImage",Settings.p_GraphBlinkImage));
        Element.AppendChild(SettingsHelper.CreateImageElement(Document,"GraphLoadImage",Settings.p_GraphLoadImage));
        Element.AppendChild(SettingsHelper.CreateImageElement(Document,"GraphSplitImage",Settings.p_GraphSplitImage));

        return Element;
    }
    public void SetSettings(XmlNode Node){
        var Element = (XmlElement)Node;

        Settings.p_HeaderShow = SettingsHelper.ParseBool(Element["HeaderShow"],Settings.p_HeaderShow);
        Settings.p_HeaderManaShow = SettingsHelper.ParseBool(Element["HeaderManaShow"],Settings.p_HeaderManaShow);
        Settings.p_HeaderRegenShow = SettingsHelper.ParseBool(Element["HeaderRegenShow"],Settings.p_HeaderRegenShow);
        Settings.p_HeaderPotionsShow = SettingsHelper.ParseBool(Element["HeaderPotionsShow"],Settings.p_HeaderPotionsShow);
        Settings.p_HeaderTextColorEnabled = SettingsHelper.ParseBool(Element["HeaderTextColorEnabled"],Settings.p_HeaderTextColorEnabled);
        Settings.p_HeaderTextFontEnabled = SettingsHelper.ParseBool(Element["HeaderTextFontEnabled"],Settings.p_HeaderTextFontEnabled);

        Settings.p_GraphShow = SettingsHelper.ParseBool(Element["GraphShow"],Settings.p_GraphShow);
        Settings.p_GraphTimespanEnabled = SettingsHelper.ParseBool(Element["GraphTimespanEnabled"],Settings.p_GraphTimespanEnabled);
        Settings.p_GraphManaShow = SettingsHelper.ParseBool(Element["GraphManaShow"],Settings.p_GraphManaShow);
        Settings.p_GraphRegenShow = SettingsHelper.ParseBool(Element["GraphRegenShow"],Settings.p_GraphRegenShow);
        Settings.p_GraphDrinkLineShow = SettingsHelper.ParseBool(Element["GraphDrinkLineShow"],Settings.p_GraphDrinkLineShow);
        Settings.p_GraphDrinkImageShow = SettingsHelper.ParseBool(Element["GraphDrinkImageShow"],Settings.p_GraphDrinkImageShow);
        Settings.p_GraphPickupLineShow = SettingsHelper.ParseBool(Element["GraphPickupLineShow"],Settings.p_GraphPickupLineShow);
        Settings.p_GraphPickupImageShow = SettingsHelper.ParseBool(Element["GraphPickupImageShow"],Settings.p_GraphPickupImageShow);
        Settings.p_GraphBlinkLineShow = SettingsHelper.ParseBool(Element["GraphBlinkLineShow"],Settings.p_GraphBlinkLineShow);
        Settings.p_GraphBlinkImageShow = SettingsHelper.ParseBool(Element["GraphBlinkImageShow"],Settings.p_GraphBlinkImageShow);
        Settings.p_GraphLoadLineShow = SettingsHelper.ParseBool(Element["GraphLoadLineShow"],Settings.p_GraphLoadLineShow);
        Settings.p_GraphLoadImageShow = SettingsHelper.ParseBool(Element["GraphLoadImageShow"],Settings.p_GraphLoadImageShow);
        Settings.p_GraphSplitLineShow = SettingsHelper.ParseBool(Element["GraphSplitLineShow"],Settings.p_GraphSplitLineShow);
        Settings.p_GraphSplitImageShow = SettingsHelper.ParseBool(Element["GraphSplitImageShow"],Settings.p_GraphSplitImageShow);
        Settings.p_GraphFlip = SettingsHelper.ParseBool(Element["GraphFlip"],Settings.p_GraphFlip);
        Settings.p_GraphIconsOnBottom = SettingsHelper.ParseBool(Element["GraphIconsOnBottom"],Settings.p_GraphIconsOnBottom);
        Settings.p_GraphBloodOxHeart = SettingsHelper.ParseBool(Element["GraphBloodOxHeart"],Settings.p_GraphBloodOxHeart);
        
        Settings.p_HeaderText = SettingsHelper.ParseString(Element["HeaderText"],Settings.p_HeaderText);
        Settings.p_HeaderManaText = SettingsHelper.ParseString(Element["HeaderManaText"],Settings.p_HeaderManaText);
        Settings.p_HeaderRegenText = SettingsHelper.ParseString(Element["HeaderRegenText"],Settings.p_HeaderRegenText);
        Settings.p_HeaderPotionsText = SettingsHelper.ParseString(Element["HeaderPotionsText"],Settings.p_HeaderPotionsText);
        
        Settings.p_GraphWidth = SettingsHelper.ParseInt(Element["GraphWidth"],Settings.p_GraphWidth);
        Settings.p_GraphHeight = SettingsHelper.ParseInt(Element["GraphHeight"],Settings.p_GraphHeight);
        Settings.p_GraphTimespan = SettingsHelper.ParseInt(Element["GraphTimespan"],Settings.p_GraphTimespan);
        Settings.p_GraphIconPadding = SettingsHelper.ParseInt(Element["GraphIconPadding"],Settings.p_GraphIconPadding);
        Settings.p_GraphLineWidth = SettingsHelper.ParseInt(Element["GraphLineWidth"],Settings.p_GraphLineWidth);

        Settings.p_HeaderTextFont = ParseFont(Element["HeaderTextFont"],Settings.p_HeaderTextFont);

        Settings.p_HeaderBackgroundColor = SettingsHelper.ParseColor(Element["HeaderBackgroundColor"],Settings.p_HeaderBackgroundColor);
        Settings.p_HeaderTextColor = SettingsHelper.ParseColor(Element["HeaderTextColor"],Settings.p_HeaderTextColor);

        Settings.p_GraphBackgroundColor = SettingsHelper.ParseColor(Element["GraphBackgroundColor"],Settings.p_GraphBackgroundColor);
        Settings.p_GraphManaColor = SettingsHelper.ParseColor(Element["GraphManaColor"],Settings.p_GraphManaColor);
        Settings.p_GraphRegenColor = SettingsHelper.ParseColor(Element["GraphRegenColor"],Settings.p_GraphRegenColor);
        Settings.p_GraphDrinkLineColor = SettingsHelper.ParseColor(Element["GraphDrinkLineColor"],Settings.p_GraphDrinkLineColor);
        Settings.p_GraphPickupLineColor = SettingsHelper.ParseColor(Element["GraphPickupLineColor"],Settings.p_GraphPickupLineColor);
        Settings.p_GraphBlinkLineColor = SettingsHelper.ParseColor(Element["GraphBlinkLineColor"],Settings.p_GraphBlinkLineColor);
        Settings.p_GraphLoadLineColor = SettingsHelper.ParseColor(Element["GraphLoadLineColor"],Settings.p_GraphLoadLineColor);
        Settings.p_GraphSplitLineColor = SettingsHelper.ParseColor(Element["GraphSplitLineColor"],Settings.p_GraphSplitLineColor);

        Settings.p_GraphDrinkImage = ParseImage(Element["GraphDrinkImage"],Settings.p_GraphDrinkImage);
        Settings.p_GraphPickupImage = ParseImage(Element["GraphPickupImage"],Settings.p_GraphPickupImage);
        Settings.p_GraphBlinkImage= ParseImage(Element["GraphBlinkImage"],Settings.p_GraphBlinkImage);
        Settings.p_GraphLoadImage = ParseImage(Element["GraphLoadImage"],Settings.p_GraphLoadImage);
        Settings.p_GraphSplitImage = ParseImage(Element["GraphSplitImage"],Settings.p_GraphSplitImage);

        Settings.UpdateEnabledControls();
    }

    private static Font ParseFont(XmlElement Element,Font Default){
        if(Element == null) return Default;
        return SettingsHelper.GetFontFromElement(Element);
    }

    private static Image ParseImage(XmlElement Element,Image Default){
        if(Element == null) return Default;
        return SettingsHelper.GetImageFromElement(Element);
    }

    private void DrawHeader(Graphics g,LiveSplitState State,float x,float y,float Width,float Height){
        HeaderLabel.ForeColor = Settings.p_HeaderTextColorEnabled?Settings.p_HeaderTextColor:
                                                                    State.LayoutSettings.TextColor;
        HeaderLabel.HorizontalAlignment = StringAlignment.Near;
        HeaderLabel.VerticalAlignment = StringAlignment.Center;
        
        HeaderLabel.X = x+4;
        HeaderLabel.Y = y+2;

        HeaderLabel.Width = Width;
        HeaderLabel.Height = Height-4;

        HeaderLabel.SetActualWidth(g);
        HeaderLabel.Font = Settings.p_HeaderTextFontEnabled?Settings.p_HeaderTextFont:State.LayoutSettings.TextFont;
        
        HeaderLabel.HasShadow = State.LayoutSettings.DropShadows;
        HeaderLabel.ShadowColor = State.LayoutSettings.ShadowsColor;

        g.FillRectangle(new SolidBrush(Settings.p_HeaderBackgroundColor),x,y,Width,Height);
        HeaderLabel.Draw(g);
    }

    private int GetDrawGraphCount(bool DrawGameTime){
        if(Settings.p_GraphTimespanEnabled) return Settings.p_GraphTimespan*10;
        
        var Count = ManaSnapshots.Count;

        if(DrawGameTime){
            foreach(var e in SpecialEvents){
                if(e.Type == SpecialEventType.Load) Count -= e.Length;
            }
        }

        return Count;
    }

    //draws the mana
    private void DrawGraphMana(Graphics g,float x,float y,float Width,float Height){
        if((!Settings.p_GraphManaShow && !Settings.p_GraphManaShow) || ManaSnapshots.Count == 0) return;
        
        if(!Settings.p_GraphIconsOnBottom) y += Settings.p_GraphIconPadding;
        Height -= Settings.p_GraphIconPadding;
        
        var Flip = Settings.p_GraphFlip;
        var DrawGameTime = State.CurrentTimingMethod == TimingMethod.GameTime;

        var Count = GetDrawGraphCount(DrawGameTime);

        var MaxMana = Settings.p_GraphBloodOxHeart?120:100;

        //the bottom part of the graph, the entire graph in many cases
        var PointsBottom = new List<PointF>(2+Count);
        List<PointF> PointsTop = null;//the top part of the graph if both mana and regen are enabled, otherwise unused
        if(Settings.p_GraphManaShow && Settings.p_GraphManaShow){
            PointsTop = new List<PointF>(2*Count);
        }
        
        PointsBottom.Add(new PointF(x+Width,Flip?y:y+Height));
        
        int k = SpecialEvents.Count-1;
        var NextLoadEvent = GetSpecialEvent(ref k,SpecialEventType.Load);
        for(int i = ManaSnapshots.Count-1,j = 0;i >= 0 && j<Count;--i,++j){
            if(DrawGameTime && i <= NextLoadEvent.Time+NextLoadEvent.Length){
                i -= NextLoadEvent.Length;
                NextLoadEvent = GetSpecialEvent(ref k,SpecialEventType.Load);
            }

            var PositionX = x+Width;
            if(Count>1) PositionX -= ((float)j)/(Count-1)*Width;

            var Snapshot = ManaSnapshots[i];
            if(Snapshot.Mana>MaxMana) Snapshot.Mana = MaxMana;
            if(Snapshot.Regen>MaxMana) Snapshot.Regen = MaxMana;

            var PositionY = (Settings.p_GraphManaShow?(Snapshot.Mana):(Snapshot.Regen))*Height/MaxMana;
            if(Flip){
                PositionY += y;
            }else{
                PositionY = y+Height-PositionY;
            }
            PointsBottom.Add(new PointF(PositionX,PositionY));

            if(PointsTop != null){
                PositionY = Snapshot.Regen*Height/MaxMana;
                if(Flip){
                    PositionY += y;
                }else{
                    PositionY = y+Height-PositionY;
                }
                PointsTop.Add(new PointF(PositionX,PositionY));
            }
        }

        if(PointsTop != null){
            for(int i = PointsBottom.Count-1;i>0;--i){
                PointsTop.Add(PointsBottom[i]);
            }
        }

        PointsBottom.Add(new PointF(PointsBottom[PointsBottom.Count-1].X,Flip?y:y+Height));

        g.FillPolygon(new SolidBrush(Settings.p_GraphManaShow?(Settings.p_GraphManaColor):(Settings.p_GraphRegenColor)),
                        PointsBottom.ToArray());

        if(PointsTop != null && PointsTop.Count>0){
            g.FillPolygon(new SolidBrush(Settings.p_GraphRegenColor),PointsTop.ToArray());
        }
    }

    private void GetGraphLineSettings(SpecialEventType EventType,
                                      out bool ShowLine,out bool ShowImage,out Color LineColor,out Image DrawnImage){
        switch(EventType){
            case SpecialEventType.Blink: {
                ShowLine = Settings.p_GraphBlinkLineShow;
                ShowImage = Settings.p_GraphBlinkImageShow && Settings.p_GraphBlinkImage != null;
                LineColor = Settings.p_GraphBlinkLineColor;
                DrawnImage = Settings.p_GraphBlinkImage;
                break;
            }
            case SpecialEventType.Pickup: {
                ShowLine = Settings.p_GraphPickupLineShow;
                ShowImage = Settings.p_GraphPickupImageShow && Settings.p_GraphPickupImage != null;
                LineColor = Settings.p_GraphPickupLineColor;
                DrawnImage = Settings.p_GraphPickupImage;
                break;
            }
            case SpecialEventType.Drink: {
                ShowLine = Settings.p_GraphDrinkLineShow;
                ShowImage = Settings.p_GraphDrinkImageShow && Settings.p_GraphDrinkImage != null;
                LineColor = Settings.p_GraphDrinkLineColor;
                DrawnImage = Settings.p_GraphDrinkImage;
                break;
            }
            case SpecialEventType.Split: {
                ShowLine = Settings.p_GraphSplitLineShow;
                ShowImage = Settings.p_GraphSplitImageShow && Settings.p_GraphSplitImage != null;
                LineColor = Settings.p_GraphSplitLineColor;
                DrawnImage = Settings.p_GraphSplitImage;
                break;
            }
            case SpecialEventType.Load: {
                ShowLine = Settings.p_GraphLoadLineShow;
                ShowImage = Settings.p_GraphLoadImageShow && Settings.p_GraphLoadImage != null;
                LineColor = Settings.p_GraphLoadLineColor;
                DrawnImage = Settings.p_GraphLoadImage;
                break;
            }
            default: {
                ShowLine = ShowImage = false;
                LineColor = Color.Transparent;
                DrawnImage = null;
                break;
            }
        }
    }

    //draws the lines and icons
    private void DrawGraphLines(Graphics g,float x,float y,float Width,float Height){
        var Flip = Settings.p_GraphIconsOnBottom;
        var DrawGameTime = State.CurrentTimingMethod == TimingMethod.GameTime;
        
        var Count = GetDrawGraphCount(DrawGameTime);
        var EndTime = ManaSnapshots.Count-Count;

        for(int i = SpecialEvents.Count-1;i >= 0;--i){
            var Event = SpecialEvents[i];
            
            if(Event.Time+Event.Length+Settings.p_GraphLineWidth*Count/Width<EndTime) return;

            if(DrawGameTime){
                if(Event.Type == SpecialEventType.Load){ 
                    EndTime -= Event.Length;
                    Event.Length = 0;
                }
            }else{
                if(Event.Time<EndTime){
                    Event.Length -= EndTime-Event.Time;
                    Event.Time = EndTime;
                }
            }

            bool ShowLine,ShowImage;
            Color LineColor;
            Image DrawnImage;
            GetGraphLineSettings(Event.Type,out ShowLine,out ShowImage,out LineColor,out DrawnImage);

            var PositionX = x+(Event.Time-EndTime)*Width/Count;
            
            var LineWidth = Event.Length*Width/Count+Settings.p_GraphLineWidth;
            if(ShowLine){
                var ImageHeight = ShowImage?DrawnImage.Height:0;

                var LineX = PositionX-Settings.p_GraphLineWidth/2;

                Brush Brush;
                if(ShowImage){
                    Brush = new LinearGradientBrush(new RectangleF(LineX,y+(Flip?Height:0),
                                                                   LineWidth,Height*(Flip?-1:1)),
                                                    Color.Transparent,Color.Transparent,90);
                    var Colors = new ColorBlend(3);
                    Colors.Colors[0] = Color.Transparent;
                    Colors.Colors[1] = Colors.Colors[2] = LineColor;
                    
                    Colors.Positions[0] = 0;
                    Colors.Positions[1] = ImageHeight/Height;
                    Colors.Positions[2] = 1;
                    
                    ((LinearGradientBrush)Brush).InterpolationColors = Colors;
                }else{
                    Brush = new SolidBrush(LineColor);
                }

                g.FillRectangle(Brush,LineX,y,LineWidth,Height);
            }
            if(ShowImage){
                g.DrawImage(DrawnImage,new PointF(PositionX-DrawnImage.Width/2,y+(Flip?Height-DrawnImage.Height:0)));
            }
        }
    }

    private void DrawGraph(Graphics g,float x,float y,float Width,float Height){
        g.FillRectangle(new SolidBrush(Settings.p_GraphBackgroundColor),x,y,Width,Height);
        
        DrawGraphMana(g,x,y,Width,Height);
        DrawGraphLines(g,x,y,Width,Height);
    }

    private void Draw(Graphics g,float Width,float Height){
        var y = 0;
        if(Settings.p_HeaderShow){
            DrawHeader(g,State,0,y,Width,24);
            y += (Settings.p_HeaderTextFontEnabled?Settings.p_HeaderTextFont:State.LayoutSettings.TextFont).Height;
        }
        if(Settings.p_GraphShow) DrawGraph(g,0,y,Width,Height-y);
    }

    public void DrawHorizontal(Graphics g,LiveSplitState State,float Height,Region ClipRegion){
        Draw(g,HorizontalWidth,Height);
    }
    public void DrawVertical(Graphics g,LiveSplitState State,float Width,Region ClipRegion){
        Draw(g,Width,VerticalHeight);
    }

    public void Update(IInvalidator Invalidator,LiveSplitState State,float Width,float Height,LayoutMode Mode){
        if(GameProcess == null || GameProcess.HasExited) GameProcess = GetGameProcess();

        if(CurrentSplitIndex == -1 && State.CurrentSplitIndex != -1 && Settings.p_GraphShow){
            OnRunStart(null,null);
            CurrentSplitIndex = State.CurrentSplitIndex;
            RunStartTime = (TimeSpan)State.CurrentTime[TimingMethod.RealTime];
        }

        if(GameProcess != null && !GameProcess.HasExited){
            int NewMana,NewRegen,NewPotions;
            if(ReadManaValues(out NewMana,out NewRegen,out NewPotions)){
                if(Settings.p_GraphShow && State.CurrentSplitIndex != -1){
                    var SecondTenths = GetTimeSecondTenths();

                    if(State.IsGameTimePaused){//loading
                        if(IsInLoadingScreen){
                            int Index = SpecialEvents.FindLastIndex(e => e.Type == SpecialEventType.Load);
                            if(Index == -1){
                                IsInLoadingScreen = false;
                            }else{
                                var Event = SpecialEvents[Index];
                                Event.Length = SecondTenths-Event.Time;
                                SpecialEvents[Index] = Event;
                            }
                        }
                        if(!IsInLoadingScreen){
                            SpecialEvents.Add(new SpecialEvent(SpecialEventType.Load,SecondTenths));
                            IsInLoadingScreen = true;
                        }
                    }else{
                        if(IsInLoadingScreen){
                            IsInLoadingScreen = false;
                            var Event = SpecialEvents[SpecialEvents.Count-1];
                            Event.Length = SecondTenths-Event.Time;
                            SpecialEvents[SpecialEvents.Count-1] = Event;
                        }else{
                            if(NewMana<Mana){//blink (maybe other power)
                                SpecialEvents.Add(new SpecialEvent(SpecialEventType.Blink,SecondTenths));
                            }else if(NewPotions>Potions){//pickup potion(s)
                                for(var i = NewPotions;i>Potions;--i){
                                    SpecialEvents.Add(new SpecialEvent(SpecialEventType.Pickup,SecondTenths));
                                }
                            }else if(NewPotions<Potions){//drink potion(s)
                                for(var i = NewPotions;i<Potions;++i){
                                    SpecialEvents.Add(new SpecialEvent(SpecialEventType.Drink,SecondTenths));
                                }
                            }
                        }
                    }

                    if(SecondTenths >= ManaSnapshots.Count){
                        //make sure the structure is filled
                        for(int i = ManaSnapshots.Count;i<SecondTenths;++i){
                            ManaSnapshots.Add(new ManaSnapshot(Mana,Regen,Potions));
                        }
                        //add the current mana values to the structure
                        ManaSnapshots.Add(new ManaSnapshot(NewMana,NewRegen,NewPotions));
                    }
                }

                //only update if game time is running, as we are in a loading screen otherwise wherein the values may fluctuate
                if(!State.IsGameTimePaused){
                    Mana = NewMana;
                    Regen = NewRegen;
                    Potions = NewPotions;
                }
            }
        }

        if(Settings.p_HeaderShow){
            HeaderLabel.Text = Settings.p_HeaderText;
            if(Settings.p_HeaderManaShow) HeaderLabel.Text += Mana+Settings.p_HeaderManaText;
            if(Settings.p_HeaderRegenShow) HeaderLabel.Text += Regen+Settings.p_HeaderRegenText;
            if(Settings.p_HeaderPotionsShow) HeaderLabel.Text += Potions+Settings.p_HeaderPotionsText;
                    
            if(Invalidator != null) Invalidator.Invalidate(0,0,Width,Height);
        }
    }

    public string ComponentName { get { return "ManaTracker"; } }
    
    public float MinimumWidth { get {
        return Math.Max(Settings.p_HeaderShow?48:0,
                        Settings.p_GraphShow?40:0);
    } }
    public float MinimumHeight { get {
        return (Settings.p_HeaderShow?16:0)+(Settings.p_GraphShow?20:0);
    } }

    public float HorizontalWidth { get {
        return Settings.p_GraphShow?Settings.p_GraphWidth:(Settings.p_HeaderShow?HeaderLabel.ActualWidth:0);
    } }
    public float VerticalHeight { get {
        return (Settings.p_HeaderShow?(Settings.p_HeaderTextFontEnabled?Settings.p_HeaderTextFont:State.LayoutSettings.TextFont).Height:0)+
               (Settings.p_GraphShow?(Settings.p_GraphHeight+Settings.p_GraphIconPadding):0);
    } }

    public float PaddingLeft   { get { return Settings.p_GraphShow?0:(Settings.p_HeaderShow?4:0); } }
    public float PaddingTop    { get { return Settings.p_HeaderShow?2:0; } }
    public float PaddingRight  { get { return PaddingLeft; } }
    public float PaddingBottom { get { return Settings.p_GraphShow?0:PaddingTop; } }

    public IDictionary<string,Action> ContextMenuControls { get; protected set; }
    
    private LiveSplitState State;
    private ManaTrackerSettings Settings;
    
    private TimeSpan RunStartTime;

    private int Mana,Regen,Potions;

    private int CurrentSplitIndex;

    private bool IsInLoadingScreen;

    private List<ManaSnapshot> ManaSnapshots;
    private List<SpecialEvent> SpecialEvents;

    private Process GameProcess;

    private SimpleLabel HeaderLabel;
    
    private static readonly IntPtr DishonoredOffset = (IntPtr)0x400000;//the offset of the dishonored.exe module, this does not change between versions 

    private int MainOffset;//the offset of the first pointer

    private int ManaOffset;//the offset of the mana value

    private int RegenOffset;//the offset of the regen value

    private const int PotionsOffset1 = 0x59C;//the first offset of the potion count, this does not change between versions
    private const int PotionsOffset2 = 0xD8;//the second offset of the potion count, this does not change between versions
    
    private const int Dishonored12Size = 0x1160000;//size of the dishonored version 1.2 module
    private const int Dishonored14Size = 0x1287000;//size of the dishonored version 1.4 module

    public void Dispose(){
        State.OnStart -= OnRunStart;
        State.OnSplit -= OnRunSplit;
        State.OnSkipSplit -= OnRunSplit;
        State.OnUndoSplit -= OnUndoRunSplit;
    }

    private void OnRunStart(object Sender,EventArgs e){
        if(Settings.p_GraphShow){
            CurrentSplitIndex = 0;
            RunStartTime = new TimeSpan(0);
            ManaSnapshots.Clear();
            SpecialEvents.Clear();
            IsInLoadingScreen = false;
        }
    }

    private void OnRunSplit(object Sender,EventArgs e){
        if(Settings.p_GraphShow && SpecialEvents != null){
            SpecialEvents.Add(new SpecialEvent(SpecialEventType.Split,GetTimeSecondTenths()));
        }
    }

    private void OnUndoRunSplit(object Sender,EventArgs e){
        if(Settings.p_GraphShow && SpecialEvents != null){
            var Index = SpecialEvents.FindLastIndex(x => x.Type == SpecialEventType.Split);
            if(Index != -1) SpecialEvents.RemoveAt(Index);
        }
    }

    private int GetTimeSecondTenths(){
        var CurrentTime = (TimeSpan)State.CurrentTime[TimingMethod.RealTime];
        //dishonored updates mana once every 10th of a second
        return (int)((CurrentTime.Ticks-RunStartTime.Ticks)/1000000);
    }

    private SpecialEvent GetSpecialEvent(ref int i,SpecialEventType Type){ 
        while(i >= 0){
            var Event = SpecialEvents[i--];
            if(Event.Type == Type) return Event;
        }
        return new SpecialEvent(SpecialEventType.Empty,-1);
    }

    private Process GetGameProcess(){
        Mana = Regen = Potions = 0;

        var Window = FindWindow("LaunchUnrealUWindowsClient","Dishonored");
        if(Window == (IntPtr)0) return null;
        int Pid;
        GetWindowThreadProcessId(Window,out Pid);

        Process GameProcess = Process.GetProcessById(Pid);

        switch(GameProcess.MainModule.ModuleMemorySize){
            case Dishonored12Size: {
                MainOffset  = 0xFCCBDC;
                ManaOffset  = 0xA58;
                RegenOffset = 0xFC8;
                break;
            }
            case Dishonored14Size: {
                MainOffset  = 0x1052DE8;
                ManaOffset  = 0xA60;
                RegenOffset = 0x1228;
                break;
            }
            default: return null;
        }

        return GameProcess;
    }

    private bool ReadManaValues(out int Mana,out int Regen,out int Potions){
        Mana = Regen = Potions = -1;

        var Bytes = new byte[sizeof(int)*2];
        if(!ReadProcessMemory(IntPtr.Add(DishonoredOffset,MainOffset),Bytes,(IntPtr)sizeof(int))) return false;
        var MainPtr = (IntPtr)BitConverter.ToInt32(Bytes,0);

        if(!ReadProcessMemory(IntPtr.Add(MainPtr,ManaOffset),Bytes,(IntPtr)(sizeof(int)*2))) return false;
        var NewMana = BitConverter.ToInt32(Bytes,0);
        var MaxMana = BitConverter.ToInt32(Bytes,4);

        if(!ReadProcessMemory(IntPtr.Add(MainPtr,RegenOffset),Bytes,(IntPtr)sizeof(int))) return false;
        var NewRegen = BitConverter.ToInt32(Bytes,0);

        if(!ReadProcessMemory(IntPtr.Add(MainPtr,PotionsOffset1),Bytes,(IntPtr)sizeof(int))) return false;
        var PotionsPtr = (IntPtr)BitConverter.ToInt32(Bytes,0);

        if(!ReadProcessMemory(IntPtr.Add(PotionsPtr,PotionsOffset2),Bytes,(IntPtr)sizeof(int))) return false;

        Mana = NewMana;

        Regen = Math.Min(NewMana+NewRegen,MaxMana);
        Potions = BitConverter.ToInt32(Bytes,0);

        return true;
    }
    
    private bool ReadProcessMemory(IntPtr Address,[Out] byte[] Buffer,IntPtr Size){
        IntPtr BytesRead;
        return ReadProcessMemory(GameProcess.Handle,Address,Buffer,Size,out BytesRead);
    }
    
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(String ClassName,String AppName);
    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr Window,out int Pid);
    [DllImport("kernel32.dll",SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr Process,IntPtr Address,[Out] byte[] Buffer,IntPtr Size,
                                                 out IntPtr BytesRead);
}

}
