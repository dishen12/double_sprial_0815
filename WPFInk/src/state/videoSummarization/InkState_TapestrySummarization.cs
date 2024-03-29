﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using WPFInk.videoSummarization;
using WPFInk.ink;
using WPFInk.tool;
using WPFInk.mouseGesture;
using System.Diagnostics;
using WPFInk.Global;

namespace WPFInk.state
{
    public class InkState_TapestrySummarization : InkState
    {
        #region 私有变量
        private Point _startPoint;
        private Point _currPoint;
        private Point _prePoint;
        private int startIndex = 0;
        private int endIndex = 0;
        private ThumbPlayer thumbPlayer = null;
        int preIndex = int.MinValue;
        Image preImage = null;
        //视频摘要
        private VideoSummarization videoSummarization = null;
        private TapestrySummarization tapestrySummarization = null;
        //WPFInk.videoSummarization.CustomSummarization customSummarization = null;
        private WPFInk.mouseGesture.MouseGesture mouseGesture = null;//鼠标手势识别类
        private Stroke moveStroke;//移动轨迹
        double left = 0;
        double top = 0;
        private KeyFrame hyperLink = null;
        //视频播放变量
        public System.Windows.Forms.Timer VideoPlayTimer;//播放视频过程中的Timer，用于超链接和摘要的显示
        public string videoSource = null;
        private SpiralSummarization hyperLinkSpiralSummarization = null;
        private System.Windows.Forms.Timer hyperLinkPlayTimer;//超链接小视频播放时间控制器
        //记录操作时间变量
        private List<MyStrokeData> trackRecord = new List<MyStrokeData>();
        private MyStrokeData myStrokeData;
        private System.DateTime currentTime = new System.DateTime();
        private System.DateTime downTime;
        private System.DateTime upTime;
        private System.Windows.Forms.Timer MoveTimer;//记录在同一关键帧上停留时间的的timer
        private int moveTimerSecond = 0;//moveTimer计数器
        private int currIndex = 0;
        //private List<KeyFrame> keyFrames;
        public VideoSummarizationControl VideoSummarizationControl;
        private Thickness inkCanvasSpiralSummarizationMargin;
        private KeyFrameAnnotation _keyFrameAnnotation;
        private KeyFramesAnnotation keyFramesAnnotation;
        private Stroke currPlayKeyFrameStroke=null;//视频当前播放对应的帧的stroke
        private TimeBar timebar;
        private string timeTotalString;//视频播放总时间
        #endregion

        #region 封装变量


        public int CurrIndex
        {
            get { return currIndex; }
            set { currIndex = value; }
        }

        public int MoveTimerSecond
        {
            get { return moveTimerSecond; }
            set { moveTimerSecond = value; }
        }
        public string TimeTotalString
        {
            get { return timeTotalString; }
            set { timeTotalString = value; }
        }


        public TimeBar Timebar
        {
            get { return timebar; }
            set { timebar = value; }
        }

        public TapestrySummarization TapestrySummarization
        {
            get { return tapestrySummarization; }
            set { tapestrySummarization = value; }
        }
        public VideoSummarization VideoSummarization
        {
            get { return videoSummarization; }
            set { videoSummarization = value; }
        }
        public List<MyStrokeData> TrackRecord
        {
            get { return trackRecord; }
            set { trackRecord = value; }
        }
        #endregion

        #region 构造函数
        public InkState_TapestrySummarization(InkCollector inkCollector)
            : base(inkCollector)
        {
            this._inkCollector = inkCollector;
            MoveTimer = new System.Windows.Forms.Timer();
            MoveTimer.Interval = 1000;
            MoveTimer.Tick += new EventHandler(MoveTimer_Tick); 
        }

