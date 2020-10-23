using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Profiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            //Source -> Target
            CreateMap<Address, AddressDto>();

            CreateMap<AddressDto, Address>();
        }



    }
}
