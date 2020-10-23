using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Profiles
{
    public class PersonalInfoProfile : Profile
    {
        public PersonalInfoProfile()
        {
            //Source -> Target
            CreateMap<PersonalInfo, PersonalInfoDto>();

            CreateMap<PersonalInfoDto, PersonalInfo>();
        }
    }
}
