using System.Reflection;

namespace MarineLaceSpace.Enumerations;

public abstract class Enumeration(int id, string name) : IComparable, IEquatable<Enumeration>
{
    public string Name { get; } = name;
    public int Id { get; } = id;

    [Obsolete]
    private string GetNameFromCallingMember()
    {
        var stackFrame = new System.Diagnostics.StackFrame(2, false);
        var method = stackFrame.GetMethod();

        if (method?.Name == ".cctor" || method?.Name == ".ctor")
        {
            var declaringType = method.DeclaringType;
            if (declaringType != null)
            {
                var fields = declaringType.GetFields(BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Static |
                                                   BindingFlags.DeclaredOnly);

                foreach (var field in fields)
                {
                    if (!field.IsStatic) continue;

                    var value = field.GetValue(null);
                    if (ReferenceEquals(this, value))
                    {
                        return field.Name;
                    }
                }
            }
        }

        throw new ArgumentNullException("name");
    }

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

    public int CompareTo(object? other)
    {
        if (other is null) return 1;
        return other is Enumeration otherValue
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
}
