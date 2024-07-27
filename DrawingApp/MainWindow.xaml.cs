using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System;
using System.Windows.Media;
using System.Security.Policy;
using System.Collections;
using DrawingApp.models;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private bool isLineMode;
        private bool isEllipseMode;
        private bool isRectangleMode;
        private bool isEraserMode;

        private List<Point> pointsList;
        private Polyline currentLine;
        private Line straightLine;
        private Ellipse currentEllipse;
        private Rectangle currentRectangle;

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string lastSavedDirectory = null;
        private string lastSavedFilePath = null;

        private List<Song> songs = new List<Song>();
        private int currentPlaySongIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            pointsList = new List<Point>();

            isLineMode = false;
            isEllipseMode = false;
            isRectangleMode = false;
            isEraserMode = false;

            mediaPlayer.Volume = 0.5;
            sliderVolume.Value = 50;

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            songs = ProcessDirectory(@"..\..\..\assets\Music");
            SongsDataGrid.ItemsSource = null;
            SongsDataGrid.ItemsSource = songs;
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
                    Stroke = isEraserMode ? Brushes.White : GetSelectedColorBrush(),
                    StrokeThickness = isEraserMode ? 10 : GetCurrentThickness(),
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
            if (currentLine != null)
            {
                currentLine.StrokeThickness = isEraserMode ? 10 : GetCurrentThickness();
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (isDrawing && currentLine != null && !isEraserMode)
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
            isEraserMode = false;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            isEllipseMode = !isEllipseMode;
            isLineMode = false;
            isRectangleMode = false;
            isEraserMode = false;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            isRectangleMode = !isRectangleMode;
            isLineMode = false;
            isEllipseMode = false;
            isEraserMode = false;
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            isLineMode = false;
            isEllipseMode = false;
            isRectangleMode = false;
            isEraserMode = false;
        }

        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            isEraserMode = !isEraserMode;
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
            {
                sliProgress.Minimum = 0;
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                }
                sliProgress.Value = mediaPlayer.Position.TotalSeconds;
                CurrentMusicTimeLabel.Content = mediaPlayer.Position.ToString(@"mm\:ss");
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    EndMusicTimeLabel.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }
            }             
        }
        private void PlayMusicButtonImageToPause()
        {
            Image newImage = new Image();
            newImage.Source = new BitmapImage(new Uri("./assets/pause.png", UriKind.Relative));
            btnPlayMusic.Content = newImage;
        }

        private void btnPlayMusic_Click(object sender, RoutedEventArgs e)
        {
            if(mediaPlayer.Source != null)
            {
                if (mediaPlayerIsPlaying)
                {
                    mediaPlayer.Pause();
                    mediaPlayerIsPlaying = false;
                    Image newImage = new Image();
                    newImage.Source = new BitmapImage(new Uri("./assets/play.png", UriKind.Relative));
                    btnPlayMusic.Content = newImage;
                }
                else
                {
                    mediaPlayer.Play();
                    mediaPlayerIsPlaying = true;
                    PlayMusicButtonImageToPause();
                }
            }
            else
            {
                MusicExpander.IsExpanded = true;
            }
        }


        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                userIsDraggingSlider = true;
            }
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (mediaPlayer.Source != null) {
                userIsDraggingSlider = false;
                mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            }       
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentMusicTimeLabel.Content = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss");
        }


        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (double)( sliderVolume.Value / 100 );
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

        private void OpenMusicExpanderButton_Click(object sender, RoutedEventArgs e)
        {
            MusicExpander.IsExpanded = !MusicExpander.IsExpanded;
        }

        private List<Song> ProcessDirectory(string targetDirectory)
        {
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetDirectory );
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.mp3");
            List<Song> songs = new List<Song>();

            int id = 1;
            foreach (string fileName in fileEntries)
            {
                Song song = new Song() { Id = id++, Name = System.IO.Path.GetFileNameWithoutExtension(fileName) };
                songs.Add(song);
            }

            return songs;
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

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));

                Image image = new Image
                {
                    Source = bitmap,
                    Width = bitmap.Width,
                    Height = bitmap.Height
                };

                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                drawingPage.Children.Add(image);
            }
        }

        private void SongsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongsDataGrid.SelectedItem != null)
            {
                Song? selectedSong = SongsDataGrid.SelectedItem as Song;
                if(selectedSong != null)
                {
                    string selectedMusicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\assets\\Music", selectedSong.Name + ".mp3");
                    mediaPlayer.Open(new Uri(selectedMusicPath));
                    mediaPlayer.Play();
                    mediaPlayerIsPlaying = true;
                    Image newImage = new Image();
                    newImage.Source = new BitmapImage(new Uri("./assets/pause.png", UriKind.Relative));
                    btnPlayMusic.Content = newImage;
                    CurrentPlayingSongLabel.Content = selectedSong.Name;
                    currentPlaySongIndex = selectedSong.Id - 1; //index
                    PlayMusicButtonImageToPause();
                }
            }
        }

        private void ExpandMusicBarButton_Click(object sender, RoutedEventArgs e)
        {
            MusicBarExpender.IsExpanded = !MusicBarExpender.IsExpanded;
            Image newImage = new Image();
            if (MusicBarExpender.IsExpanded)
            {      
                newImage.Source = new BitmapImage(new Uri("./assets/down-arrow.png", UriKind.Relative));
                ExpandMusicBarButton.Content = newImage;
                ExpandMusicBarButton.Margin = new Thickness(ExpandMusicBarButton.Margin.Left, 0, ExpandMusicBarButton.Margin.Right, -8);
            }
            else {
                newImage.Source = new BitmapImage(new Uri("./assets/up-arrow.png", UriKind.Relative));
                ExpandMusicBarButton.Content = newImage;
                ExpandMusicBarButton.Margin = new Thickness(ExpandMusicBarButton.Margin.Left, 70, ExpandMusicBarButton.Margin.Right, 10);
            }              
        }
        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Xử lý chuyển sang bài hát tiếp theo ở đây
            // Ví dụ: Chuyển đến bài hát tiếp theo trong danh sách
            currentPlaySongIndex++;
            if (currentPlaySongIndex >= songs.Count) 
                currentPlaySongIndex = 0;
            PlaySongByIndex();
        }

        private void PreviousSongButton_Click(object sender, RoutedEventArgs e)
        {
            currentPlaySongIndex--;
            if (currentPlaySongIndex < 0)
                currentPlaySongIndex = songs.Count - 1;
            PlaySongByIndex();
        }
        private void PlaySongByIndex()
        {
            string selectedMusicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\assets\\Music", songs[currentPlaySongIndex].Name + ".mp3");
            CurrentPlayingSongLabel.Content = songs[currentPlaySongIndex].Name;
            mediaPlayer.Open(new Uri(selectedMusicPath));
            mediaPlayer.Play();
            PlayMusicButtonImageToPause();
        }

        private void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            currentPlaySongIndex++;
            if (currentPlaySongIndex >= songs.Count)
                currentPlaySongIndex = 0;
            PlaySongByIndex();
        }
    }
}
