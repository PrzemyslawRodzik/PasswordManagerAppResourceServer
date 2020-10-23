using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;


namespace PasswordManagerAppResourceServer.Profiles
{
    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            //Source -> Target
            CreateMap<CreditCard, CreditCardDto>();

            CreateMap<CreditCardDto, CreditCard>();
        }



    }
}
