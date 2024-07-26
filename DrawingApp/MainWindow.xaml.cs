using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;


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

        private bool mediaPlayerIsPlaying = false;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string lastSavedDirectory = null;
        private string lastSavedFilePath = null;
        public MainWindow()
        {
            InitializeComponent();
            pointsList = new List<Point>();

            isLineMode = false;
            isEllipseMode = false;
            isRectangleMode = false;

            mediaPlayer.Volume = 0.5;
            sliderVolume.Value = 50;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

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

        private int GetCurrentThickness()
        {
            if (thicknessComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)thicknessComboBox.SelectedItem;
                return int.Parse(selectedItem.Tag.ToString());
            }
            return 1;
        }

        private void ThicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            currentLine.StrokeThickness = GetCurrentThickness();

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
            MessageBoxResult result = MessageBox.Show("Do you want to delete all?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }
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

            if (lastSavedFilePath != null)
            {
                SaveToFile(lastSavedFilePath, renderBitmap);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Files (*.png)|*.png",
                    DefaultExt = "png",
                    AddExtension = true
                };

                if (lastSavedDirectory != null)
                {
                    saveFileDialog.InitialDirectory = lastSavedDirectory;
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    lastSavedFilePath = saveFileDialog.FileName;
                    lastSavedDirectory = System.IO.Path.GetDirectoryName(lastSavedFilePath);
                    SaveToFile(lastSavedFilePath, renderBitmap);
                }
            }
        }

        private void SaveToFile(string filePath, RenderTargetBitmap renderBitmap)
        {
            using (FileStream saveFile = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = null;
                if (filePath.EndsWith(".png"))
                {
                    encoder = new PngBitmapEncoder();
                }

                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(saveFile);

                    MessageBox.Show(@$"File saved successfully!
Your file has been saved to: {filePath}", "Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            else
                lblStatus.Content = "No file selected...";
        }

        private void PlayMusic_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mediaPlayer == null) || (mediaPlayer.Source == null);
        }
        private void PauseMusic_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }
        private void StopMusic_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }
        private void btnPlayMusic_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
            mediaPlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void btnPauseMusic_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void btnStopMusic_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (double)(sliderVolume.Value / 100);
        }

        void onDragDelta(object sender, DragDeltaEventArgs e)
        {
            //Move the Thumb to the mouse position during the drag operation
            double yadjust = drawingPage.Height + e.VerticalChange;
            double xadjust = drawingPage.Width + e.HorizontalChange;
            if ((xadjust >= 0) && (yadjust >= 0))
            {
                drawingPage.Width = xadjust;
                drawingPage.Height = yadjust;
                drawingPageContainer.Width = xadjust;
                drawingPageContainer.Height = yadjust + 20;
                Canvas.SetLeft(myThumb, Canvas.GetLeft(myThumb) +
                                        e.HorizontalChange);
                Canvas.SetTop(myThumb, Canvas.GetTop(myThumb) +
                                        e.VerticalChange);

            }
        }

        void onDragStarted(object sender, DragStartedEventArgs e)
        {
            myThumb.Background = Brushes.Orange;
        }
        void onDragCompleted(object sender, DragCompletedEventArgs e)
        {
            myThumb.Background = Brushes.Blue;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to close the application?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

        }
    }
}

