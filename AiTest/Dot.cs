using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    public class Dot
    {
        private const int MinSpeed = 50;
        private const int MaxSpeed = 100;
        private const double DotRadius = 5;
        private const int DirectionChangeMs = 100;

        private static readonly Brush AliveColor = Brushes.Green;
        private static readonly Brush DeadColor = Brushes.DarkGray;
        private static readonly Brush WinnerColor = Brushes.HotPink;
        private static readonly Random random = new Random();

        private readonly Brain brain;
        private readonly Canvas canvas;

        private DateTime lastMove = DateTime.Now;
        private DateTime lastDirectionChange = DateTime.Now;

        public Point Position { get; set; }
        public Point Center => new Point(Position.X + (DotRadius / 2), Position.Y + (DotRadius / 2));
        public VelocityVector Velocity { get; set; }
        public Ellipse Ellipse { get; set; }
        public Line Line { get; set; }
        public double Fitness { get; set; }

        private bool dead = false;
        public bool Dead
        {
            get => dead;
            set
            {
                dead = value;
                canvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Ellipse.Fill = dead ? DeadColor : AliveColor;
                    Ellipse.Stroke = dead ? DeadColor : AliveColor;
                    Line.Stroke = dead ? DeadColor : AliveColor;
                }));
            }
        }

        public Dot(Canvas canvas)
        {
            brain = new Brain(300);
            this.canvas = canvas;

            var x = random.Next(Convert.ToInt32(canvas.ActualWidth - 5.0d));
            var y = random.Next(Convert.ToInt32((canvas.ActualHeight / 4.0d) - 5.0d)) + (canvas.ActualHeight * 0.75d);
            Position = new Point(x, y); ;

            Velocity = new VelocityVector
            {
                Speed = (random.NextDouble() * (MaxSpeed - MinSpeed)) + MinSpeed,
                Direction = 90
            };

            Ellipse = new Ellipse
            {
                Height = DotRadius,
                Width = DotRadius,
                Stroke = AliveColor,
                Fill = AliveColor
            };

            canvas.Children.Add(Ellipse);

            Line = new Line()
            {
                Stroke = AliveColor,
                StrokeThickness = 1,
                X1 = Position.X,
                X2 = Position.X + Velocity.Speed * Math.Cos(Velocity.DirectionInRadians),
                Y1 = Position.Y,
                Y2 = Position.Y - Velocity.Speed * Math.Sin(Velocity.DirectionInRadians)
            };

            canvas.Children.Add(Line);
        }

        public void Draw()
        {
            if (!Dead)
            {
                canvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Canvas.SetLeft(Ellipse, Position.X);
                    Canvas.SetTop(Ellipse, Position.Y);

                    Line.X1 = Position.X + (DotRadius / 2);
                    Line.X2 = Position.X + Velocity.Speed * Math.Cos(Velocity.DirectionInRadians);
                    Line.Y1 = Position.Y + (DotRadius / 2);
                    Line.Y2 = Position.Y - Velocity.Speed * Math.Sin(Velocity.DirectionInRadians);
                }));
            }
        }

        public void Move()
        {
            var timeSinceLastMove = DateTime.Now - lastMove;
            lastMove = DateTime.Now;

            var distance = Velocity.Speed * timeSinceLastMove.TotalSeconds;

            var x = distance * Math.Cos(Velocity.DirectionInRadians);
            var y = -distance * Math.Sin(Velocity.DirectionInRadians);
            Position = new Point(Position.X + x, Position.Y + y);

            if (Position.X < 0 ||
                Position.X > canvas.ActualWidth - 5 ||
                Position.Y < 0 ||
                Position.Y > canvas.ActualHeight - 5)
            {
                Dead = true;
                return;
            }

            var timeSinceLastDirectionChange = DateTime.Now - lastDirectionChange;
            if (timeSinceLastDirectionChange.TotalMilliseconds > DirectionChangeMs)
            {
                var direction = brain.NextDirection();
                if (direction.HasValue)
                {
                    Velocity.Direction += direction.Value;
                    lastDirectionChange = DateTime.Now;
                }
                else
                {
                    Dead = true;
                    return;
                }
            }
        }

        public void Highlight()
        {
            canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                Ellipse.Stroke = WinnerColor;
                Ellipse.Fill = WinnerColor;
                Line.Stroke = WinnerColor;
            }));
        }
    }
}
