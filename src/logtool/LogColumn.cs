namespace logtool;

public class LogColumn : IEquatable<LogColumn>
{
    public LogColumn(
        int index,
        string name)
    {
        Index = index;
        Name = name;
    }

    public int Index { get; }

    public string Name { get; }

    public bool Equals(LogColumn? other) =>
        other is not null && (ReferenceEquals(this, other) || other.Name == Name);

    public override bool Equals(object? o) =>
        o is not null && o is LogColumn c && Equals(c);

    public override int GetHashCode() =>
        Name.GetHashCode();
}
