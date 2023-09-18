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
        public static string ToLogString(this BoardCoordinate coordinate)
            => $"[{coordinate.Row},{coordinate.Column}]";
    }
}