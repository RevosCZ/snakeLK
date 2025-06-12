using System;
using System.Collections.Generic;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        static void Main()
        {
            var game = new Game(new ConsoleRenderer(), new ConsoleInput());
            game.Run();
        }
    }

    public class Game
    {
        private readonly IRenderer _renderer;
        private readonly IInput _input;
        private readonly Snake _snake;
        private readonly Food _food;
        private readonly int _width = 32;
        private readonly int _height = 16;
        private Direction _direction = Direction.Right;
        private int _score = 5;
        private bool _isGameOver = false;

        public Game(IRenderer renderer, IInput input)
        {
            _renderer = renderer;
            _input = input;
            _snake = new Snake(new Position(_width / 2, _height / 2));
            _food = new Food(_width, _height);
        }

        public void Run()
        {
            Console.SetWindowSize(_width, _height);
            Console.CursorVisible = false;

            while (!_isGameOver)
            {
                _renderer.Clear();
                _renderer.DrawBorders(_width, _height);

                HandleInput();
                Update();
                Render();

                Thread.Sleep(100);
            }

            _renderer.ShowGameOver(_score, _width, _height);
        }

        private void HandleInput()
        {
            Direction? inputDirection = _input.GetDirection();
            if (inputDirection.HasValue && !_snake.IsOppositeDirection(inputDirection.Value))
            {
                _direction = inputDirection.Value;
            }
        }

        private void Update()
        {
            _snake.Move(_direction);

            if (_snake.Head.X == 0 || _snake.Head.X == _width - 1 ||
                _snake.Head.Y == 0 || _snake.Head.Y == _height - 1 ||
                _snake.HasCollision())
            {
                _isGameOver = true;
                return;
            }

            if (_snake.Head.Equals(_food.Position))
            {
                _score++;
                _snake.Grow();
                _food.Respawn(_width, _height);
            }
        }

        private void Render()
        {
            _renderer.Draw(_food);
            _renderer.Draw(_snake);
        }
    }

    public interface IRenderer
    {
        void Clear();
        void Draw(Snake snake);
        void Draw(Food food);
        void DrawBorders(int width, int height);
        void ShowGameOver(int score, int width, int height);
    }

    public class ConsoleRenderer : IRenderer
    {
        public void Clear() => Console.Clear();

        public void Draw(Snake snake)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var segment in snake.Body)
            {
                Console.SetCursorPosition(segment.X, segment.Y);
                Console.Write("■");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(snake.Head.X, snake.Head.Y);
            Console.Write("■");
        }

        public void Draw(Food food)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(food.Position.X, food.Position.Y);
            Console.Write("■");
        }

        public void DrawBorders(int width, int height)
        {
            Console.ForegroundColor = ConsoleColor.White;

            for (int x = 0; x < width; x++)
            {
                Console.SetCursorPosition(x, 0);
                Console.Write("■");
                Console.SetCursorPosition(x, height - 1);
                Console.Write("■");
            }

            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write("■");
                Console.SetCursorPosition(width - 1, y);
                Console.Write("■");
            }
        }

        public void ShowGameOver(int score, int width, int height)
        {
            Console.SetCursorPosition(width / 5, height / 2);
            Console.WriteLine($"Game Over. Score: {score}");
        }
    }

    public interface IInput
    {
        Direction? GetDirection();
    }

    public class ConsoleInput : IInput
    {
        public Direction? GetDirection()
        {
            if (!Console.KeyAvailable) return null;

            var key = Console.ReadKey(true).Key;

            return key switch
            {
                ConsoleKey.UpArrow => Direction.Up,
                ConsoleKey.DownArrow => Direction.Down,
                ConsoleKey.LeftArrow => Direction.Left,
                ConsoleKey.RightArrow => Direction.Right,
                _ => null
            };
        }
    }

    public class Snake
    {
        public LinkedList<Position> Body { get; }
        public Position Head => Body.Last.Value;
        private int _growAmount = 0;

        public Snake(Position start)
        {
            Body = new LinkedList<Position>();
            Body.AddLast(start);
        }

        public void Move(Direction direction)
        {
            var newHead = Head.Move(direction);
            Body.AddLast(newHead);

            if (_growAmount > 0)
                _growAmount--;
            else
                Body.RemoveFirst();
        }

        public void Grow() => _growAmount++;

        public bool HasCollision()
        {
            foreach (var segment in Body)
            {
                if (!segment.Equals(Head) && segment.Equals(Head))
                    return true;
            }
            return false;
        }

        public bool IsOppositeDirection(Direction newDirection)
        {
            return Head.Move(newDirection).Equals(Body.Last.Previous?.Value);
        }
    }

    public class Food
    {
        private readonly Random _random = new();
        public Position Position { get; private set; }

        public Food(int width, int height)
        {
            Respawn(width, height);
        }

        public void Respawn(int width, int height)
        {
            Position = new Position(
                _random.Next(1, width - 2),
                _random.Next(1, height - 2)
            );
        }
    }

    public record Position(int X, int Y)
    {
        public Position Move(Direction direction) => direction switch
        {
            Direction.Up => this with { Y = Y - 1 },
            Direction.Down => this with { Y = Y + 1 },
            Direction.Left => this with { X = X - 1 },
            Direction.Right => this with { X = X + 1 },
            _ => this
        };
    }

    public enum Direction
    {
        Up, Down, Left, Right
    }
}
