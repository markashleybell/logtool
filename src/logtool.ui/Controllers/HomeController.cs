using Microsoft.AspNetCore.Mvc;

namespace logtool.ui.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) =>
        _logger = logger;

    public IActionResult Index() =>
        View();
}
