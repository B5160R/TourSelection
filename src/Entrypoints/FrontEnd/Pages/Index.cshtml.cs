using FrontEnd.Pages.ViewModels;
using Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontEnd.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel(ILogger<IndexModel> logger,
    IRabbitMq rabbitMq) : PageModel
{
    [BindProperty]
    public SubmitViewModel Model { get; set; }

    public void OnGet()
    {

    }

    public IActionResult OnPost()
    {
        var routingKey = Model.Action == "Booking" ? "tour.booked" : "tour.cancelled";
        var message = "";
        
        if (!Model.InvalidTest)
        {
            message = $"Name: {Model.Name}, Email: {Model.Email}, Tour: {Model.Tour}, Action: {(Model.Action == "Booking" ? "Booked" : "Cancelled")}";
        }
        
        rabbitMq.Publish(routingKey, message);

        return RedirectToPage("Index");
    }
}