        void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (_startPoint.X == 0 && _startPoint.Y == 0)// && _inkCollector.KeyFrameAnnotation.Visibility == Visibility.Collapsed)
            {
                moveTimerSecond += 1;
                if (moveTimerSecond >= 3 && currIndex != int.MinValue && currIndex < videoSummarization.ShowKeyFrames.Count
                    && _inkCollector.IsShowUnbrokenKeyFrame && !tapestrySummarization.IsZoomIng)
                {
                    if (preImage != null)
                    {
                        _inkCanvas.Children.Remove(preImage);
                    }
                    TransformGroup tg = (TransformGroup)(videoSummarization.ShowKeyFrames[currIndex].Image.RenderTransform);
                    double x = tg.Children.Count > 0 ? ((TranslateTransform)(tg.Children[0])).X : 0;
                    left = videoSummarization.ShowKeyFrames[currIndex].Image.Margin.Left+ x+ 
                        videoSummarization.InkCanvas.Margin.Left;
                    top = videoSummarization.InkCanvas.Margin.Top+(currIndex%2==0?-40:(videoSummarization.ShowHeight+50));
                    _inkCollector.SelectKeyFrames.Add(videoSummarization.KeyFrames[currIndex]);
                    int videoTime = videoSummarization.ShowKeyFrames[currIndex].Time;
                    //在关键帧上画圆，显示小视频                        
                    _inkCollector._mainPage._thumbVideoPlayer.Margin = new Thickness(left, top, 0, 0);
                    _inkCollector._mainPage._thumbVideoPlayer.Visibility = Visibility.Visible;
                    _inkCollector._mainPage._thumbVideoPlayer.InitVideoPlayer(videoSummarization.ShowKeyFrames[currIndex].VideoName, videoTime, 5000, true);
                    _inkCollector._mainPage._thumbVideoPlayer.videoPlayer.MouseLeftButtonUp += new MouseButtonEventHandler(thumbVideoPlayer_MouseLeftButtonUp);

                    //显示关键帧注释
                    KeyFrame selectKeyFrame= videoSummarization.ShowKeyFrames[currIndex];
                    Dictionary<Stroke, KeyFramesAnnotation> s = selectKeyFrame.Annotations;
                    if (s.Count > 0)
                    {
                        _keyFrameAnnotation = new KeyFrameAnnotation();
                        KeyValuePair<Stroke, KeyFramesAnnotation> currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in selectKeyFrame.Annotations
                                                                              //where anno.Value == _keyFramesAnnotation
                                                                              select anno).First();
                        keyFramesAnnotation = (KeyFramesAnnotation)(currPair.Value);
                        foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                        {
                            currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in videoSummarization.ShowKeyFrames[index].Annotations
                                        where anno.Value == keyFramesAnnotation
                                        select anno).First();
                            Stroke stroke = (Stroke)(currPair.Key);
                            if (_inkCanvas.Strokes.IndexOf(stroke) == -1)
                            {
                                _inkCanvas.Strokes.Add(stroke);
                            }
                        }
                        _keyFrameAnnotation.InkCanvasAnnotation.Strokes.Add(keyFramesAnnotation.Strokes);
                        //显示关键帧注释
                        _keyFrameAnnotation.Width = keyFramesAnnotation.Width;
                        _keyFrameAnnotation.Height = keyFramesAnnotation.Height;
                        _keyFrameAnnotation.VerticalAlignment = VerticalAlignment.Top;
                        if (videoSummarization.ShowKeyFrameCenterPoints[startIndex].X < videoSummarization.Center.X)
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        else
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Right;
                        }
                        _keyFrameAnnotation.Visibility = Visibility.Visible;
                        _inkCollector._mainPage.LayoutRoot.Children.Add(_keyFrameAnnotation);
                    }

