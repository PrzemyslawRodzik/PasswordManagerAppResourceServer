using AutoMapper;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;

namespace PasswordManagerAppResourceServer.Profiles
{
    public class NotesProfile : Profile
    {
        public NotesProfile()
        {
            //Source -> Target
            CreateMap<Note,NoteDto>();
            
            CreateMap<NoteDto,Note>();
        }



    }
}