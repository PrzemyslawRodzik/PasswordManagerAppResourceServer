using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;


namespace PasswordManagerAppResourceServer.Profiles
{
    public class PaypalAccountProfile : Profile
    {
        public PaypalAccountProfile()
        {
            //Source -> Target
            CreateMap<PaypalAccount, PaypalAccountDto>();

            CreateMap<PaypalAccountDto, PaypalAccount>();
        }
    }
}
