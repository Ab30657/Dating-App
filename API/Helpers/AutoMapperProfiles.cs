using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			CreateMap<AppUser, MemberDto>()
				.ForMember(destinationMember => destinationMember.PhotoUrl, opt => opt.MapFrom(sourceMember =>
					sourceMember.Photos.FirstOrDefault(x => x.IsMain).Url))
				.ForMember(destinationMember => destinationMember.Age, opt => opt.MapFrom(sourceMember =>
						sourceMember.DateOfBirth.CalculateAge()));
			CreateMap<Photo, PhotoDto>();

			CreateMap<MemberUpdateDto, AppUser>();

			CreateMap<RegisterDto, AppUser>();

			CreateMap<Message, MessageDto>().ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(sourceMember
				=> sourceMember.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
				.ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(sourceMember
				=> sourceMember.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
		}
	}
}