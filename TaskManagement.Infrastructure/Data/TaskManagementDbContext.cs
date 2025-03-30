using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;

namespace TaskManagement.Infrastructure.Data
{
    public class TaskManagementDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity => { entity.ToTable("Users"); });
            builder.Entity<ApplicationRole>(entity => { entity.ToTable("Roles"); });
            builder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("UserRoles"); });

            builder.Ignore<IdentityUserClaim<int>>();
            builder.Ignore<IdentityUserLogin<int>>();
            builder.Ignore<IdentityUserToken<int>>();
            builder.Ignore<IdentityRoleClaim<int>>();

            builder.Entity<TaskEntity>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(t => t.AssignedUserId)
                    .IsRequired();

                entity.Property(t => t.CreatedAt)
                    .IsRequired()
                    .HasColumnType("DATETIME")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP()");

                entity.Property(t => t.DueDate)
                    .IsRequired();

                entity.HasOne(t => t.AssignedUser)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.AssignedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}