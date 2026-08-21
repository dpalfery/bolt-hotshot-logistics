// <copyright file="JobAssignmentTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Tests.Jobs
{
    /// <summary>
    ///     Tests for the JobAssignment class.
    /// </summary>
    public class JobAssignmentTests
    {
        /// <summary>
        ///     Tests that the default constructor sets appropriate default values.
        /// </summary>
        [Fact]
        public void Default_Constructor_Sets_Defaults()
        {
            JobAssignment assignment = new();
            Assert.False(string.IsNullOrWhiteSpace(assignment.Id));
            Assert.Equal(string.Empty, assignment.JobId);
            Assert.Equal(0, assignment.DriverId);
            Assert.True((DateTime.UtcNow - assignment.AssignedAt).TotalSeconds < 5); // AssignedAt is now
            Assert.Equal(JobAssignmentStatus.Active, assignment.Status);
            Assert.Null(assignment.UpdatedAt);
        }

        /// <summary>
        ///     Tests that properties can be assigned values.
        /// </summary>
        [Fact]
        public void Can_Assign_Properties()
        {
            JobAssignment assignment = new()
            {
                Id = "test-id",
                JobId = "job-123",
                DriverId = 42,
                AssignedAt = new DateTime(2024, 6, 16, 12, 0, 0, DateTimeKind.Utc),
                Status = JobAssignmentStatus.Completed,
                UpdatedAt = new DateTime(2024, 6, 17, 8, 0, 0, DateTimeKind.Utc)
            };
            Assert.Equal("test-id", assignment.Id);
            Assert.Equal("job-123", assignment.JobId);
            Assert.Equal(42, assignment.DriverId);
            Assert.Equal(new DateTime(2024, 6, 16, 12, 0, 0, DateTimeKind.Utc), assignment.AssignedAt);
            Assert.Equal(JobAssignmentStatus.Completed, assignment.Status);
            Assert.Equal(new DateTime(2024, 6, 17, 8, 0, 0, DateTimeKind.Utc), assignment.UpdatedAt);
        }

        /// <summary>
        ///     Tests that the status can be transitioned.
        /// </summary>
        [Fact]
        public void Can_Transition_Status()
        {
            JobAssignment assignment = new();
            Assert.Equal(JobAssignmentStatus.Active, assignment.Status);
            assignment.Status = JobAssignmentStatus.Completed;
            Assert.Equal(JobAssignmentStatus.Completed, assignment.Status);
        }

        /// <summary>
        ///     Tests that navigation properties can be set.
        /// </summary>
        [Fact]
        public void Can_Set_Navigation_Properties()
        {
            Job job = new() { Id = "job-1", Title = "Test Job" };
            Driver driver = new()
            {
                Id = 1,
                PersonalInfo = new PersonalInfo { FirstName = "Alice", LastName = "Smith" }
            };
            JobAssignment assignment = new()
            {
                Job = job,
                Driver = driver
            };
            Assert.Equal(job, assignment.Job);
            Assert.Equal(driver, assignment.Driver);
        }
    }
}
