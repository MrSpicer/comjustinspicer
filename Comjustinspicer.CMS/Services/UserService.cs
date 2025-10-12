using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Comjustinspicer.CMS.Services;

public class UserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public bool IsUserAuthor =>
		_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true &&
		(_httpContextAccessor.HttpContext.User.IsInRole("Admin") || _httpContextAccessor.HttpContext.User.IsInRole("Editor"));
}