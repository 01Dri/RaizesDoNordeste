using System;
using NUnit.Framework;
using RestauranteUni.Application.UseCases.Payments.Validations;
using RestauranteUni.Domain.Core.Payments.DTO;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using System.Linq;

namespace RestaurenteUni.Test.UseCases.Payments
{
    [TestFixture]
    public class PaymentRequestDtoValidatorTest
    {
        private PaymentRequestDtoValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new PaymentRequestDtoValidator();
        }

        [Test]
        public void ShouldBeValid_WhenDtoIsValid()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = new PaymentMethodDto
                {
                    Method = PaymentMethod.Pix
                },
                PaymentDetails = new PaymentDetailsDto
                {
                    Amount = 100.50m
                }
            };

            var result = _validator.Validate(dto);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ShouldBeInvalid_WhenOrderIdIsEmpty()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.Empty,
                PaymentMethod = new PaymentMethodDto
                {
                    Method = PaymentMethod.Pix
                },
                PaymentDetails = new PaymentDetailsDto
                {
                    Amount = 100.50m
                }
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == nameof(PaymentRequestDto.OrderId)), Is.True);
            });
        }

        [Test]
        public void ShouldBeInvalid_WhenPaymentMethodIsNull()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = null!,
                PaymentDetails = new PaymentDetailsDto
                {
                    Amount = 100.50m
                }
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == nameof(PaymentRequestDto.PaymentMethod)), Is.True);
            });
        }

        [Test]
        public void ShouldBeInvalid_WhenPaymentMethodEnumIsInvalid()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = new PaymentMethodDto
                {
                    Method = (PaymentMethod)99
                },
                PaymentDetails = new PaymentDetailsDto
                {
                    Amount = 100.50m
                }
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == "PaymentMethod.Method"), Is.True);
            });
        }

        [Test]
        public void ShouldBeInvalid_WhenPaymentDetailsIsNull()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = new PaymentMethodDto
                {
                    Method = PaymentMethod.Pix
                },
                PaymentDetails = null!
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == nameof(PaymentRequestDto.PaymentDetails)), Is.True);
            });
        }

        [Test]
        public void ShouldBeInvalid_WhenAmountIsZeroOrNegative()
        {
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = new PaymentMethodDto
                {
                    Method = PaymentMethod.Pix
                },
                PaymentDetails = new PaymentDetailsDto
                {
                    Amount = 0
                }
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == "PaymentDetails.Amount"), Is.True);
            });
        }
    }
}
