using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
	public class StopsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
