using System.Reflection;

namespace MarineLaceSpace.Enumerations;

public abstract class Enumeration(int id, string name) : IComparable,
                                                         IEquatable<Enumeration>,
                                                         IEqualityComparer<Enumeration>
{
    public string Name { get; } = name;
    public int Id { get; } = id;

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var fields = typeof(T).GetFields(BindingFlags.Public |
                                        BindingFlags.Static |
                                        BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null))
                     .OfType<T>();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType().ToString(), Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Enumeration otherValue &&
               GetType() == obj.GetType() &&
               Id == otherValue.Id;
    }

    public bool Equals(Enumeration? other)
    {
        return other is not null &&
               GetType() == other.GetType() &&
               Id == other.Id;
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        return obj is Enumeration otherValue
               ? Id.CompareTo(otherValue.Id)
               : throw new ArgumentException($"Object must be of type {nameof(Enumeration)}");
    }

    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration? left, Enumeration? right)
    {
        return !(left == right);
    }

    public static T? FromId<T>(int id) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(item => item.Id == id);
    }

    public static T? FromName<T>(string name) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(item => item.Name == name);
    }

    public static bool TryFromId<T>(int id, out T? result) where T : Enumeration
    {
        result = FromId<T>(id);
        return result is not null;
    }

    public static bool TryFromName<T>(string name, out T? result) where T : Enumeration
    {
        result = FromName<T>(name);
        return result is not null;
    }

    public bool Equals(Enumeration? x, Enumeration? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;

        return x.GetType() == y.GetType() && x.Id == y.Id;
    }

    public int GetHashCode(Enumeration obj)
    {
        if (obj is null)
            return 0;

        return HashCode.Combine(obj.GetType().ToString(), obj.Id);
    }
}
