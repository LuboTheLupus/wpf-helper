using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SnappingGrid;
public partial class ClippingGrid : UserControl
{
    #region private fields
    bool isDragging = false;
    Point anchorPoint;
    double distance0;
    double distance1;
    TranslateTransform transform;

    #endregion private fields

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
    void MyRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!isDragging)
        {
            anchorPoint = e.GetPosition(this);
            isDragging = true;
            MyRectangle.CaptureMouse();
            distanceLine0.Visibility = Visibility.Visible;
            distanceLine1.Visibility = Visibility.Visible;
        }
    }

    void MyRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging)
        {
            MyRectangle.ReleaseMouseCapture();
            isDragging = false;
            if (distance0 < distance1)
            {
                transform.X = line0.X2;
            }
            else
            {
                transform.X = line1.X2;
            }

            distanceLine0.Visibility = Visibility.Collapsed;
            distanceLine1.Visibility = Visibility.Collapsed;
        }
    }

    void MyRectangle_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            var currentPoint = e.GetPosition(this);
            transform = MyRectangle.RenderTransform as TranslateTransform ?? new TranslateTransform();
            transform.X += currentPoint.X - anchorPoint.X;
            transform.Y += currentPoint.Y - anchorPoint.Y;
            MyRectangle.RenderTransform = transform;
            anchorPoint = currentPoint;

            distanceLine0.X1 = line0.X1;
            distanceLine0.X2 = transform.X + MyRectangle.ActualWidth / 2;
            distanceLine0.Y1 = line0.Y2 / 2;
            distanceLine0.Y2 = transform.Y + MyRectangle.ActualHeight / 2;

            distanceLine1.X1 = line1.X1;
            distanceLine1.X2 = transform.X + MyRectangle.ActualWidth / 2;
            distanceLine1.Y1 = line1.Y2 / 2;
            distanceLine1.Y2 = transform.Y + MyRectangle.ActualHeight / 2;

            distance0 = Math.Sqrt(Math.Pow(distanceLine0.X2 - distanceLine0.X1, 2) + Math.Pow(distanceLine0.Y2 - distanceLine0.Y1, 2));
            distance1 = Math.Sqrt(Math.Pow(distanceLine1.X2 - distanceLine1.X1, 2) + Math.Pow(distanceLine1.Y2 - distanceLine1.Y1, 2));

            info.Text = $"{Math.Round(distance0)}\n {Math.Round(distance1)}";


        }
    }
    #endregion event handlers
}

