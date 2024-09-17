using FrontEnd.Pages.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontEnd.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel(ILogger<IndexModel> logger,
    RabbitMQService rabbitMQService) : PageModel
{
    [BindProperty]
    public SubmitViewModel Model { get; set; }
    
    public void OnGet()
    {

    }

    public IActionResult OnPost()
    {
        var routingKey = Model.Action == "Booking" ? "tour.booked" : "tour.cancelled";
        var message = $"Name: {Model.Name}, Email: {Model.Email}, Tour: {Model.Tour}, Action: {(Model.Action == "Booking" ? "Booked" : "Cancelled")}";
        
        rabbitMQService.SendMessage(routingKey, message);
        
        return RedirectToPage("Index");
    }
}