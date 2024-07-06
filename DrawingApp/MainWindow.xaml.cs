using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;


namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private bool isLineMode;
        private bool isEllipseMode;
        private bool isRectangleMode;
        private List<Point> pointsList;
        private Polyline currentLine;
        private Line straightLine;
        private Ellipse currentEllipse;
        private Rectangle currentRectangle;

        private string lastSavedDirectory;
        public MainWindow()
        {
            InitializeComponent();
            pointsList = new List<Point>();
            isLineMode = false;
            isEllipseMode = false;
            isRectangleMode = false;
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
            else if (isEllipseMode)
            {
                currentEllipse = new Ellipse
                {
                    Stroke = GetSelectedColorBrush(),
                    StrokeThickness = GetCurrentThickness()
                };
                Canvas.SetLeft(currentEllipse, startPoint.X);
                Canvas.SetTop(currentEllipse, startPoint.Y);
                drawingPage.Children.Add(currentEllipse);
            }
            else if (isRectangleMode)
            {
                currentRectangle = new Rectangle
                {
                    Stroke = GetSelectedColorBrush(),
                    StrokeThickness = GetCurrentThickness()
                };
                Canvas.SetLeft(currentRectangle, startPoint.X);
                Canvas.SetTop(currentRectangle, startPoint.Y);
                drawingPage.Children.Add(currentRectangle);
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
                else if (isEllipseMode && currentEllipse != null)
                {
                    double width = currentPoint.X - Canvas.GetLeft(currentEllipse);
                    double height = currentPoint.Y - Canvas.GetTop(currentEllipse);
                    currentEllipse.Width = Math.Abs(width);
                    currentEllipse.Height = Math.Abs(height);
                    if (width < 0)
                    {
                        Canvas.SetLeft(currentEllipse, currentPoint.X);
                    }
                    if (height < 0)
                    {
                        Canvas.SetTop(currentEllipse, currentPoint.Y);
                    }
                }
                else if (isRectangleMode && currentRectangle != null)
                {
                    double width = currentPoint.X - Canvas.GetLeft(currentRectangle);
                    double height = currentPoint.Y - Canvas.GetTop(currentRectangle);
                    currentRectangle.Width = Math.Abs(width);
                    currentRectangle.Height = Math.Abs(height);
                    if (width < 0)
                    {
                        Canvas.SetLeft(currentRectangle, currentPoint.X);
                    }
                    if (height < 0)
                    {
                        Canvas.SetTop(currentRectangle, currentPoint.Y);
                    }
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
            //selectedColor.A = 128;
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
            isEllipseMode = false;
            isRectangleMode = false;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            isEllipseMode = !isEllipseMode;
            isLineMode = false;
            isRectangleMode = false;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            isRectangleMode = !isRectangleMode;
            isLineMode = false;
            isEllipseMode = false;
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            isLineMode = false;
            isEllipseMode = false;
            isRectangleMode = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)drawingPage.ActualWidth, (int)drawingPage.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            renderBitmap.Render(drawingPage);

            // Check if a file has been saved previously
            if (lastSavedDirectory != null)
            {
                string filePath = System.IO.Path.Combine(lastSavedDirectory, "saved_image.png"); // Default name
                SaveToFile(filePath, renderBitmap);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Files (*.png)|*.png",
                    DefaultExt = "png",
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    lastSavedDirectory = System.IO.Path.GetDirectoryName(filePath);
                    SaveToFile(filePath, renderBitmap);
                }
            }
        }

        private void SaveToFile(string filePath, RenderTargetBitmap renderBitmap)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = null;

                encoder = new PngBitmapEncoder();

                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fs);

                    MessageBox.Show("File saved successfully!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }



}

