using System;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Payments.Validations;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using System.Linq;

namespace RaizesDoNordeste.Test.UseCases.Payments
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
                PaymentMethod = null!
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
                }
            };

            var result = _validator.Validate(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.PropertyName == "PaymentMethod.Method"), Is.True);
            });
        }
    }
}
