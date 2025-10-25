using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jobs.EasyApply.Common.Models;
using Jobs.EasyApply.Infrastructure.Data;
using Jobs.EasyApply.Infrastructure.Repositories;
using Jobs.EasyApply.Infrastructure.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jobs.EasyApply.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for the Repository pattern implementation
    /// </summary>
    public class RepositoryTests : IDisposable
    {
        private readonly JobDbContext _context;
        private readonly Mock<ILogger<Repository<AppliedJob, int>>> _loggerMock;
        private readonly Repository<AppliedJob, int> _repository;

        public RepositoryTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<JobDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new JobDbContext(options);
            _loggerMock = new Mock<ILogger<Repository<AppliedJob, int>>>();
            _repository = new Repository<AppliedJob, int>(_context, _loggerMock.Object);
        }

        [Fact]
        /// <summary>
        /// Tests that GetByIdAsync returns null for non-existent entity
        /// </summary>
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = 999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        /// <summary>
        /// Tests that AddAsync successfully adds an entity
        /// </summary>
        public async Task AddAsync_ValidEntity_AddsSuccessfully()
        {
            // Arrange
            var appliedJob = new AppliedJob
            {
                JobTitle = "Test Developer",
                Company = "Test Company",
                JobId = "test-123",
                Url = "https://example.com/job/test-123",
                Provider = JobProvider.LinkedIn,
                AppliedDate = DateTime.UtcNow,
                Success = true
            };

            // Act
            var result = await _repository.AddAsync(appliedJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appliedJob.JobTitle, result.JobTitle);
            Assert.Equal(appliedJob.Company, result.Company);
            Assert.True(result.Id > 0);
        }

        [Fact]
        /// <summary>
        /// Tests that AddAsync throws ArgumentNullException for null entity
        /// </summary>
        public async Task AddAsync_NullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            AppliedJob nullEntity = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(nullEntity));
        }

        [Fact]
        /// <summary>
        /// Tests that GetAllAsync returns all entities
        /// </summary>
        public async Task GetAllAsync_MultipleEntities_ReturnsAll()
        {
            // Arrange
            var entities = new List<AppliedJob>
            {
                new AppliedJob
                {
                    JobTitle = "Developer 1",
                    Company = "Company 1",
                    JobId = "job-1",
                    Url = "https://example.com/job/1",
                    Provider = JobProvider.LinkedIn,
                    AppliedDate = DateTime.UtcNow,
                    Success = true
                },
                new AppliedJob
                {
                    JobTitle = "Developer 2",
                    Company = "Company 2",
                    JobId = "job-2",
                    Url = "https://example.com/job/2",
                    Provider = JobProvider.Indeed,
                    AppliedDate = DateTime.UtcNow,
                    Success = false
                }
            };

            foreach (var entity in entities)
            {
                await _repository.AddAsync(entity);
            }

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        /// <summary>
        /// Tests that ExistsAsync returns correct existence status
        /// </summary>
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var appliedJob = new AppliedJob
            {
                JobTitle = "Test Developer",
                Company = "Test Company",
                JobId = "test-exists",
                Url = "https://example.com/job/test-exists",
                Provider = JobProvider.LinkedIn,
                AppliedDate = DateTime.UtcNow,
                Success = true
            };

            var addedEntity = await _repository.AddAsync(appliedJob);

            // Act
            var result = await _repository.ExistsAsync(addedEntity.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        /// <summary>
        /// Tests that CountAsync returns correct count
        /// </summary>
        public async Task CountAsync_AfterAddingEntities_ReturnsCorrectCount()
        {
            // Arrange
            var initialCount = await _repository.CountAsync();
            var entitiesToAdd = 3;

            for (int i = 0; i < entitiesToAdd; i++)
            {
                var appliedJob = new AppliedJob
                {
                    JobTitle = $"Test Developer {i}",
                    Company = $"Test Company {i}",
                    JobId = $"test-{i}",
                    Url = $"https://example.com/job/test-{i}",
                    Provider = JobProvider.LinkedIn,
                    AppliedDate = DateTime.UtcNow,
                    Success = true
                };
                await _repository.AddAsync(appliedJob);
            }

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.Equal(initialCount + entitiesToAdd, result);
        }

        [Fact]
        /// <summary>
        /// Tests that SoftDeleteAsync properly marks entity as deleted
        /// </summary>
        public async Task SoftDeleteAsync_ExistingEntity_MarksAsDeleted()
        {
            // Arrange
            var appliedJob = new AppliedJob
            {
                JobTitle = "Test Developer",
                Company = "Test Company",
                JobId = "test-soft-delete",
                Url = "https://example.com/job/test-soft-delete",
                Provider = JobProvider.LinkedIn,
                AppliedDate = DateTime.UtcNow,
                Success = true,
                IsDeleted = false
            };

            var addedEntity = await _repository.AddAsync(appliedJob);

            // Act
            var deleteResult = await _repository.SoftDeleteAsync(addedEntity.Id);

            // Assert
            Assert.True(deleteResult);

            // Verify the entity is marked as deleted
            var deletedEntity = await _context.AppliedJobs.FindAsync(addedEntity.Id);
            Assert.NotNull(deletedEntity);
            Assert.True(deletedEntity.IsDeleted);
            Assert.NotNull(deletedEntity.DeletedAt);
        }

        [Fact]
        /// <summary>
        /// Tests that UpdateAsync successfully updates an entity
        /// </summary>
        public async Task UpdateAsync_ValidEntity_UpdatesSuccessfully()
        {
            // Arrange
            var appliedJob = new AppliedJob
            {
                JobTitle = "Original Title",
                Company = "Original Company",
                JobId = "test-update",
                Url = "https://example.com/job/test-update",
                Provider = JobProvider.LinkedIn,
                AppliedDate = DateTime.UtcNow,
                Success = false
            };

            var addedEntity = await _repository.AddAsync(appliedJob);
            addedEntity.JobTitle = "Updated Title";
            addedEntity.Success = true;

            // Act
            var result = await _repository.UpdateAsync(addedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.JobTitle);
            Assert.True(result.Success);
        }

        [Fact]
        /// <summary>
        /// Tests that DeleteAsync removes entity from database
        /// </summary>
        public async Task DeleteAsync_ExistingEntity_RemovesFromDatabase()
        {
            // Arrange
            var appliedJob = new AppliedJob
            {
                JobTitle = "Test Developer",
                Company = "Test Company",
                JobId = "test-delete",
                Url = "https://example.com/job/test-delete",
                Provider = JobProvider.LinkedIn,
                AppliedDate = DateTime.UtcNow,
                Success = true
            };

            var addedEntity = await _repository.AddAsync(appliedJob);

            // Act
            var deleteResult = await _repository.DeleteAsync(addedEntity.Id);

            // Assert
            Assert.True(deleteResult);

            // Verify the entity is actually deleted
            var deletedEntity = await _context.AppliedJobs.FindAsync(addedEntity.Id);
            Assert.Null(deletedEntity);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
