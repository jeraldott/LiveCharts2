﻿// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using LiveChartsCore.Kernel;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore.Measure;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel.Helpers;

namespace LiveChartsCore
{
    /// <summary>
    /// Defines an Axis in a Cartesian chart.
    /// </summary>
    /// <typeparam name="TDrawingContext">The type of the drawing context.</typeparam>
    /// <typeparam name="TTextGeometry">The type of the text geometry.</typeparam>
    /// <typeparam name="TLineGeometry">The type of the line geometry.</typeparam>
    public abstract class Axis<TDrawingContext, TTextGeometry, TLineGeometry>
        : ChartElement<TDrawingContext>, ICartesianAxis, IPlane<TDrawingContext>
            where TDrawingContext : DrawingContext
            where TTextGeometry : ILabelGeometry<TDrawingContext>, new()
            where TLineGeometry : ILineGeometry<TDrawingContext>, new()
    {
        #region fields

        /// <summary>
        /// The active separators
        /// </summary>
        protected readonly Dictionary<IChart, Dictionary<double, AxisVisualSeprator<TDrawingContext>>> activeSeparators = new();

        internal AxisOrientation _orientation;
        private double _minStep = 0;
        private Bounds? _dataBounds = null;
        private Bounds? _visibleDataBounds = null;
        private double _labelsRotation;
        // xo (x origin) and yo (y origin) are the distance to the center of the axis to the control bounds
        internal float _xo = 0f, _yo = 0f;
        private TTextGeometry? _nameGeometry;
        private AxisPosition _position = AxisPosition.Start;
        private Func<double, string> _labeler = Labelers.Default;
        private Padding _padding = Padding.Default;
        private double? _minLimit = null;
        private double? _maxLimit = null;
        private IPaint<TDrawingContext>? _namePaint;
        private double _nameTextSize = 20;
        private Padding _namePadding = new(5);
        private IPaint<TDrawingContext>? _labelsPaint;
        private double _unitWidth = 1;
        private double _textSize = 16;
        private IPaint<TDrawingContext>? _separatorsPaint;
        private bool _showSeparatorLines = true;
        private bool _isVisible = true;
        private bool _isInverted;
        private bool _forceStepToMin;

        #endregion

        #region properties

        float ICartesianAxis.Xo { get => _xo; set => _xo = value; }
        float ICartesianAxis.Yo { get => _yo; set => _yo = value; }

        Bounds? IPlane.PreviousDataBounds { get; set; }

        Bounds? IPlane.PreviousVisibleDataBounds { get; set; }

        double? IPlane.PreviousMaxLimit { get; set; }

        double? IPlane.PreviousMinLimit { get; set; }

        Bounds IPlane.DataBounds => _dataBounds ?? throw new Exception("bounds not found");

        Bounds IPlane.VisibleDataBounds => _visibleDataBounds ?? throw new Exception("bounds not found");

        /// <inheritdoc cref="IPlane.Name"/>
        public string? Name { get; set; } = null;

        /// <inheritdoc cref="IPlane.NameTextSize"/>
        public double NameTextSize { get => _nameTextSize; set { _nameTextSize = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.NamePadding"/>
        public Padding NamePadding { get => _namePadding; set { _namePadding = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="ICartesianAxis.Orientation"/>
        public AxisOrientation Orientation => _orientation;

        /// <inheritdoc cref="ICartesianAxis.Padding"/>
        public Padding Padding { get => _padding; set { _padding = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.Labeler"/>
        public Func<double, string> Labeler { get => _labeler; set { _labeler = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.MinStep"/>
        public double MinStep { get => _minStep; set { _minStep = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.ForceStepToMin"/>
        public bool ForceStepToMin { get => _forceStepToMin; set { _forceStepToMin = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.MinLimit"/>
        public double? MinLimit { get => _minLimit; set { _minLimit = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.MaxLimit"/>
        public double? MaxLimit { get => _maxLimit; set { _maxLimit = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.UnitWidth"/>
        public double UnitWidth { get => _unitWidth; set { _unitWidth = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="ICartesianAxis.Position"/>
        public AxisPosition Position { get => _position; set { _position = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.LabelsRotation"/>
        public double LabelsRotation { get => _labelsRotation; set { _labelsRotation = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.TextSize"/>
        public double TextSize { get => _textSize; set { _textSize = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.Labels"/>
        public IList<string>? Labels { get; set; }

        /// <inheritdoc cref="IPlane.ShowSeparatorLines"/>
        public bool ShowSeparatorLines { get => _showSeparatorLines; set { _showSeparatorLines = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.IsVisible"/>
        public bool IsVisible { get => _isVisible; set { _isVisible = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane.IsInverted"/>
        public bool IsInverted { get => _isInverted; set { _isInverted = value; OnPropertyChanged(); } }

        /// <inheritdoc cref="IPlane{TDrawingContext}.NamePaint"/>
        public IPaint<TDrawingContext>? NamePaint
        {
            get => _namePaint;
            set => SetPaintProperty(ref _namePaint, value);
        }

        /// <inheritdoc cref="IPlane{TDrawingContext}.LabelsPaint"/>
        public IPaint<TDrawingContext>? LabelsPaint
        {
            get => _labelsPaint;
            set => SetPaintProperty(ref _labelsPaint, value);
        }

        /// <inheritdoc cref="IPlane{TDrawingContext}.SeparatorsPaint"/>
        public IPaint<TDrawingContext>? SeparatorsPaint
        {
            get => _separatorsPaint;
            set => SetPaintProperty(ref _separatorsPaint, value);
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Renamed to LabelsPaint")]
        public IPaint<TDrawingContext>? TextBrush { get => LabelsPaint; set => LabelsPaint = value; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Renamed to SeparatorsPaint")]
        public IPaint<TDrawingContext>? SeparatorsBrush { get => SeparatorsPaint; set => SeparatorsPaint = value; }

        /// <inheritdoc cref="IPlane.AnimationsSpeed"/>
        public TimeSpan? AnimationsSpeed { get; set; }

        /// <inheritdoc cref="IPlane.EasingFunction"/>
        public Func<float, float>? EasingFunction { get; set; }

        /// <inheritdoc cref="IStopNPC.IsNotifyingChanges"/>
        bool IStopNPC.IsNotifyingChanges { get; set; }

        #endregion

        /// <inheritdoc cref="ICartesianAxis.Initialized"/>
        public event Action<ICartesianAxis>? Initialized;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <returns></returns>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc cref="ChartElement{TDrawingContext}.Measure(Chart{TDrawingContext})"/>
        public override void Measure(Chart<TDrawingContext> chart)
        {
            var cartesianChart = (CartesianChart<TDrawingContext>)chart;

            if (_dataBounds is null) throw new Exception("DataBounds not found");

            var controlSize = cartesianChart.ControlSize;
            var drawLocation = cartesianChart.DrawMarginLocation;
            var drawMarginSize = cartesianChart.DrawMarginSize;

            var scale = new Scaler(drawLocation, drawMarginSize, this);
            var previousSacale = ((ICartesianAxis)this).PreviousDataBounds is null
                ? null
                : new Scaler(drawLocation, drawMarginSize, this, true);
            var axisTick = ((ICartesianAxis)this).GetTick(drawMarginSize);

            var labeler = Labeler;
            if (Labels is not null)
            {
                labeler = Labelers.BuildNamedLabeler(Labels).Function;
                _minStep = 1;
            }

            var s = axisTick.Value;
            if (s < _minStep) s = _minStep;
            if (_forceStepToMin) s = _minStep;

            if (NamePaint is not null)
            {
                if (NamePaint.ZIndex == 0) NamePaint.ZIndex = -1;
                cartesianChart.Canvas.AddDrawableTask(NamePaint);
            }
            if (LabelsPaint is not null)
            {
                if (LabelsPaint.ZIndex == 0) LabelsPaint.ZIndex = -0.9;
                cartesianChart.Canvas.AddDrawableTask(LabelsPaint);
            }
            if (SeparatorsPaint is not null)
            {
                if (SeparatorsPaint.ZIndex == 0) SeparatorsPaint.ZIndex = -1;
                SeparatorsPaint.SetClipRectangle(cartesianChart.Canvas, new LvcRectangle(drawLocation, drawMarginSize));
                cartesianChart.Canvas.AddDrawableTask(SeparatorsPaint);
            }

            var lyi = drawLocation.Y;
            var lyj = drawLocation.Y + drawMarginSize.Height;
            var lxi = drawLocation.X;
            var lxj = drawLocation.X + drawMarginSize.Width;

            float xoo = 0f, yoo = 0f;

            if (_orientation == AxisOrientation.X)
            {
                yoo = _position == AxisPosition.Start
                     ? controlSize.Height - _yo
                     : _yo;
            }
            else
            {
                xoo = _position == AxisPosition.Start
                    ? _xo
                    : controlSize.Width - _xo;
            }

            var size = (float)TextSize;
            var r = (float)_labelsRotation;
            var hasRotation = Math.Abs(r) > 0.01f;

            var max = MaxLimit is null ? (_visibleDataBounds ?? _dataBounds).Max : MaxLimit.Value;
            var min = MinLimit is null ? (_visibleDataBounds ?? _dataBounds).Min : MinLimit.Value;

            var start = Math.Truncate(min / s) * s;
            if (!activeSeparators.TryGetValue(cartesianChart, out var separators))
            {
                separators = new Dictionary<double, AxisVisualSeprator<TDrawingContext>>();
                activeSeparators[cartesianChart] = separators;
            }

            if (Name is not null && NamePaint is not null)
            {
                if (_nameGeometry is null)
                {
                    _nameGeometry = new TTextGeometry
                    {
                        TextSize = size,
                        HorizontalAlign = Align.Middle,
                        VerticalAlign = Align.Middle
                    };
                }

                _nameGeometry.Padding = NamePadding;
                _nameGeometry.Text = Name;
                _nameGeometry.TextSize = (float)NameTextSize;

                if (_orientation == AxisOrientation.X)
                {
                    var nameSize = _nameGeometry.Measure(NamePaint);
                    _nameGeometry.X = (lxi + lxj) * 0.5f;
                    _nameGeometry.Y = Position == AxisPosition.Start ? yoo + nameSize.Height : yoo - nameSize.Height;
                }
                else
                {
                    _nameGeometry.RotateTransform = -90;
                    var nameSize = _nameGeometry.Measure(NamePaint);
                    _nameGeometry.X = Position == AxisPosition.Start ? xoo - nameSize.Width - Padding.Bottom : xoo + nameSize.Width + Padding.Bottom;
                    _nameGeometry.Y = (lyi + lyj) * 0.5f;
                }
            }

            var measured = new HashSet<AxisVisualSeprator<TDrawingContext>>();

            for (var i = start; i <= max; i += s)
            {
                if (i < min) continue;

                var label = labeler(i);
                float x, y;
                if (_orientation == AxisOrientation.X)
                {
                    x = scale.ToPixels(i);
                    y = yoo;
                }
                else
                {
                    x = xoo;
                    y = scale.ToPixels(i);
                }

                if (!separators.TryGetValue(i, out var visualSeparator))
                {
                    visualSeparator = new AxisVisualSeprator<TDrawingContext>() { Value = i };

                    if (LabelsPaint is not null)
                    {
                        var textGeometry = new TTextGeometry { TextSize = size };
                        visualSeparator.Text = textGeometry;
                        if (hasRotation) textGeometry.RotateTransform = r;

                        _ = textGeometry
                            .TransitionateProperties(
                                nameof(textGeometry.X),
                                nameof(textGeometry.Y),
                                nameof(textGeometry.Opacity))
                            .WithAnimation(animation =>
                                animation
                                    .WithDuration(AnimationsSpeed ?? cartesianChart.AnimationsSpeed)
                                    .WithEasingFunction(EasingFunction ?? cartesianChart.EasingFunction));

                        textGeometry.Opacity = 0;

                        if (previousSacale is not null)
                        {
                            float xi, yi;

                            if (_orientation == AxisOrientation.X)
                            {
                                xi = previousSacale.ToPixels(i);
                                yi = yoo;
                            }
                            else
                            {
                                xi = xoo;
                                yi = previousSacale.ToPixels(i);
                            }

                            textGeometry.X = xi;
                            textGeometry.Y = yi;
                            textGeometry.CompleteAllTransitions();
                        }
                    }

                    if (SeparatorsPaint is not null && ShowSeparatorLines)
                    {
                        var lineGeometry = new TLineGeometry();

                        visualSeparator.Line = lineGeometry;

                        _ = lineGeometry
                            .TransitionateProperties(
                                nameof(lineGeometry.X), nameof(lineGeometry.X1),
                                nameof(lineGeometry.Y), nameof(lineGeometry.Y1),
                                nameof(lineGeometry.Opacity))
                            .WithAnimation(animation =>
                                animation
                                    .WithDuration(AnimationsSpeed ?? cartesianChart.AnimationsSpeed)
                                    .WithEasingFunction(EasingFunction ?? cartesianChart.EasingFunction));

                        lineGeometry.Opacity = 0;

                        if (previousSacale is not null)
                        {
                            float xi, yi;

                            if (_orientation == AxisOrientation.X)
                            {
                                xi = previousSacale.ToPixels(i);
                                yi = yoo;
                            }
                            else
                            {
                                xi = xoo;
                                yi = previousSacale.ToPixels(i);
                            }

                            if (_orientation == AxisOrientation.X)
                            {
                                lineGeometry.X = xi;
                                lineGeometry.X1 = xi;
                                lineGeometry.Y = lyi;
                                lineGeometry.Y1 = lyj;
                            }
                            else
                            {
                                lineGeometry.X = lxi;
                                lineGeometry.X1 = lxj;
                                lineGeometry.Y = yi;
                                lineGeometry.Y1 = yi;
                            }

                            lineGeometry.CompleteAllTransitions();
                        }
                    }

                    separators.Add(i, visualSeparator);
                }

                if (NamePaint is not null && _nameGeometry is not null)
                    NamePaint.AddGeometryToPaintTask(cartesianChart.Canvas, _nameGeometry);
                if (SeparatorsPaint is not null && ShowSeparatorLines && visualSeparator.Line is not null)
                    SeparatorsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visualSeparator.Line);
                if (LabelsPaint is not null && visualSeparator.Text is not null)
                    LabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visualSeparator.Text);

                if (visualSeparator.Text is not null)
                {
                    visualSeparator.Text.Text = label;
                    visualSeparator.Text.Padding = _padding;
                    visualSeparator.Text.X = x;
                    visualSeparator.Text.Y = y;
                    if (hasRotation) visualSeparator.Text.RotateTransform = r;

                    visualSeparator.Text.Opacity = 1;

                    if (((ICartesianAxis)this).PreviousDataBounds is null) visualSeparator.Text.CompleteAllTransitions();
                }

                if (visualSeparator.Line is not null)
                {
                    if (_orientation == AxisOrientation.X)
                    {
                        visualSeparator.Line.X = x;
                        visualSeparator.Line.X1 = x;
                        visualSeparator.Line.Y = lyi;
                        visualSeparator.Line.Y1 = lyj;
                    }
                    else
                    {
                        visualSeparator.Line.X = lxi;
                        visualSeparator.Line.X1 = lxj;
                        visualSeparator.Line.Y = y;
                        visualSeparator.Line.Y1 = y;
                    }

                    visualSeparator.Line.Opacity = 1;

                    if (((ICartesianAxis)this).PreviousDataBounds is null) visualSeparator.Line.CompleteAllTransitions();
                }

                if (visualSeparator.Text is not null || visualSeparator.Line is not null) _ = measured.Add(visualSeparator);
            }

            foreach (var separator in separators.ToArray())
            {
                if (measured.Contains(separator.Value)) continue;

                SoftDeleteSeparator(cartesianChart, separator.Value, scale);
                _ = separators.Remove(separator.Key);
            }
        }

        /// <inheritdoc cref="IPlane{TDrawingContext}.GetNameLabelSize(Chart{TDrawingContext})"/>
        public LvcSize GetNameLabelSize(Chart<TDrawingContext> chart)
        {
            if (NamePaint is null || string.IsNullOrWhiteSpace(Name)) return new LvcSize(0, 0);

            var textGeometry = new TTextGeometry
            {
                Text = Name ?? string.Empty,
                TextSize = (float)NameTextSize,
                RotateTransform = Orientation == AxisOrientation.X ? 0 : -90,
                Padding = Padding
            };

            return textGeometry.Measure(NamePaint);
        }

        /// <inheritdoc cref="IPlane{TDrawingContext}.GetPossibleSize(Chart{TDrawingContext})"/>
        public virtual LvcSize GetPossibleSize(Chart<TDrawingContext> chart)
        {
            if (_dataBounds is null) throw new Exception("DataBounds not found");
            if (LabelsPaint is null) return new LvcSize(0f, 0f);

            var ts = (float)TextSize;
            var labeler = Labeler;

            if (Labels is not null)
            {
                labeler = Labelers.BuildNamedLabeler(Labels).Function;
                _minStep = 1;
            }

            var axisTick = this.GetTick(chart.DrawMarginSize);
            var s = axisTick.Value;
            if (s < _minStep) s = _minStep;

            var max = MaxLimit is null ? (_visibleDataBounds ?? _dataBounds).Max : MaxLimit.Value;
            var min = MinLimit is null ? (_visibleDataBounds ?? _dataBounds).Min : MinLimit.Value;

            var start = Math.Truncate(min / s) * s;

            var w = 0f;
            var h = 0f;
            var r = (float)LabelsRotation;

            for (var i = start; i <= max; i += s)
            {
                var textGeometry = new TTextGeometry
                {
                    Text = labeler(i),
                    TextSize = ts,
                    RotateTransform = r,
                    Padding = _padding
                };
                var m = textGeometry.Measure(LabelsPaint); // TextBrush.MeasureText(labeler(i, axisTick));
                if (m.Width > w) w = m.Width;
                if (m.Height > h) h = m.Height;
            }

            return new LvcSize(w, h);
        }

        /// <inheritdoc cref="ICartesianAxis.Initialize(AxisOrientation)"/>
        void ICartesianAxis.Initialize(AxisOrientation orientation)
        {
            _orientation = orientation;
            _dataBounds = new Bounds();
            _visibleDataBounds = new Bounds();
            Initialized?.Invoke(this);
        }

        /// <summary>
        /// Deletes the specified chart.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <returns></returns>
        public virtual void Delete(Chart<TDrawingContext> chart)
        {

            if (_labelsPaint is not null)
            {
                chart.Canvas.RemovePaintTask(_labelsPaint);
                _labelsPaint.ClearGeometriesFromPaintTask(chart.Canvas);
            }
            if (_separatorsPaint is not null)
            {
                chart.Canvas.RemovePaintTask(_separatorsPaint);
                _separatorsPaint.ClearGeometriesFromPaintTask(chart.Canvas);
            }

            _ = activeSeparators.Remove(chart);
        }

        /// <inheritdoc cref="IChartElement{TDrawingContext}.RemoveFromUI(Chart{TDrawingContext})"/>
        public override void RemoveFromUI(Chart<TDrawingContext> chart)
        {
            base.RemoveFromUI(chart);
            ((IPlane)this).PreviousDataBounds = null;
            _ = activeSeparators.Remove(chart);
        }

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (!((ICartesianAxis)this).IsNotifyingChanges) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Softly deletes the separator.
        /// </summary>
        /// <param name="chart">The chart.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="scaler">The scaler.</param>
        /// <returns></returns>
        protected virtual void SoftDeleteSeparator(
            Chart<TDrawingContext> chart,
            AxisVisualSeprator<TDrawingContext> separator,
            Scaler scaler)
        {
            var controlSize = chart.ControlSize;
            var drawLocation = chart.DrawMarginLocation;
            var drawMarginSize = chart.DrawMarginSize;

            var lyi = drawLocation.Y;
            var lyj = drawLocation.Y + drawMarginSize.Height;
            var lxi = drawLocation.X;
            var lxj = drawLocation.X + drawMarginSize.Width;

            float xoo = 0f, yoo = 0f;

            if (_orientation == AxisOrientation.X)
            {
                yoo = _position == AxisPosition.Start
                     ? controlSize.Height - _yo
                     : _yo;
            }
            else
            {
                xoo = _position == AxisPosition.Start
                    ? _xo
                    : controlSize.Width - _xo;
            }

            float x, y;
            if (_orientation == AxisOrientation.X)
            {
                x = scaler.ToPixels(separator.Value);
                y = yoo;
            }
            else
            {
                x = xoo;
                y = scaler.ToPixels(separator.Value);
            }

            if (separator.Line is not null)
            {
                if (_orientation == AxisOrientation.X)
                {
                    separator.Line.X = x;
                    separator.Line.X1 = x;
                    separator.Line.Y = lyi;
                    separator.Line.Y1 = lyj;
                }
                else
                {
                    separator.Line.X = lxi;
                    separator.Line.X1 = lxj;
                    separator.Line.Y = y;
                    separator.Line.Y1 = y;
                }

                separator.Line.Opacity = 0;
                separator.Line.RemoveOnCompleted = true;
            }

            if (separator.Text is not null)
            {
                separator.Text.X = x;
                separator.Text.Y = y;
                separator.Text.Opacity = 0;
                separator.Text.RemoveOnCompleted = true;
            }
        }

        /// <summary>
        /// Called when [paint changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected override void OnPaintChanged(string? propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Gets the paint tasks.
        /// </summary>
        /// <returns></returns>
        protected override IPaint<TDrawingContext>?[] GetPaintTasks()
        {
            return new[] { _separatorsPaint, _labelsPaint, _namePaint };
        }
    }
}
