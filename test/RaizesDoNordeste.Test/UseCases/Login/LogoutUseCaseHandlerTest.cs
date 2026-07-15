using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Login;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Login;

namespace RaizesDoNordeste.Test.UseCases.Login
{
    [TestFixture]
    public class LogoutUseCaseHandlerTest
    {
        private ApplicationDbContext _context;
        private LogoutUseCaseHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _handler = new LogoutUseCaseHandler(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldRevokeToken_WhenRefreshTokenExists()
        {
            // Arrange
            var token = new UserRefreshToken
            {
                Id = 15,
                AccountId = 50,
                Token = "active_logout_token",
                ExpiresAt = Calendar.Now.AddDays(1),
                Revoked = false,
                RestaurantId = Guid.NewGuid()
            };

            _context.UserRefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            var dto = new LogoutRequestDto { RefreshToken = "active_logout_token" };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Success, Is.True);
            });

            var dbToken = await _context.UserRefreshTokens.FindAsync(15L);
            Assert.Multiple(() =>
            {
                Assert.That(dbToken, Is.Not.Null);
                Assert.That(dbToken.Revoked, Is.True);
                Assert.That(dbToken.Active, Is.False);
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenRefreshTokenDoesNotExist()
        {
            // Arrange
            var dto = new LogoutRequestDto { RefreshToken = "non_existent" };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Contains.Substring("não encontrado"));
            });
        }
    }
}
