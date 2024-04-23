using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnappingGrid;
public partial class ClippingGrid : UserControl
{
    #region private fields
    bool _isDragging = false;
    Point _anchorPoint;
    TranslateTransform? _transform;
    Line[] _verticalLines = new Line[100];
    Line[] _verticalDistanceLines = new Line[100];
    Line[] _horizontalLines = new Line[100];
    Line[] _horizontalDistanceLines = new Line[100];
    DispatcherTimer? _delayTimer;
    int _delayInMilliseconds = 200;
    #endregion private fields

    #region VerticalSlices dp
    public int VerticalSlices
    {
        get { return (int)GetValue(VerticalSlicesProperty); }
        set
        {
            SetValue(VerticalSlicesProperty, value);
            if (_delayTimer is null)
            {
                _delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_delayInMilliseconds) };
                _delayTimer.Tick += (s, e) =>
                {
                    _delayTimer.Stop();
                    GenerateVerticalSlices(VerticalSlices);
                };
            }
            _delayTimer.Stop();
            _delayTimer.Start();
        }
    }

    public static readonly DependencyProperty VerticalSlicesProperty =
        DependencyProperty.Register("VerticalSlices", typeof(int), typeof(ClippingGrid), new PropertyMetadata(0, (dp, args) =>
        {
            var clippingGrid = dp as ClippingGrid;
            if (clippingGrid is null) return;
            if ((int)args.NewValue < 0) throw new ArgumentOutOfRangeException("VerticalSlices must be 0 or greater");
            if ((int)args.NewValue != (int)args.OldValue)
            {
                clippingGrid.GenerateVerticalSlices((int)args.NewValue);
            }
        }));
    #endregion VerticalSlices dp

    #region HorizontalSlices dp
    public int HorizontalSlices
    {
        get { return (int)GetValue(HorizontalSlicesProperty); }
        set
        {
            SetValue(HorizontalSlicesProperty, value);
            if (_delayTimer is null)
            {
                _delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_delayInMilliseconds) };
                _delayTimer.Tick += (s, e) =>
                {
                    _delayTimer.Stop();
                    GenerateHorizontalSlices(HorizontalSlices);
                };
            }
            _delayTimer.Stop();
            _delayTimer.Start();
        }
    }

    public static readonly DependencyProperty HorizontalSlicesProperty =
        DependencyProperty.Register("HorizontalSlices", typeof(int), typeof(ClippingGrid), new PropertyMetadata(0, (dp, args) =>
        {
            var clippingGrid = dp as ClippingGrid;
            if (clippingGrid is null) return;
            if ((int)args.NewValue < 0) throw new ArgumentOutOfRangeException("HorizontalSlices must be 0 or greater");
            if ((int)args.NewValue != (int)args.OldValue)
            {
                clippingGrid.GenerateHorizontalSlices((int)args.NewValue);
            }
        }));
    #endregion HorizontalSlices dp

    #region constructors
    public ClippingGrid()
    {
        InitializeComponent();
        MyRectangle.MouseLeftButtonDown += MyRectangle_MouseLeftButtonDown;
        MyRectangle.MouseLeftButtonUp += MyRectangle_MouseLeftButtonUp;
        MyRectangle.MouseMove += MyRectangle_MouseMove;
    }
    #endregion constructors

    #region event handlers
    void MyRectangle_MouseLeftButtonDown(object s, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (!_isDragging)
        {
            currentClosestLine = _verticalDistanceLines.Take(VerticalSlices).MinBy(line => Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)));
            if (currentClosestLine is not null)
            {
                currentClosestLine.Visibility = Visibility.Visible;
            }

            _anchorPoint = e.GetPosition(this);
            _isDragging = true;
            MyRectangle.CaptureMouse();
        }
    }
    Line? currentClosestLine;
    void MyRectangle_MouseLeftButtonUp(object s, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (_transform is null) return;
        if (_isDragging)
        {
            MyRectangle.ReleaseMouseCapture();
            _isDragging = false;
        }
    }

    void MyRectangle_MouseMove(object s, MouseEventArgs e)
    {
        e.Handled = true;
        if (_isDragging)
        {
            var currentPoint = e.GetPosition(this);
            _transform = MyRectangle.RenderTransform as TranslateTransform ?? new TranslateTransform();
            _transform.X += currentPoint.X - _anchorPoint.X;
            _transform.Y += currentPoint.Y - _anchorPoint.Y;
            MyRectangle.RenderTransform = _transform;
            _anchorPoint = currentPoint;
            for (int i = 0; i < VerticalSlices; i++)
            {
                _verticalDistanceLines[i].X1 = _verticalLines[i].X1;
                _verticalDistanceLines[i].X2 = _transform.X + MyRectangle.ActualWidth / 2;
                _verticalDistanceLines[i].Y1 = _verticalLines[i].Y2 / 2;
                _verticalDistanceLines[i].Y2 = _transform.Y + MyRectangle.ActualHeight / 2;
            }
            var closestLine = _verticalDistanceLines.Take(VerticalSlices).MinBy(line => Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)));
            if (currentClosestLine is not null && closestLine is not null && closestLine != currentClosestLine)
            {
                currentClosestLine.Visibility = Visibility.Collapsed;
                currentClosestLine = closestLine;
                currentClosestLine.Visibility = Visibility.Visible;
            }
        }
    }
    void UserControl_Loaded(object s, RoutedEventArgs e)
    {
        e.Handled = true;
        GenerateVerticalSlices(VerticalSlices);
        GenerateHorizontalSlices(HorizontalSlices);
    }
    void UserControl_SizeChanged(object s, SizeChangedEventArgs e)
    {
        e.Handled = true;
        GenerateVerticalSlices(VerticalSlices);
        GenerateHorizontalSlices(HorizontalSlices);
    }
    #endregion event handlers

    #region private methods
    void GenerateVerticalSlices(int countSlices)
    {
        if (countSlices > _verticalLines.Length)
        {
            _verticalLines = new Line[countSlices];
            _verticalDistanceLines = new Line[countSlices];
        }
        else
        {
            foreach (var line in _verticalLines)
            {
                canvas.Children.Remove(line);
            }

            foreach (var line in _verticalDistanceLines)
            {
                canvas.Children.Remove(line);
            }
        }
        for (int i = 0; i < countSlices; i++)
        {
            _verticalLines[i] = new Line
            {
                X1 = ActualWidth / countSlices * i,
                X2 = ActualWidth / countSlices * i,
                Y1 = 0,
                Y2 = ActualHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _verticalDistanceLines[i] = new Line
            {
                Stroke = Brushes.White,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection([8, 4]),
                Visibility = Visibility.Collapsed
            };
            canvas.Children.Add(_verticalLines[i]);
            canvas.Children.Add(_verticalDistanceLines[i]);
        }
    }
    void GenerateHorizontalSlices(int countSlices)
    {
        if (countSlices > _horizontalLines.Length)
        {
            _horizontalLines = new Line[countSlices];
            _horizontalDistanceLines = new Line[countSlices];
        }
        else
        {
            foreach (var line in _horizontalLines)
            {
                canvas.Children.Remove(line);
            }

            foreach (var line in _horizontalDistanceLines)
            {
                canvas.Children.Remove(line);
            }
        }
        for (int i = 0; i < countSlices; i++)
        {
            _horizontalLines[i] = new Line
            {
                X1 = 0,
                X2 = ActualWidth,
                Y1 = ActualHeight / countSlices * i,
                Y2 = ActualHeight / countSlices * i,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _horizontalDistanceLines[i] = new Line
            {
                Stroke = Brushes.White,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection([8, 4]),
                Visibility = Visibility.Collapsed
            };
            canvas.Children.Add(_horizontalLines[i]);
            canvas.Children.Add(_horizontalDistanceLines[i]);
        }
    }
    #endregion private methods
}
