using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Accounts.DTO
{
    public class UserProfileResponseDto : IUseCaseResponse
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public List<string> Roles { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public Error? ErrorResponse { get; set; }
    }
}
