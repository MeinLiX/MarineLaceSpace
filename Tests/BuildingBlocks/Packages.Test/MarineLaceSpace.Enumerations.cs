using MarineLaceSpace.Enumerations;

namespace Packages.MarineLaceSpace.Enumerations.Tests;

public class TestEnumeration(int id, string name) : Enumeration(id, name)
{
    public static readonly TestEnumeration First = new(1, "First");
    public static readonly TestEnumeration Second = new(2, "Second");
    public static readonly TestEnumeration Third = new(3, "Third");
}

public class EnumerationTests
{
    [Fact]
    public void ToString_ReturnsName()
    {
        var enumeration = TestEnumeration.First;

        var result = enumeration.ToString();

        Assert.Equal("First", result);
    }

    [Fact]
    public void GetAll_ReturnsAllDefinedValues()
    {
        var enumerations = Enumeration.GetAll<TestEnumeration>().ToList();

        Assert.Equal(3, enumerations.Count);
        Assert.Contains(TestEnumeration.First, enumerations);
        Assert.Contains(TestEnumeration.Second, enumerations);
        Assert.Contains(TestEnumeration.Third, enumerations);
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForSameEnumerations()
    {
        var first1 = TestEnumeration.First;
        var first2 = TestEnumeration.First;

        var hashCode1 = first1.GetHashCode();
        var hashCode2 = first2.GetHashCode();

        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentValueForDifferentEnumerations()
    {
        var first = TestEnumeration.First;
        var second = TestEnumeration.Second;

        var hashCode1 = first.GetHashCode();
        var hashCode2 = second.GetHashCode();

        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_IEqualityComparer_ReturnsSameValueForSameEnumerations()
    {
        var comparer = TestEnumeration.First;
        var enumeration = TestEnumeration.First;

        var hashCode = comparer.GetHashCode(enumeration);

        Assert.Equal(enumeration.GetHashCode(), hashCode);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameReference()
    {
        var enumeration = TestEnumeration.First;

        Assert.True(enumeration.Equals(enumeration));
    }

    [Fact]
    public void Equals_ReturnsTrueForSameEnumeration()
    {
        var first1 = TestEnumeration.First;
        var first2 = TestEnumeration.First;

        Assert.True(first1.Equals(first2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentEnumerations()
    {
        var first = TestEnumeration.First;
        var second = TestEnumeration.Second;

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_ReturnsFalseForNull()
    {
        var enumeration = TestEnumeration.First;

        Assert.False(enumeration.Equals(null));
    }

    [Fact]
    public void Equals_IEqualityComparer_ReturnsTrueForSameEnumerations()
    {
        var comparer = TestEnumeration.First;
        var x = TestEnumeration.First;
        var y = TestEnumeration.First;

        Assert.True(comparer.Equals(x, y));
    }

    [Fact]
    public void Equals_IEqualityComparer_ReturnsFalseForDifferentEnumerations()
    {
        var comparer = TestEnumeration.First;
        var x = TestEnumeration.First;
        var y = TestEnumeration.Second;

        Assert.False(comparer.Equals(x, y));
    }

    [Fact]
    public void Equals_IEqualityComparer_HandlesNullValues()
    {
        var comparer = TestEnumeration.First;
        var enumeration = TestEnumeration.First;

        Assert.False(comparer.Equals(enumeration, null));
        Assert.False(comparer.Equals(null, enumeration));
        Assert.True(comparer.Equals(null, null));
    }

    [Fact]
    public void CompareTo_ReturnsPositiveForNull()
    {
        var enumeration = TestEnumeration.First;

        var result = enumeration.CompareTo(null);

        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_ReturnsZeroForSameId()
    {
        var first1 = TestEnumeration.First;
        var first2 = TestEnumeration.First;

        var result = first1.CompareTo(first2);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_ReturnsCorrectValueForDifferentIds()
    {
        var first = TestEnumeration.First;
        var second = TestEnumeration.Second;

        var result1 = first.CompareTo(second);
        var result2 = second.CompareTo(first);

        Assert.True(result1 < 0);
        Assert.True(result2 > 0);
    }

    [Fact]
    public void CompareTo_ThrowsForInvalidType()
    {
        var enumeration = TestEnumeration.First;
        var invalidObject = new object();

        Assert.Throws<ArgumentException>(() => enumeration.CompareTo(invalidObject));
    }

    [Fact]
    public void EqualityOperator_ReturnsTrueForSameEnumerations()
    {
        var first1 = TestEnumeration.First;
        var first2 = TestEnumeration.First;

        Assert.True(first1 == first2);
    }

    [Fact]
    public void EqualityOperator_ReturnsFalseForDifferentEnumerations()
    {
        var first = TestEnumeration.First;
        var second = TestEnumeration.Second;

        Assert.False(first == second);
    }

    [Fact]
    public void EqualityOperator_HandlesNullValues()
    {
        TestEnumeration? nullEnumeration = null;
        var enumeration = TestEnumeration.First;

        Assert.False(enumeration == nullEnumeration);
        Assert.False(nullEnumeration == enumeration);
        Assert.True(nullEnumeration == nullEnumeration);
    }

    [Fact]
    public void InequalityOperator_ReturnsFalseForSameEnumerations()
    {
        var first1 = TestEnumeration.First;
        var first2 = TestEnumeration.First;

        Assert.False(first1 != first2);
    }

    [Fact]
    public void InequalityOperator_ReturnsTrueForDifferentEnumerations()
    {
        var first = TestEnumeration.First;
        var second = TestEnumeration.Second;

        Assert.True(first != second);
    }

    [Fact]
    public void InequalityOperator_HandlesNullValues()
    {
        TestEnumeration? nullEnumeration = null;
        var enumeration = TestEnumeration.First;

        Assert.True(enumeration != nullEnumeration);
        Assert.True(nullEnumeration != enumeration);
        Assert.False(nullEnumeration != nullEnumeration);
    }

    [Fact]
    public void FromId_ReturnsCorrectEnumeration()
    {
        var result = Enumeration.FromId<TestEnumeration>(1);

        Assert.Equal(TestEnumeration.First, result);
    }

    [Fact]
    public void FromId_ReturnsNullForInvalidId()
    {
        var result = Enumeration.FromId<TestEnumeration>(999);

        Assert.Null(result);
    }

    [Fact]
    public void FromName_ReturnsCorrectEnumeration()
    {
        var result = Enumeration.FromName<TestEnumeration>("Second");

        Assert.Equal(TestEnumeration.Second, result);
    }

    [Fact]
    public void FromName_ReturnsNullForInvalidName()
    {
        var result = Enumeration.FromName<TestEnumeration>("NonExistent");

        Assert.Null(result);
    }

    [Fact]
    public void TryFromId_ReturnsTrueAndSetsResultForValidId()
    {
        var success = Enumeration.TryFromId<TestEnumeration>(2, out var result);

        Assert.True(success);
        Assert.Equal(TestEnumeration.Second, result);
    }

    [Fact]
    public void TryFromId_ReturnsFalseAndSetsNullForInvalidId()
    {
        var success = Enumeration.TryFromId<TestEnumeration>(999, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromName_ReturnsTrueAndSetsResultForValidName()
    {
        var success = Enumeration.TryFromName<TestEnumeration>("Third", out var result);

        Assert.True(success);
        Assert.Equal(TestEnumeration.Third, result);
    }

    [Fact]
    public void TryFromName_ReturnsFalseAndSetsNullForInvalidName()
    {
        var success = Enumeration.TryFromName<TestEnumeration>("NonExistent", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void IEqualityComparer_CanBeUsedWithCollections()
    {
        var dictionary = new Dictionary<TestEnumeration, string>
            {
                { TestEnumeration.First, "First Value" },
                { TestEnumeration.Second, "Second Value" }
            };

        Assert.Equal(2, dictionary.Count);
        Assert.Equal("First Value", dictionary[TestEnumeration.First]);
        Assert.Equal("Second Value", dictionary[TestEnumeration.Second]);
    }
}
