namespace Checkers
{
    public record BoardCoordinate(int Row, int Column)
    {
        public int Row { get; } = Row;
        public int Column { get; } = Column;
    }

    public static class BoardCoordinateExtensions
    {
        public const string BoardCoordinatePattern = @"(\d+),(\d+)";

        // +1 для читаемости
        public static string ToLogString(this BoardCoordinate coordinate)
            => $"[{coordinate.Row + 1},{coordinate.Column + 1}]";

        // -1 для работы с индексами
        public static BoardCoordinate ToBoardCoordinate(this (string rowString, string columnString) values)
            => new(int.Parse(values.rowString) - 1,
                int.Parse(values.columnString) - 1);
    }
}