using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int DotCount = 100;
        private DateTime startTime = DateTime.Now;

        private Game game;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string FrameRateText => $"{game?.FrameRate ?? 0:F2} fps";
        public string TimeText => $"Elapsed {(DateTime.Now - startTime).TotalSeconds:F2}";
        public string DeadText => $"Dead {game?.DeadDotCount ?? 0}/{DotCount}";
        public string ReachedGoalText => $"Reached Goal {game?.ReachedGoalCount ?? 0}/{DotCount}";
        public string GenerationText => $"Generation {game?.Generation ?? 0}";
        public string BestText => $"Best {game?.Best}/{DotCount}";
        public bool IsStartButtonEnabled { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && IsStartButtonEnabled)
            {
                Start();
            }
        }

        private void Start()
        {
            IsStartButtonEnabled = false;
            RaisePropertyChanged(nameof(IsStartButtonEnabled));

            Canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (game == null)
                {
                    Canvas.Children.Clear();
                }

                var startingLine = new Line
                {
                    X1 = 0,
                    X2 = Canvas.ActualWidth,
                    Y1 = Canvas.ActualHeight * 0.75d,
                    Y2 = Canvas.ActualHeight * 0.75d,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection(new[] { 2.0d, 2.0d })
                };

                Canvas.Children.Add(startingLine);

                if (game == null)
                {
                    game = new Game(DotCount, Canvas);
                    game.FrameHappened += Game_FrameHappened;
                    game.NextGeneration += Game_NextGeneration;
                }

                startTime = DateTime.Now;
                game.Start();
            }));
        }

        private void Game_NextGeneration(object sender, EventArgs e) => Start();

        private void Game_FrameHappened(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(FrameRateText));
            RaisePropertyChanged(nameof(TimeText));
            RaisePropertyChanged(nameof(DeadText));
            RaisePropertyChanged(nameof(ReachedGoalText));
            RaisePropertyChanged(nameof(GenerationText));
            RaisePropertyChanged(nameof(BestText));
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Start();
    }
}
