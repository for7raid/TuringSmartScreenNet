using SkiaSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TuringSmartScreenNet
{
    public class ConwayLifeV8
    {
        const int ROWS = 20;
        const int COLS = 60;
        const int Capacity = ROWS * COLS;

        const int ImageCellPixels = 5;

        private readonly Queue<int[]> _lastTurns;
        private Cell[] _bank;

        private object _locker = new object();

        public int Age { get; private set; }
        public bool IsFinished { get; private set; }

        public ConwayLifeV8()
        {
            _bank = new Cell[Capacity];
            _lastTurns = new Queue<int[]>(4);
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < _bank.Length; i++)
            {
                _bank[i] = new(Random.Shared.Next() > int.MaxValue / 2 ? 1 : 0, 0);
            }
            IsFinished = false;
            Age = 0;
            _lastTurns.Clear();
        }

        public ImageSource Next()
        {
            lock (_locker)
            {
                if (IsFinished)
                {
                    Reset();
                }

                MakeTurn();
                return GetImage();
            }
        }
        private void MakeTurn()
        {
            if (IsFinished) return;

            var newBank = new Cell[Capacity];
            var changedCells = new List<int>();
            var hasChanges = false;

            for (int cell = 0; cell < Capacity; cell++)
            {
                int row = cell / COLS;
                int col = cell % COLS;

                // Получаем индексы соседей с кольцевой адресацией
                int prevRow = (row - 1 + ROWS) % ROWS;
                int nextRow = (row + 1) % ROWS;
                int prevCol = (col - 1 + COLS) % COLS;
                int nextCol = (col + 1) % COLS;

                // Получаем значения соседей
                var cellA = _bank[prevRow * COLS + prevCol];
                var cellB = _bank[prevRow * COLS + col];
                var cellC = _bank[prevRow * COLS + nextCol];
                var cellD = _bank[row * COLS + prevCol];
                var cellE = _bank[row * COLS + nextCol];
                var cellF = _bank[nextRow * COLS + prevCol];
                var cellG = _bank[nextRow * COLS + col];
                var cellH = _bank[nextRow * COLS + nextCol];

                var neighbours = new[] { cellA, cellB, cellC, cellD, cellE, cellF, cellG, cellH };
                var nCount = neighbours.Count(x => x.generation > 0);

                var current = _bank[cell];

                if (current.generation == 0 && nCount == 3)
                {
                    newBank[cell] = new(neighbours.Max(x => x.generation) + 1, 0);
                }
                else if (current.generation > 0 && (nCount == 2 || nCount == 3))
                {
                    newBank[cell] = new(_bank[cell].generation, _bank[cell].age + 1);
                }
                //else if (current.age > 255) //умирает от старости и неизменности обстановки
                //{
                //    newBank[cell] = new(0, 0);
                //}
                else
                {
                    newBank[cell] = new(0, 0);
                }

                var isCellChanged = _bank[cell].generation != newBank[cell].generation;

                if (isCellChanged)
                {
                    changedCells.Add(cell);
                    hasChanges = true;
                }

            }

            IsFinished = !hasChanges || Age > 10000;

            foreach (var turn in _lastTurns.ToArray())
            {
                if (turn.SequenceEqual(changedCells))
                {
                    IsFinished = true; break;
                }
            }

            _lastTurns.Enqueue(changedCells.ToArray());
            if (_lastTurns.Count > 3)
            {
                _lastTurns.Dequeue();
            }

            Age++;
            _bank = newBank;
        }

        private ImageSource GetImage()
        {
            var imageInfo = new SKImageInfo(COLS * ImageCellPixels, ROWS * ImageCellPixels);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            for (int cell = 0; cell < Capacity; cell++)
            {
                int currentRow = cell / COLS;
                int currentColInRow = cell % COLS;

                using var paintCell = new SKPaint
                {
                    Color = FromHsv(_bank[cell].generation, _bank[cell].age)
                };

                canvas.DrawRect(
                    x: currentColInRow * ImageCellPixels,
                    y: currentRow * ImageCellPixels,
                    w: ImageCellPixels,
                    h: ImageCellPixels,
                    paint: paintCell);


            }

            using var paintWhite = new SKPaint
            {
                Color = SKColors.White,
            };
            using var font = new SKFont
            {
                Size = 14,
                Typeface = SKTypeface.FromFamilyName("Arial"),
            };

            canvas.DrawText(Age.ToString(), x: 5, y: 20, font: font, paint: paintWhite);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = data.AsStream();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        private static SKColor FromHsv(double value, int age)
        {
            if (value < 1) return SKColors.Black;

            byte alfa = (byte)Math.Min(255, Math.Max(40, age));

            var tr = (float)Math.Min(1, value / 2000);
            float hue = 70 + tr * 290f; // 0..270

            return SKColor.FromHsv(hue, 100f, 100f, 255);

        }

        public void Up()
        {
            lock (_locker)
            {
                var newBank = new Cell[Capacity];

                new Span<Cell>(_bank, COLS, Capacity - COLS).CopyTo(newBank);
                new Span<Cell>(_bank, 0, COLS).CopyTo(new Span<Cell>(newBank, Capacity - COLS, COLS));


                _bank = newBank;
            }
        }
        public void Down()
        {
            lock (_locker)
            {
                var newBank = new Cell[Capacity];

                new Span<Cell>(_bank, 0, Capacity - COLS).CopyTo(new Span<Cell>(newBank, COLS, Capacity - COLS));
                new Span<Cell>(_bank, Capacity - COLS, COLS).CopyTo(newBank);

                _bank = newBank;
            }
        }

        public void Left()
        {
            lock (_locker)
            {
                var newBank = new Cell[Capacity];

                for (int i = 0; i < ROWS; i++)
                {
                    var firstCell = i * COLS;

                    new Span<Cell>(_bank, firstCell + 1, COLS - 1).CopyTo(new Span<Cell>(newBank, firstCell, COLS - 1));
                    newBank[firstCell + COLS - 1] = _bank[firstCell];
                }

                _bank = newBank;
            }
        }
        public void Right()
        {
            lock (_locker)
            {
                var newBank = new Cell[Capacity];

                for (int i = 0; i < ROWS; i++)
                {
                    var firstCell = i * COLS;

                    new Span<Cell>(_bank, firstCell, COLS - 1).CopyTo(new Span<Cell>(newBank, firstCell + 1, COLS - 1));
                    newBank[firstCell] = _bank[firstCell + COLS - 1];
                }

                _bank = newBank;
            }
        }

        private struct Cell
        {
            public int generation;
            public int age;
            public Cell(int generation, int age)
            {
                this.generation = generation;
                this.age = age;
            }
        }
    }


}
