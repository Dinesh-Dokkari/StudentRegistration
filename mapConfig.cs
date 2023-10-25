using AutoMapper;
using Data_Access_Layer.Models;
using StudentRegistration.Models;

namespace StudentRegistration
{
    public class mapConfig:Profile
    {
        public mapConfig()
        {
            CreateMap<StudentDTO,Student>().ReverseMap();
            CreateMap<StudentDTO,StudentUploadDto>().ReverseMap();
            CreateMap<StudentUploadDto,StudentDTO>().ReverseMap();
            //CreateMap<StudentDto,Student>().ReverseMap();
            
        }

    }
}
