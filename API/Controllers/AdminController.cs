using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class AdminController : BaseApiController
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IphotoService _photoService;
		public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IphotoService photoService)
		{
			_photoService = photoService;
			_unitOfWork = unitOfWork;

			_userManager = userManager;
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpGet("users-with-roles")]
		public async Task<ActionResult> GetUsersWithRoles()
		{
			var users = await _userManager.Users
						.Include(x => x.UserRoles)
						.ThenInclude(x => x.Role)
						.OrderBy(x => x.UserName)
						.Select(x => new
						{
							x.Id,
							Username = x.UserName,
							Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
						})
						.ToListAsync();
			return Ok(users);
		}


		[Authorize(Policy = "ModeratePhotoRole")]
		[HttpGet("photos-to-moderate")]
		public async Task<ActionResult> GetPhotosForModerationAsync()
		{
			var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
			return Ok(photos);
		}


		[HttpPost("edit-roles/{username}")]
		public async Task<ActionResult> EditRolesAsync(string username, [FromQuery] string roles)
		{
			var selectedRoles = roles.Split(",").ToArray();
			var user = await _userManager.FindByNameAsync(username);
			if (user == null) return NotFound("Couldn't find the user");
			var userRoles = await _userManager.GetRolesAsync(user);
			var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
			if (!result.Succeeded) return BadRequest("Failed to add roles");

			result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
			if (!result.Succeeded) return BadRequest("Failed to remove roles");
			return Ok(await _userManager.GetRolesAsync(user));
		}

		[Authorize(Policy = "ModeratePhotoRole")]
		[HttpPost("approve-photo/{id}")]
		public async Task<ActionResult> ApprovePhoto(int id)
		{
			var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);
			if (photo == null) return NotFound("Can't find photo");
			photo.IsApproved = true;
			var user = await _unitOfWork.UserRepository.GetUserByPhotoId(id);
			if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

			await _unitOfWork.Complete();
			return Ok();
		}

		[Authorize(Policy = "ModeratePhotoRole")]
		[HttpPost("reject-photo/{id}")]
		public async Task<ActionResult> RejectPhoto(int id)
		{
			var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);

			if (photo.PublicId != null)
			{
				var result = await _photoService.DeletePhotoAsync(photo.PublicId);

				if (result.Result == "ok")
				{
					_unitOfWork.PhotoRepository.DeletePhoto(photo);
				}
			}
			else
			{
				_unitOfWork.PhotoRepository.DeletePhoto(photo);
			}
			return Ok();
		}


	}
}