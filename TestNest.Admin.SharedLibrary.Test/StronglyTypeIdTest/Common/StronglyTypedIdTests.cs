using TestNest.Admin.SharedLibrary.StronglyTypeIds.Common;

namespace TestNest.Admin.SharedLibrary.Test.StronglyTypeIdTest.Common;

public record TestStronglyTypedId : StronglyTypedId<TestStronglyTypedId>
{
    public TestStronglyTypedId(Guid value) : base(value) { }
    public static TestStronglyTypedId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public class StronglyTypedIdTests
{
    [Fact]
    public void Constructor_Default_SetsNewGuid()
    {
        var id = new TestStronglyTypedId(Guid.NewGuid());
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Constructor_WithValue_SetsProvidedValue()
    {
        var testGuid = Guid.NewGuid();

        var id = new TestStronglyTypedId(testGuid);

        Assert.Equal(testGuid, id.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_SetsEmptyGuid()
    {
        var id = new TestStronglyTypedId(Guid.Empty);

        Assert.Equal(Guid.Empty, id.Value);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var testGuid = Guid.NewGuid();
        var id = new TestStronglyTypedId(testGuid);

        string result = id.ToString();

        Assert.Equal(testGuid.ToString(), result);
    }

    [Fact]
    public void ImplicitOperatorGuid_ReturnsInnerGuidValue()
    {
        var testGuid = Guid.NewGuid();
        var id = new TestStronglyTypedId(testGuid);

        Guid implicitGuid = id;

        Assert.Equal(testGuid, implicitGuid);
    }

    [Fact]
    public void Empty_ReturnsNewInstanceWithEmptyGuid()
    {
        var emptyId = TestStronglyTypedId.Empty();

        Assert.Equal(Guid.Empty, emptyId.Value);
    }

    [Fact]
    public void ImplicitOperatorString_ValidGuidString_ReturnsNewInstance()
    {
        // Arrange
        var testGuid = Guid.NewGuid();
        string guidString = testGuid.ToString();

        var id = (TestStronglyTypedId?)guidString;

        Assert.NotNull(id);
        Assert.Equal(testGuid, id!.Value);
    }

    [Fact]
    public void ImplicitOperatorString_InvalidGuidString_ReturnsNull()
    {
        string invalidGuidString = "not-a-guid";

        var id = (TestStronglyTypedId?)invalidGuidString;

        Assert.Null(id);
    }

    [Fact]
    public void New_ReturnsNewInstanceWithNewGuid()
    {
        var newId = TestStronglyTypedId.New();

        Assert.NotEqual(Guid.Empty, newId.Value);
    }

    [Fact]
    public void TryParse_ValidGuidString_ReturnsTrueAndSetsResult()
    {
        var testGuid = Guid.NewGuid();
        string guidString = testGuid.ToString();

        bool success = TestStronglyTypedId.TryParse(guidString, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(testGuid, result!.Value);
    }

    [Fact]
    public void TryParse_InvalidGuidString_ReturnsFalseAndSetsNullResult()
    {
        string invalidGuidString = "not-a-guid";

        bool success = TestStronglyTypedId.TryParse(invalidGuidString, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_ValidGuid_ReturnsTrueAndSetsResult()
    {
        var testGuid = Guid.NewGuid();

        bool success = TestStronglyTypedId.TryParse(testGuid, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(testGuid, result!.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var testGuid = Guid.NewGuid();
        var id1 = new TestStronglyTypedId(testGuid);
        var id2 = new TestStronglyTypedId(testGuid);

        bool result = id1.Equals(id2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var id1 = new TestStronglyTypedId(Guid.NewGuid());
        var id2 = new TestStronglyTypedId(Guid.NewGuid());

        bool result = id1.Equals(id2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_NullOther_ReturnsFalse()
    {
        var id = new TestStronglyTypedId(Guid.NewGuid());

        bool result = id.Equals(null);

        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        var testGuid = Guid.NewGuid();
        var id1 = new TestStronglyTypedId(testGuid);
        var id2 = new TestStronglyTypedId(testGuid);

        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentValue_ReturnsDifferentHashCode()
    {
        var id1 = new TestStronglyTypedId(Guid.NewGuid());
        var id2 = new TestStronglyTypedId(Guid.NewGuid());

        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void CompareTo_SameValue_ReturnsZero()
    {
        var testGuid = Guid.NewGuid();
        var id1 = new TestStronglyTypedId(testGuid);
        var id2 = new TestStronglyTypedId(testGuid);

        int result = id1.CompareTo(id2);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_SmallerValue_ReturnsNegativeOne()
    {
        var guid1 = new Guid("00000000-0000-0000-0000-000000000001");
        var guid2 = new Guid("00000000-0000-0000-0000-000000000002");
        var id1 = new TestStronglyTypedId(guid1);
        var id2 = new TestStronglyTypedId(guid2);

        int result = id1.CompareTo(id2);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void CompareTo_LargerValue_ReturnsPositiveOne()
    {
        var guid1 = new Guid("00000000-0000-0000-0000-000000000002");
        var guid2 = new Guid("00000000-0000-0000-0000-000000000001");
        var id1 = new TestStronglyTypedId(guid1);
        var id2 = new TestStronglyTypedId(guid2);

        int result = id1.CompareTo(id2);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CompareTo_OtherIsNull_ReturnsPositiveOne()
    {
        var id = new TestStronglyTypedId(Guid.NewGuid());

        int result = id.CompareTo(null);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CompareToObject_SameValue_ReturnsZero()
    {
        var testGuid = Guid.NewGuid();
        var id1 = new TestStronglyTypedId(testGuid);
        object id2 = new TestStronglyTypedId(testGuid);

        int result = id1.CompareTo((TestStronglyTypedId?)id2);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareToObject_DifferentValue_ReturnsNonZero()
    {
        var id1 = new TestStronglyTypedId(Guid.NewGuid());
        object id2 = new TestStronglyTypedId(Guid.NewGuid());

        int result = id1.CompareTo((TestStronglyTypedId?)id2);

        Assert.NotEqual(0, result);
    }

    [Fact]
    public void CompareToObject_NullObject_ReturnsPositiveOne()
    {
        var id = new TestStronglyTypedId(Guid.NewGuid());
        object? other = null;

        int result = id.CompareTo((TestStronglyTypedId?)other);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CompareToObject_DifferentObjectType_ThrowsInvalidCastException()
    {
        var id = new TestStronglyTypedId(Guid.NewGuid());
        object other = new();

        _ = Assert.Throws<InvalidCastException>(() => id.CompareTo((TestStronglyTypedId?)other));
    }
}
