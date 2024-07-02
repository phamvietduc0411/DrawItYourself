using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private bool isLineMode;
        private List<Point> pointsList;
        private Polyline currentLine;
        private Line straightLine;

        public MainWindow()
        {
            InitializeComponent();
            pointsList = new List<Point>();
            isLineMode = false;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            Point startPoint = e.GetPosition(drawingPage);
            pointsList.Clear();

            if (isLineMode)
            {
                straightLine = new Line
                {
                    Stroke = GetSelectedColorBrush(),
                    StrokeThickness = GetCurrentThickness(),
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = startPoint.X,
                    Y2 = startPoint.Y
                };
                drawingPage.Children.Add(straightLine);
            }
            else
            {
                currentLine = new Polyline
                {
                    Stroke = GetSelectedColorBrush(),
                    StrokeThickness = GetCurrentThickness(),
                    Points = new PointCollection(pointsList)
                };
                drawingPage.Children.Add(currentLine);
                pointsList.Add(startPoint);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                Point currentPoint = e.GetPosition(drawingPage);

                if (isLineMode && straightLine != null)
                {
                    straightLine.X2 = currentPoint.X;
                    straightLine.Y2 = currentPoint.Y;
                }
                else if (currentLine != null)
                {
                    pointsList.Add(currentPoint);
                    currentLine.Points = new PointCollection(pointsList);
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        private SolidColorBrush GetSelectedColorBrush()
        {
            Color selectedColor = colorPicker.SelectedColor ?? Colors.Black;
            selectedColor.A = 128;
            return new SolidColorBrush(selectedColor);
        }

        private double GetCurrentThickness()
        {
            if (thicknessComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)thicknessComboBox.SelectedItem;
                return double.Parse(selectedItem.Tag.ToString());
            }
            return 1;
        }

        private void ThicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDrawing && currentLine != null)
            {
                currentLine.StrokeThickness = GetCurrentThickness();
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (isDrawing && currentLine != null)
            {
                currentLine.Stroke = GetSelectedColorBrush();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            drawingPage.Children.Clear();
        }

        private void LineModeButton_Click(object sender, RoutedEventArgs e)
        {
            isLineMode = !isLineMode;
        }

    }
}
