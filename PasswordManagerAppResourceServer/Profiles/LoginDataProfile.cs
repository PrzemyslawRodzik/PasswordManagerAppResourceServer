using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;


namespace PasswordManagerAppResourceServer.Profiles
{
    public class LoginDataProfile : Profile
    {
        public LoginDataProfile()
        {
            //Source -> Target
            CreateMap<LoginData, LoginDataDto>();

            CreateMap<LoginDataDto, LoginData>();
        }
    }
}
