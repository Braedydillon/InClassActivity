using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentTeacherDatabaseApproach.Models;

namespace StudentTeacherDatabaseApproach.Data;

public partial class StudentTeacherDbContext : DbContext
{
    public StudentTeacherDbContext(DbContextOptions<StudentTeacherDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseResult> CourseResults { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<TeacherProfile> TeacherProfiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // Map the entity classes to the existing SQL Server tables, keys and relationships.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7FEC772C0");

            entity.HasIndex(e => e.Code, "UQ__Courses__A25C5AA7600CCCE3").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.CourseImageUrl).HasMaxLength(2048);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.CourseCategory).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__CourseC__4AB81AF0");

            entity.HasOne(d => d.TeacherProfile).WithMany(p => p.Courses)
                .HasForeignKey(d => d.TeacherProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__Teacher__49C3F6B7");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.CourseCategoryId).HasName("PK__Course_C__4D67EBB6DB605209");

            entity.ToTable("Course_Categories");

            entity.HasIndex(e => e.Name, "UQ__Course_C__737584F65A17D7F2").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CourseResult>(entity =>
        {
            entity.HasKey(e => e.CourseResultId).HasName("PK__Course_R__D642BA9FD46A9450");

            entity.ToTable("Course_Results");

            // The unique enrolment key allows at most one result per enrolment.
            entity.HasIndex(e => e.EnrolmentId, "UQ__Course_R__5C0E5FCEB5D92F08").IsUnique();

            entity.Property(e => e.FinalMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Outcome).HasMaxLength(10);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Enrolment).WithOne(p => p.CourseResult)
                .HasForeignKey<CourseResult>(d => d.EnrolmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Course_Re__Enrol__59063A47");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId).HasName("PK__Enrolmen__5C0E5FCFCBC87291");

            // The combined unique key prevents a student from enrolling in the same course twice.
            entity.HasIndex(e => new { e.StudentProfileId, e.CourseId }, "UQ__Enrolmen__4EB907AB79EDE4C1").IsUnique();

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrolment__Cours__52593CB8");

            entity.HasOne(d => d.StudentProfile).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.StudentProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrolment__Stude__5165187F");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.StudentProfileId).HasName("PK__Student___222BD0B0AE546EAB");

            entity.ToTable("Student_Profiles");

            entity.HasIndex(e => e.UserId, "UQ__Student___1788CC4D52D5A86F").IsUnique();

            entity.HasIndex(e => e.StudentNumber, "UQ__Student___DD81BF6C68599ED1").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(2048);
            entity.Property(e => e.StudentNumber).HasMaxLength(30);

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_P__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<TeacherProfile>(entity =>
        {
            entity.HasKey(e => e.TeacherProfileId).HasName("PK__Teacher___6414496D8722E798");

            entity.ToTable("Teacher_Profiles");

            entity.HasIndex(e => e.UserId, "UQ__Teacher___1788CC4D6088A571").IsUnique();

            entity.HasIndex(e => e.EmployeeNumber, "UQ__Teacher___8D66359864162BC8").IsUnique();

            entity.Property(e => e.EmployeeNumber).HasMaxLength(30);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(2048);

            entity.HasOne(d => d.User).WithOne(p => p.TeacherProfile)
                .HasForeignKey<TeacherProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teacher_P__UserI__4222D4EF");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CB90D1734");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053475195FE7").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
