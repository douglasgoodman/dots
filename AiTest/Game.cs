using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    public class Game
    {
        private const int GoalRadius = 30;

        private static readonly Brush GoalColor = Brushes.Yellow;

        private readonly int dotCount;

        private Canvas canvas;
        private List<Dot> dots = new List<Dot>();
        private Ellipse goalEllipse;
        private Point goalPosition;
        private Point goalCenter;
        private Dot winner;

        public event EventHandler FrameHappened;
        public event EventHandler Finished;

        public int DeadDotCount => dots.Count(d => d.Dead);
        public double FrameRate { get; private set; }
        public double MaxFitness { get; private set; }

        public Game(int count, Canvas canvas)
        {
            dotCount = count;
            this.canvas = canvas;

            var x = canvas.ActualWidth / 2.0d;
            var y = canvas.ActualHeight / 10.0d;
            goalCenter = new Point(x, y);

            goalPosition = new Point(goalCenter.X - (GoalRadius / 2d), goalCenter.Y - (GoalRadius / 2d));

            for (int i = 0; i < dotCount; i++)
            {
                dots.Add(new Dot(canvas));
            }
        }

        public void Start()
        {
            winner = null;
            PlaceGoal();
            Task.Run(() => Loop());
        }

        private void PlaceGoal()
        {
            goalEllipse = new Ellipse
            {
                Stroke = GoalColor,
                Fill = GoalColor,
                Height = GoalRadius,
                Width = GoalRadius,
            };

            canvas.Children.Add(goalEllipse);

            Canvas.SetLeft(goalEllipse, goalPosition.X);
            Canvas.SetTop(goalEllipse, goalPosition.Y);
        }

        public async Task Loop()
        {
            while (DeadDotCount < dotCount && winner == null)
            {
                var frameStart = DateTime.Now;

                MoveDots();
                DrawDots();
                CalculateFitness();
                if (winner != null)
                {
                    DoWinnerStuff();
                }
                FrameHappened?.Invoke(this, EventArgs.Empty);
                await Task.Delay(1);

                var frameTime = DateTime.Now - frameStart;
                FrameRate = 1000 / frameTime.TotalMilliseconds;
            }

            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void DoWinnerStuff()
        {
            dots.Where(d => d != winner).ToList().ForEach(d => d.Dead = true);
            winner.Highlight();

            canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                goalEllipse.Stroke = Brushes.White;
                goalEllipse.Fill = Brushes.White;
            }));
        }

        private void CalculateFitness()
        {
            MaxFitness = double.MinValue;

            dots.ForEach(d =>
            {
                var distance = goalCenter - d.Center;
                if (distance.Length < (GoalRadius / 2d))
                {
                    winner = d;
                }

                if (distance.Length == 0)
                {
                    d.Fitness = 1;
                }
                else
                {
                    d.Fitness = 1.0d / distance.Length;
                }
                MaxFitness = Math.Max(MaxFitness, d.Fitness);
            });
        }

        private void MoveDots() => dots.ForEach(d => d.Move());

        private void DrawDots() => dots.ForEach(d => d.Draw());
    }
}
