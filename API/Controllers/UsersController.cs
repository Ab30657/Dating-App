using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using System.Collections.Generic;
using API.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
	[Authorize]
	public class UsersController : BaseApiController
	{
		private readonly IMapper _mapper;
		private readonly IphotoService _photoService;
		private readonly IUnitOfWork _unitOfWork;
		public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IphotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_photoService = photoService;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
		{
			var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
			userParams.CurrentUsername = User.GetUsername();
			if (string.IsNullOrEmpty(userParams.Gender))
				userParams.Gender = gender == "male" ? "female" : "male";
			var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
			Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
			return Ok(users);
		}

		[HttpGet("{username}", Name = "GetUser")]
		public async Task<ActionResult<MemberDto>> GetUser(string username)
		{
			return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: User.GetUsername() == username);
		}

		[HttpPut]
		public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
		{
			var username = User.GetUsername();
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

			_mapper.Map(memberUpdateDto, user);

			_unitOfWork.UserRepository.Update(user);

			if (await _unitOfWork.Complete()) return NoContent();

			return BadRequest("Failed to update request");
		}

		[HttpPost("add-photo")]
		public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
		{
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

			var result = await _photoService.AddPhotoAsync(file);

			if (result.Error != null) return BadRequest(result.Error.Message);

			var photo = new Photo
			{
				Url = result.SecureUrl.AbsoluteUri,
				PublicId = result.PublicId
			};

			user.Photos.Add(photo);

			if (await _unitOfWork.Complete())
			{
				return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
			}
			return BadRequest("Problem adding photo");
		}

		[HttpPut("set-main-photo/{photoId}")]
		public async Task<ActionResult> SetMainPhoto(int photoId)
		{
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

			if (photo.IsMain) return BadRequest("This is already your main photo");

			var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
			if (currentMain != null)
			{
				currentMain.IsMain = false;
			}
			photo.IsMain = true;

			if (await _unitOfWork.Complete()) return NoContent();

			return BadRequest("Failed to set main photo!");
		}

		[HttpDelete("delete-photo/{photoId}")]
		public async Task<ActionResult> DeletePhoto(int photoId)
		{
			var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);

			var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

			if (photo == null) return NotFound();

			if (photo.IsMain) return BadRequest("You cannot delete your main photo");

			if (photo.PublicId != null)
			{
				var results = await _photoService.DeletePhotoAsync(photo.PublicId);
				if (results.Error != null)
				{
					return BadRequest(results.Error.Message);
				}
			}

			user.Photos.Remove(photo);

			if (await _unitOfWork.Complete()) return Ok();

			return BadRequest("Faield to delete the photos");
		}
	}
}