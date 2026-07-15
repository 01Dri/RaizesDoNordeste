using System;

namespace RaizesDoNordeste.Domain.Core.Restaurants.DTO
{
    public class RestaurantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Cnpj { get; set; } = null!;
        public string AddressStreet { get; set; } = null!;
        public string AddressNumber { get; set; } = null!;
        public string AddressDistrict { get; set; } = null!;
        public string AddressCity { get; set; } = null!;
        public string AddressState { get; set; } = null!;
        public string AddressZipCode { get; set; } = null!;
        public string? AddressComplement { get; set; }
    }
}