                    moveTimerSecond = 0;
                }
            }
        }
        public void VideoPlayTimer_Tick(object sender, EventArgs e)
        {
            int videoTimeNow = (int)VideoSummarizationControl.mediaPlayer.Position.TotalMilliseconds;
            //修改时间轴和播放时间
            VideoSummarizationControl._timeBar.Value = videoTimeNow;

            //修改显示播放进度的值的textbox
            List<string> timeCurr = new List<string>();
            timeCurr = ConvertClass.getInstance().MsToHMS(videoTimeNow);
            VideoSummarizationControl.VideoProgressText.Text = timeCurr[0] + ":" + timeCurr[1] + ":" + timeCurr[2] + "/" + timeTotalString;

            //显示超链接关键帧
            foreach (KeyFrame keyFrame in _inkCollector.HyperLinkKeyFrames)
            {
                if (keyFrame.VideoName == videoSource && (int)(keyFrame.Time / 1000) == (int)(videoTimeNow / 1000))
                {
                    
                    if (hyperLink != null)
                    {
                        VideoSummarizationControl.TableGrid.Children.Remove(hyperLink.Image);
                    }
                    hyperLink = keyFrame.HyperLink;
                    VideoSummarizationControl.hyperLinkPlayer.Visibility = Visibility.Visible;
                    VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.hyperLinkPlayer, hyperLink);
                    hyperLinkPlayTimer = new System.Windows.Forms.Timer();
                    hyperLinkPlayTimer.Interval = 5000;
                    hyperLinkPlayTimer.Tick += new System.EventHandler(hyperLinkPlayTimer_Tick);
                    hyperLinkPlayTimer.Start();
                    hyperLinkSpiralSummarization = keyFrame.HyperLinkSpiralSummarization;
                    VideoSummarizationControl.hyperLinkPlayer.MouseLeftButtonUp += new MouseButtonEventHandler(hyperLinkPlayer_MouseLeftButtonUp);
                    

                    break;
                }
            }
            
            bool isHasAnnotation = false;
            bool isHasKeyFrame = false;
            foreach (KeyFrame keyFrame in videoSummarization.KeyFrames)
            {
                if ((int)(keyFrame.Time / 1000) == (int)(videoTimeNow / 1000))
                {
                    if (_inkCollector.DefaultSummarizationNum == 0)
                    {
                        //在螺旋摘要中修改螺旋线表明播放到当前帧了
                        int currIndex = videoSummarization.KeyFrames.IndexOf(keyFrame);
                        if (currPlayKeyFrameStroke != null)
                        {
                            _inkCollector.VideoSummarization.InkCanvas.Strokes.Remove(currPlayKeyFrameStroke);
                        }
                        currPlayKeyFrameStroke = _inkCollector.VideoSummarization.AddPoint2Track(currIndex, Colors.Red, 8);
                    }
                    //显示草图注释
                    isHasKeyFrame = true;
                    Dictionary<Stroke, KeyFramesAnnotation> s = keyFrame.Annotations;
                    if (s.Count > 0)
                    {
                        KeyValuePair<Stroke, KeyFramesAnnotation> currPair;
                        if (_keyFrameAnnotation != null && keyFramesAnnotation != null)
                        {
                            _keyFrameAnnotation.Visibility = Visibility.Collapsed;
                            _inkCollector._mainPage.LayoutRoot.Children.Remove(_keyFrameAnnotation);
                            _keyFrameAnnotation = null;
                            foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                            {
                                currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in _inkCollector.VideoSummarization.ShowKeyFrames[index].Annotations
                                                                                      where anno.Value == keyFramesAnnotation
                                                                                      select anno).First();
                                Stroke linkline = (Stroke)(currPair.Key);
                                _inkCollector._mainPage._inkCanvas.Strokes.Remove(linkline);
                            }
                        }
                        _keyFrameAnnotation = new KeyFrameAnnotation();
                        currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in keyFrame.Annotations
                                                                              //where anno.Value == _keyFramesAnnotation
                                                                              select anno).First();
                        keyFramesAnnotation = (KeyFramesAnnotation)(currPair.Value);
                        int firstIndex = 0;
                        int count = 0;
                        foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                        {
                            if (count == 0)
                            {
                                firstIndex = index;
                            }
                            count++;
                            currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in videoSummarization.ShowKeyFrames[index].Annotations
                                        where anno.Value == keyFramesAnnotation
                                        select anno).First();
                            Stroke stroke = (Stroke)(currPair.Key);
                            if (_inkCanvas.Strokes.IndexOf(stroke) == -1)
                            {
                                _inkCanvas.Strokes.Add(stroke);
                            }

                        }
                        _keyFrameAnnotation.InkCanvasAnnotation.Strokes.Add(keyFramesAnnotation.Strokes);
                        //显示关键帧注释
                        _keyFrameAnnotation.Width = keyFramesAnnotation.Width;
                        _keyFrameAnnotation.Height = keyFramesAnnotation.Height;
                        _keyFrameAnnotation.VerticalAlignment = VerticalAlignment.Top;
                        if (videoSummarization.ShowKeyFrameCenterPoints[firstIndex].X < videoSummarization.Center.X)
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        else
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Right;
                        }
                        _keyFrameAnnotation.Visibility = Visibility.Visible;
                        _inkCollector._mainPage.LayoutRoot.Children.Add(_keyFrameAnnotation);

                        //VideoSummarizationControl.BtnSpiralScreenBack.Visibility = Visibility.Collapsed;
                        //VideoSummarizationControl.BtnSpiralScreen.Visibility = Visibility.Collapsed;
                        isHasAnnotation = true;
                    }
                    break;
                }
            }
            if (isHasKeyFrame&&!isHasAnnotation&&_inkCollector.Mode!=InkMode.AddKeyFrameAnnotation)
            {
                KeyValuePair<Stroke, KeyFramesAnnotation> currPair;
                if (_keyFrameAnnotation != null && keyFramesAnnotation != null)
                {
                    _keyFrameAnnotation.Visibility = Visibility.Collapsed;
                    foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                    {
                        currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in _inkCollector.VideoSummarization.ShowKeyFrames[index].Annotations
                                    where anno.Value == keyFramesAnnotation
                                    select anno).First();
                        Stroke linkline = (Stroke)(currPair.Key);
                        _inkCollector._mainPage._inkCanvas.Strokes.Remove(linkline);
                    }
                }
            }
        }
        #endregion
          
        #region 事件
        public override void _presenter_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _inkCanvas.CaptureMouse();
            _startPoint = e.GetPosition(_inkCanvas);
            _prePoint = _startPoint;
            //记录操作类型与持续时间
            recordOperateTrace("DOWN");
            downTime = System.DateTime.Now;
            startIndex = VideoSummarization.getSelectedKeyFrameIndex(_startPoint);//, videoSummarization);
            if (startIndex==int.MinValue&&mouseGesture == null)
            {
                createGesture();
            }
            if (startIndex == int.MinValue)
            {
                mouseGesture.StartCapture((int)_startPoint.X, (int)_startPoint.Y);
            }
            if (moveStroke != null)
            {
                _inkCanvas.Strokes.Remove(moveStroke);
                moveStroke = null;
            }

        }



        public override void _presenter_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.VideoSummarizationControl == null)
            {
                this.VideoSummarizationControl = _inkCollector._mainPage.VideoSummarizationControl;
            }
            if (_startPoint.X == 0 && _startPoint.Y == 0)
            {
                _currPoint = e.GetPosition(_inkCanvas);
                _currPoint.X -= inkCanvasSpiralSummarizationMargin.Left;
                _currPoint.Y -= inkCanvasSpiralSummarizationMargin.Top;
                currIndex = videoSummarization.getSelectedKeyFrameIndex(_currPoint);
                //记录操作类型与持续时间
                recordOperateTrace("MOVE");
                if (currIndex != int.MinValue && currIndex != preIndex&&_inkCollector.IsShowUnbrokenKeyFrame)
                {
                    moveTimerSecond = 0;
                    MoveTimer.Start();
                    clearPreMessage();
                    currIndex = currIndex >= videoSummarization.ShowKeyFrames.Count ? videoSummarization.ShowKeyFrames.Count - 1 : currIndex;
                    currIndex = (currIndex == -1 ? 0 : currIndex);
                    Image currImage = InkConstants.getImageFromName(videoSummarization.ShowKeyFrames[currIndex].ImageName);
                    currImage.Width = 300;
                    currImage.Height = 200;
                    currImage.Margin = new Thickness((VideoSummarizationControl.summarization._inkCanvas.ActualWidth-currImage.Width)/2,
                         200, 0, 0);
                    tapestrySummarization.ParentInkcanvas.Children.Add(currImage);
                    preImage = currImage;
                    preIndex = currIndex;
                }
                else if (currIndex == int.MinValue)
                {
                    if (preImage != null)
                    {
                        _inkCanvas.Children.Remove(preImage);
                    }
                    moveTimerSecond = 0;
                }
                //显示移动轨迹
                if (moveStroke != null)
                {
                    moveStroke.StylusPoints.Add(new StylusPoint(_currPoint.X, _currPoint.Y));
                    if (moveStroke.StylusPoints.Count > 300)
                    {
                        moveStroke.StylusPoints.RemoveAt(0);
                    }
                }
                else
                {
                    StylusPointCollection spc = new StylusPointCollection();
                    spc.Add(new StylusPoint(_currPoint.X, _currPoint.Y));
                    moveStroke = new Stroke(spc);
                    moveStroke.DrawingAttributes.Color = Colors.Transparent;
                    moveStroke.DrawingAttributes.Width = 3;
                    moveStroke.DrawingAttributes.Height = 3;
                    _inkCanvas.Strokes.Add(moveStroke);
                }
            }
            else
            {
                _currPoint = e.GetPosition(_inkCanvas);
                if (startIndex != int.MinValue && _currPoint.X>0)
                {
                    left = videoSummarization.InkCanvas.Margin.Left + _currPoint.X - _prePoint.X;
                    //if (left <= 0 && left >VideoSummarizationControl.summarization._inkCanvas.ActualWidth -tapestrySummarization.Width  )
                    //{
                    videoSummarization.InkCanvas.Margin = new Thickness(left,
                        videoSummarization.InkCanvas.Margin.Top, 0, 0);
                    //}
                    if (left > 0)
                    {
                        left = 0;
                        timebar.Show_EndTime = VideoSummarizationControl.summarization._inkCanvas.ActualWidth-videoSummarization.InkCanvas.Margin.Left;
                    }
                    else if (left < -tapestrySummarization.Width + VideoSummarizationControl.summarization._inkCanvas.ActualWidth)
                    {
                        left = videoSummarization.InkCanvas.Margin.Left;
                        timebar.Show_EndTime = -left + tapestrySummarization.Width + videoSummarization.InkCanvas.Margin.Left;
                    }
                    else
                    {
                        timebar.Show_EndTime = -left + VideoSummarizationControl.summarization._inkCanvas.ActualWidth;
                    }
                    timebar.Show_StartTime = -left;
                   // timebar.Show_EndTime = -left + VideoSummarizationControl.summarization._inkCanvas.ActualWidth;
                    timebar.computeLocation();
                    _prePoint = _currPoint;
                }
                if (startIndex == int.MinValue && mouseGesture != null)
                {
                    mouseGesture.Capturing((int)_currPoint.X, (int)_currPoint.Y);
                }
                //记录操作类型与持续时间
                recordOperateTrace("MOVE");
            }

        }
        public override void _presenter_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (videoSummarization != null && _startPoint != null && _inkCanvas.Strokes.Count>0)
            {
                Stroke lastStroke = _inkCanvas.Strokes.Last();
                //先清空原来选中的关键帧序列
                clearPreMessage();
                _currPoint = e.GetPosition(_inkCanvas);
                _currPoint.X -= inkCanvasSpiralSummarizationMargin.Left;
                _currPoint.Y -= inkCanvasSpiralSummarizationMargin.Top;
                endIndex = videoSummarization.getSelectedKeyFrameIndex(_currPoint);
                //记录操作类型与持续时间
                recordOperateTrace("UP");
                upTime = System.DateTime.Now;

                if (lastStroke.StylusPoints.Count == 1)
                {
                    if (startIndex != int.MinValue && startIndex == endIndex&&_inkCollector.IsShowUnbrokenKeyFrame)
                    {
                        if (null != mouseGesture)
                        {
                            mouseGesture.Points.Clear();
                        }

                        if (GlobalValues.locationQuestions.Count > 0)
                        {
                            if (GlobalValues.CurrLocationId + 1 < GlobalValues.locationQuestions.Count)
                            {
                                if (GlobalValues.locationAnswers[GlobalValues.CurrLocationId].IndexOf(videoSummarization.KeyFrames.IndexOf(videoSummarization.ShowKeyFrames[startIndex]) + 1) != -1)
                                {
                                    GlobalValues.CurrLocationId++;
                                    GlobalValues.LocationQuestion.setQuestion(GlobalValues.locationQuestions[GlobalValues.CurrLocationId]);
                                    VideoSummarizationControl.RecordCurrOperationAndTime("第" + (GlobalValues.CurrLocationId - 1).ToString() +
                                        "次定位正确");
                                    GlobalValues.LocationQuestion.TBQuestion.Background =new SolidColorBrush(GlobalValues.LocationQuestionBackGrouds[GlobalValues.CurrLocationId]);
                                }
                            }
                            else
                            {
                                VideoSummarizationControl.RecordCurrOperationAndTime("最后一次定位正确");
                                switch (GlobalValues.summarizationTypeNo)
                                {
                                    case 0:
                                        VideoSummarizationControl.Record("Spiral");
                                        break;
                                    case 1:
                                        VideoSummarizationControl.Record("Tile");
                                        break;
                                    case 2:
                                        VideoSummarizationControl.Record("Tapestry");
                                        break;
                                }
                                MessageBox.Show("恭喜你完成此项任务！");
                                VideoSummarizationControl.sWriter.Close();
                                VideoSummarizationControl.myStream.Close();
                                Environment.Exit(1);
                            }
                        }
                        //定位视频
                        if (VideoSummarizationControl != null)
                        {
                            //定位视频，position的单位是秒      
                            videoSource = videoSummarization.ShowKeyFrames[startIndex].VideoName;
                            VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.mediaPlayer, videoSummarization.ShowKeyFrames[startIndex]);
                            //显示超链接
                            if (hyperLink != null)
                            {
                                VideoSummarizationControl.TableGrid.Children.Remove(hyperLink.Image);
                            }
                            hyperLink = videoSummarization.ShowKeyFrames[startIndex].HyperLink;
                            if (hyperLink != null)
                            {
                                VideoSummarizationControl.hyperLinkPlayer.Visibility = Visibility.Visible;
                                VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.hyperLinkPlayer, hyperLink);
                                hyperLinkSpiralSummarization = videoSummarization.ShowKeyFrames[startIndex].HyperLinkSpiralSummarization;
                                VideoSummarizationControl.hyperLinkPlayer.MouseLeftButtonUp += new MouseButtonEventHandler(hyperLinkPlayer_MouseLeftButtonUp);
                    
                            }
                            //记录操作事件与持续时间
                            recordOperateEvent("locate");
                        }
                    }
                }
                else
                {
                    if (startIndex==int.MinValue&& mouseGesture != null)
                    {
                        mouseGesture.StopCapture();
                    }
                }
                _startPoint.X = 0;
                _startPoint.Y = 0;
                _inkCanvas.Strokes.Remove(lastStroke);

            }
        }
        #endregion 

        #region 成员函数
        private void hyperLinkPlayTimer_Tick(object sender, EventArgs e)
        {
            hyperLinkPlayTimer.Stop();
            VideoSummarizationControl.hyperLinkPlayer.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// //记录操作类型与持续时间
        /// </summary>
        private void recordOperateTrace(string OperateType)
        {
            myStrokeData = new MyStrokeData();
            myStrokeData.Point = _currPoint;
            currentTime = System.DateTime.Now;
            myStrokeData.CurrentTime = currentTime.ToString("s") + ":" + currentTime.Millisecond.ToString();
            myStrokeData.OperateType = OperateType;
            trackRecord.Add(myStrokeData);
        }

        /// <summary>
        /// 单击超链接图片，视频进行跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void hyperLinkPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.mediaPlayer,hyperLink);
            videoSummarization.hideSummarization();
            hyperLinkSpiralSummarization.showSpiralSummarization();
            _inkCollector.VideoSummarization = hyperLinkSpiralSummarization;
            videoSummarization = hyperLinkSpiralSummarization;
            //_inkCollector.KeyFrames = _inkCollector.VideoSummarization.KeyFrames;
            _inkCollector.VideoSummarization.InkCanvas.Margin = new Thickness(280, 0, 0, 0);
            videoSummarization = hyperLinkSpiralSummarization;
            VideoSummarizationControl.hyperLinkPlayer.Visibility = Visibility.Collapsed;

        }
        /// <summary>
        /// 记录操作事件与持续时间
        /// </summary>
        /// <param name="operateEvent"></param>
        private void recordOperateEvent(string operateEvent)
        {
            myStrokeData = new MyStrokeData();
            myStrokeData.CurrentTime = (upTime - downTime).ToString();
            myStrokeData.OperateType = operateEvent;
            trackRecord.Add(myStrokeData);
        }
        /// <summary>
        /// 清除原来的事件和操作信息
        /// </summary>
        private void clearPreMessage()
        {
            _inkCollector.SelectKeyFrames.Clear();
            if (thumbPlayer != null)
            {
                _inkCanvas.Children.Remove(thumbPlayer.VideoPlayer);
            }
            //_inkCollector.KeyFrameAnnotation.InkCanvasAnnotation.Strokes.Clear();
            //_inkCollector.KeyFrameAnnotation.Visibility = Visibility.Collapsed;
            _inkCollector._mainPage._thumbVideoPlayer.videoPlayer.Source = null;
            _inkCollector._mainPage._thumbVideoPlayer.Visibility = Visibility.Collapsed;
            if (preImage != null)
            {
                _inkCanvas.Children.Remove(preImage);
            }
            //清除注释框
            if (_keyFrameAnnotation != null && keyFramesAnnotation != null)
            {
                _keyFrameAnnotation.Visibility = Visibility.Collapsed;
                //记录宽度和高度
                //_keyFramesAnnotation.Width = this.Width;
                //_keyFramesAnnotation.Height = this.Height;
                foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                {
                    KeyValuePair<Stroke, KeyFramesAnnotation> currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in _inkCollector.VideoSummarization.ShowKeyFrames[index].Annotations
                                                                          where anno.Value == keyFramesAnnotation
                                                                          select anno).First();
                    Stroke linkline = (Stroke)(currPair.Key);
                    _inkCollector._mainPage._inkCanvas.Strokes.Remove(linkline);
                }
                //if (VideoSummarizationControl.IsSpiralScreen)
                //{
                //    VideoSummarizationControl.BtnSpiralScreenBack.Visibility = Visibility.Visible;
                //    VideoSummarizationControl.BtnSpiralScreen.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    VideoSummarizationControl.BtnSpiralScreenBack.Visibility = Visibility.Collapsed;
                //    VideoSummarizationControl.BtnSpiralScreen.Visibility = Visibility.Visible;
                //}
            }
        }
        

        /// <summary>
        /// 创建手势库
        /// </summary>
        public void createGesture()
        {
            mouseGesture = new WPFInk.mouseGesture.MouseGesture();
            //显示注释
            mouseGesture.AddGesture("keyFrameAnnotation", "0", null);
            //摘要全屏
            mouseGesture.AddGesture("SpiralFullScreen", "4", null);
            mouseGesture.GestureMatchEvent += new WPFInk.mouseGesture.MouseGesture.GestureMatchDelegate(gesture_GestureMatchEvent);
        }
        /// <summary>
        /// 方向和笔序识别结果匹配和处理
        /// </summary>
        /// <param name="args"></param>
        void gesture_GestureMatchEvent(MouseGestureEventArgs args)
        {
            //记录操作事件与持续时间
            recordOperateEvent(args.Present);
            switch (args.Present)
            {               
                case "keyFrameAnnotation":
                    if (startIndex != int.MinValue && startIndex == endIndex && _inkCollector.IsShowUnbrokenKeyFrame)
                    {
                        _keyFrameAnnotation = new KeyFrameAnnotation();
                        KeyFrame selectKeyFrame = videoSummarization.ShowKeyFrames[startIndex];
                        _inkCollector.SelectKeyFrames.Add(videoSummarization.ShowKeyFrames[startIndex]);
                        Dictionary<Stroke, KeyFramesAnnotation> s = selectKeyFrame.Annotations;
                        //在已经有注释的情况下显示已有的注释
                        if (selectKeyFrame.Annotations.Count > 0)
                        {
                            KeyValuePair<Stroke, KeyFramesAnnotation> currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in selectKeyFrame.Annotations
                                                                                  //where anno.Value == _keyFramesAnnotation
                                                                                  select anno).First();
                            keyFramesAnnotation = (KeyFramesAnnotation)(currPair.Value);
                            foreach (int index in keyFramesAnnotation.relatedKeyFrameIndexes)
                            {
                                currPair = (from KeyValuePair<Stroke, KeyFramesAnnotation> anno in videoSummarization.ShowKeyFrames[index].Annotations
                                            where anno.Value == keyFramesAnnotation
                                            select anno).First();
                                Stroke stroke = (Stroke)(currPair.Key);
                                _inkCanvas.Strokes.Add(stroke);
                            }
                            _keyFrameAnnotation.InkCanvasAnnotation.Strokes.Add(keyFramesAnnotation.Strokes);
                        }
                        else
                        {
                            keyFramesAnnotation = new KeyFramesAnnotation();
                        }
                        _inkCollector.KeyFramesAnnotation = keyFramesAnnotation;
                        _inkCollector.KeyFrameAnnotation = _keyFrameAnnotation;
                        _keyFrameAnnotation.setInkCollector(_inkCollector);
                        _keyFrameAnnotation.setKeyFramesAnnotation(keyFramesAnnotation,true);
                        //显示关键帧注释
                        _keyFrameAnnotation.Width = keyFramesAnnotation.Width;
                        _keyFrameAnnotation.Height = keyFramesAnnotation.Height;
                        _keyFrameAnnotation.VerticalAlignment = VerticalAlignment.Top;
                        Stroke linkLine;
                        if (videoSummarization.ShowKeyFrameCenterPoints[startIndex].X < videoSummarization.Center.X)
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Left;
                            if (selectKeyFrame.Annotations.Count == 0)
                            {//画关键帧到注释框的连线
                                linkLine = InkTool.getInstance().DrawLine(videoSummarization.ShowKeyFrameCenterPoints[startIndex].X + inkCanvasSpiralSummarizationMargin.Left,
                                    videoSummarization.ShowKeyFrameCenterPoints[startIndex].Y + inkCanvasSpiralSummarizationMargin.Top,
                                    _keyFrameAnnotation.Margin.Left + _keyFrameAnnotation.Width / 2,
                                    _keyFrameAnnotation.Margin.Top + _keyFrameAnnotation.Height / 2,
                                    _inkCanvas, Color.FromArgb(180, 0, 255, 0));
                                selectKeyFrame.Annotations.Add(linkLine, keyFramesAnnotation);
                                keyFramesAnnotation.relatedKeyFrameIndexes.Add(startIndex);
                                MoveTimer.Stop();
                            }
                        }
                        else
                        {
                            _keyFrameAnnotation.HorizontalAlignment = HorizontalAlignment.Right;
                            if (selectKeyFrame.Annotations.Count == 0)
                            {
                                linkLine = InkTool.getInstance().DrawLine(videoSummarization.ShowKeyFrameCenterPoints[startIndex].X + inkCanvasSpiralSummarizationMargin.Left,
                                    videoSummarization.ShowKeyFrameCenterPoints[startIndex].Y + inkCanvasSpiralSummarizationMargin.Top,
                                    _inkCanvas.ActualWidth - _keyFrameAnnotation.Width / 2,
                                    _keyFrameAnnotation.Margin.Top + _keyFrameAnnotation.Height / 2,
                                    _inkCanvas, Color.FromArgb(180, 0, 255, 0));
                                selectKeyFrame.Annotations.Add(linkLine, keyFramesAnnotation);
                                keyFramesAnnotation.relatedKeyFrameIndexes.Add(startIndex);
                                MoveTimer.Stop();
                            }
                        }
                        _keyFrameAnnotation.Visibility = Visibility.Visible;
                        _inkCollector._mainPage.LayoutRoot.Children.Add(_keyFrameAnnotation);
                        _inkCollector.Mode = InkMode.AddKeyFrameAnnotation;
                    }
                    else
                    {
                        //VideoSummarizationControl.TableGrid.ColumnDefinitions[0].Width = new GridLength(VideoSummarizationControl.TableGrid.ActualWidth * 0.25);
                       
                    }
                    break;
                case "SpiralFullScreen":
                    //VideoSummarizationControl.TableGrid.ColumnDefinitions[0].Width = new GridLength(0);
                    break;
                default:
                    //Console.WriteLine("default");
                    break;
            }
        }
        
        private void thumbVideoPlayer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (startIndex==int.MinValue)
            {
                VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.mediaPlayer, videoSummarization.ShowKeyFrames[currIndex]);                           

            } 
            else
            {
                VideoSummarizationTool.locateMediaPlayer(VideoSummarizationControl.mediaPlayer, videoSummarization.ShowKeyFrames[startIndex]);           
            }
                            

        }
        #endregion
    }
}
