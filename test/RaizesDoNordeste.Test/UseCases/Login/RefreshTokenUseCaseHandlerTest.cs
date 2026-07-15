using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Login;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.Services;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Login
{
    [TestFixture]
    public class RefreshTokenUseCaseHandlerTest
    {
        private ApplicationDbContext _context;
        private Mock<ITokenService> _tokenServiceMock;
        private RefreshTokenUseCaseHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock
                .Setup(s => s.WriteToken(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<List<Claim>>(), It.IsAny<DateTime?>()))
                .Returns("new_jwt_token");

            _handler = new RefreshTokenUseCaseHandler(_context, _tokenServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var account = new RaizesDoNordeste.Domain.Core.Accounts.Account
            {
                Id = 100,
                Email = new Email("user@test.com"),
                Password = "hashedpassword",
                RoleAccounts = new List<RoleAccount>
                {
                    RoleAccount.Create(RoleType.Customer, RoleStatus.Enable)
                }
            };

            var restaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

            var token = new UserRefreshToken
            {
                Id = 10,
                AccountId = 100,
                Account = account,
                Token = "valid_refresh_token",
                ExpiresAt = Calendar.Now.AddDays(1),
                Revoked = false,
                RestaurantId = restaurantId
            };

            _context.Accounts.Add(account);
            _context.UserRefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            var dto = new RefreshRequestDto { RefreshToken = "valid_refresh_token" };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Token, Is.EqualTo("new_jwt_token"));
                Assert.That(result.Data.RefreshToken, Is.Not.Null.And.Not.EqualTo("valid_refresh_token"));
            });

            // Verify old token is revoked
            var oldToken = await _context.UserRefreshTokens.FindAsync(10L);
            Assert.Multiple(() =>
            {
                Assert.That(oldToken, Is.Not.Null);
                Assert.That(oldToken.Revoked, Is.True);
                Assert.That(oldToken.Active, Is.False);
            });

            // Verify new token exists in DB
            var newToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.Token == result.Data.RefreshToken);
            Assert.Multiple(() =>
            {
                Assert.That(newToken, Is.Not.Null);
                Assert.That(newToken.AccountId, Is.EqualTo(100));
                Assert.That(newToken.RestaurantId, Is.EqualTo(restaurantId));
                Assert.That(newToken.Revoked, Is.False);
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenRefreshTokenIsExpired()
        {
            // Arrange
            var account = new RaizesDoNordeste.Domain.Core.Accounts.Account
            {
                Id = 101,
                Email = new Email("user2@test.com"),
                Password = "hashedpassword"
            };

            var token = new UserRefreshToken
            {
                AccountId = 101,
                Account = account,
                Token = "expired_token",
                ExpiresAt = Calendar.Now.AddDays(-1), // Expired
                Revoked = false,
                RestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a")
            };

            _context.Accounts.Add(account);
            _context.UserRefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            var dto = new RefreshRequestDto { RefreshToken = "expired_token" };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Contains.Substring("expirado"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenRefreshTokenIsRevoked()
        {
            // Arrange
            var account = new RaizesDoNordeste.Domain.Core.Accounts.Account
            {
                Id = 102,
                Email = new Email("user3@test.com"),
                Password = "hashedpassword"
            };

            var token = new UserRefreshToken
            {
                AccountId = 102,
                Account = account,
                Token = "revoked_token",
                ExpiresAt = Calendar.Now.AddDays(1),
                Revoked = true, // Revoked
                RestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a")
            };

            _context.Accounts.Add(account);
            _context.UserRefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            var dto = new RefreshRequestDto { RefreshToken = "revoked_token" };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Contains.Substring("inválido"));
            });
        }
    }
}
