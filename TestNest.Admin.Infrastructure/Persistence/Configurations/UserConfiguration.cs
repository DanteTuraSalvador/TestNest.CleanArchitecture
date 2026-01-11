using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestNest.Admin.Domain.Users;
using TestNest.Admin.Infrastructure.Persistence.Configurations.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id)
            .IsClustered(false);

        builder.Property(u => u.Id)
            .ConfigureStronglyTypedId<UserId>()
            .HasColumnName("UserId")
            .HasColumnType("UNIQUEIDENTIFIER")
            .IsRequired();

        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Email)
                .HasColumnName("Email")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(256)
                .IsRequired();

            emailBuilder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        builder.Property(u => u.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasColumnType("NVARCHAR")
            .HasMaxLength(256)
            .IsRequired();

        builder.OwnsOne(u => u.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.MiddleName)
                .HasColumnName("MiddleName")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(100);

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasColumnType("NVARCHAR")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(u => u.RefreshToken)
            .HasColumnName("RefreshToken")
            .HasColumnType("NVARCHAR")
            .HasMaxLength(256);

        builder.Property(u => u.RefreshTokenExpiryTime)
            .HasColumnName("RefreshTokenExpiryTime")
            .HasColumnType("DATETIME2");

        builder.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("BIT")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("LastLoginAt")
            .HasColumnType("DATETIME2");

        builder.Property(u => u.EmployeeId)
            .ConfigureStronglyTypedId<EmployeeId>()
            .HasColumnName("EmployeeId")
            .HasColumnType("UNIQUEIDENTIFIER");

        builder.Property(u => u.Role)
            .HasColumnName("Role")
            .HasColumnType("NVARCHAR")
            .HasMaxLength(50);

        builder.HasIndex(u => u.RefreshToken)
            .HasDatabaseName("IX_Users_RefreshToken");
    }
}
