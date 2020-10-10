using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
