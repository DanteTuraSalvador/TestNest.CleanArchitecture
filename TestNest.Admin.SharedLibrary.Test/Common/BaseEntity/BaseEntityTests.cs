using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TestNest.Admin.SharedLibrary.Common.BaseEntity;
using TestNest.Admin.SharedLibrary.StronglyTypeIds.Common;

namespace TestNest.Admin.SharedLibrary.Test.Common.BaseEntity;

public record TestId : StronglyTypedId<TestId>
{
    public TestId() : base() { }
    public TestId(Guid value) : base(value) { }
}

public class TestEntity : BaseEntity<TestId>
{
    public TestEntity() : base()
    {
    }

    public TestEntity(TestId id) : base(id)
    {
    }
}

public class BaseEntityTests
{
    [Fact]
    public void Constructor_WithId_SetsIdProperty()
    {
        var testId = TestId.New();

        var entity = new TestEntity(testId);

        Assert.Equal(testId, entity.Id);
    }

    [Fact]
    public void Constructor_WithNullId_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => new TestEntity(null!));

    [Fact]
    public void DefaultConstructor_SetsIdToNull()
    {
        var entity = new TestEntity();

        Assert.Null(entity.Id);
    }

    [Fact]
    public void Id_Property_HasRequiredAttribute()
    {
        PropertyInfo? property = typeof(BaseEntity<TestId>).GetProperty(nameof(BaseEntity<TestId>.Id));

        object[]? attribute = property?.GetCustomAttributes(typeof(RequiredAttribute), true);

        Assert.NotNull(attribute);
        Assert.NotEmpty(attribute);
    }

    [Fact]
    public void Equals_WithSameIdAndType_ReturnsTrue()
    {
        var id = TestId.New();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1 == entity2);
        Assert.False(entity1 != entity2);
    }

    [Fact]
    public void Equals_WithDifferentIds_ReturnsFalse()
    {
        var entity1 = new TestEntity(TestId.New());
        var entity2 = new TestEntity(TestId.New());

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void Equals_WithDifferentTypes_ReturnsFalse()
    {
        var id = TestId.New();
        var entity1 = new TestEntity(id);

        var entity2 = new OtherTestEntity(id);

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var entity = new TestEntity(TestId.New());

        Assert.False(entity.Equals(null));
        Assert.False(entity == null);
        Assert.False(null == entity);
        Assert.True(entity != null);
        Assert.True(null != entity);
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        var entity = new TestEntity(TestId.New());

        Assert.True(entity.Equals(entity));
        Assert.True(entity == entity);
        Assert.False(entity != entity);
    }

    [Fact]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var id = TestId.New();
        var entity = new TestEntity(id);

        int hash1 = entity.GetHashCode();
        int hash2 = entity.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ForEqualObjects_ReturnsSameValue()
    {
        var id = TestId.New();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        int hash1 = entity1.GetHashCode();
        int hash2 = entity2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ForDifferentObjects_ReturnsDifferentValues()
    {
        var entity1 = new TestEntity(TestId.New());
        var entity2 = new TestEntity(TestId.New());

        int hash1 = entity1.GetHashCode();
        int hash2 = entity2.GetHashCode();

        Assert.NotEqual(hash1, hash2);
    }

    private class OtherTestEntity(TestId id) : BaseEntity<TestId>(id)
    {
    }
}
