using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;


namespace PasswordManagerAppResourceServer.Profiles
{
    public class PhoneNumberProfile : Profile
    {
        public PhoneNumberProfile()
        {
            //Source -> Target
            CreateMap<PhoneNumber, PhoneNumberDto>();

            CreateMap<PhoneNumberDto, PhoneNumber>();
        }
    }
}
