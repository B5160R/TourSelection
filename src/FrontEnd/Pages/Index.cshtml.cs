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

    public IActionResult OnPost(string action)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var routingKey = Model.Action == "Booking" ? "tour.booked" : "tour.cancelled";

        if (action == "invalid")
        {
            var message = "";
            rabbitMQService.Publish("tour_selection.exchange", routingKey, message);
        }

        if (action == "submit")
        {
            var message = $"Name: {Model.Name}, Email: {Model.Email}, Tour: {Model.Tour}, Action: {(Model.Action == "Booking" ? "Booked" : "Cancelled")}";
            rabbitMQService.Publish("tour_selection.exchange", routingKey, message);
        }

        return RedirectToPage("Index");
    }
}