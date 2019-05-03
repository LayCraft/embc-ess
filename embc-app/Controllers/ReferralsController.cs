using Gov.Jag.Embc.Public.Utils;
using Gov.Jag.Embc.Public.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.Controllers
{
    [Route("api/registrations/{registrationId}/[controller]")]
    [Authorize]
    public class ReferralsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get(string registrationId, [FromQuery] SearchQueryParameters searchQuery)
        {
            var referrals = new[]{
                new ReferralListItem
                {
                    ReferralId = "D1000001",
                    Supplier = new Supplier {Name="Supplier1" },
                    ValidFrom = DateTime.Parse("2019-04-02T11:00:00-07:00"),
                    ValidTo = DateTime.Parse("2019-04-06T11:00:00-07:00"),
                    Type = "Food",
                    SubType = "Groceries",
                    Active = true,
                },
                new ReferralListItem
                {
                    ReferralId = "D1000002",
                    Supplier = new Supplier  {Name="Supplier1" },
                    ValidFrom = DateTime.Parse("2019-04-02T11:00:00-07:00"),
                    ValidTo = DateTime.Parse("2019-04-06T11:00:00-07:00"),
                    Type = "Food",
                    SubType = "Groceries",
                    Active = true,
                },
                new ReferralListItem
                {
                    ReferralId = "D1000003",
                    Supplier =  new Supplier {Name="Supplier2" },
                    ValidFrom = DateTime.Parse("2019-04-02T11:00:00-07:00"),
                    ValidTo = DateTime.Parse("2019-04-06T11:00:00-07:00"),
                    Type = "Clothing",
                    SubType = (string)null,
                    Active = true,
                },
                new ReferralListItem
                {
                    ReferralId = "D1000004",
                    Supplier =  new Supplier {Name="Supplier2" },
                    ValidFrom = DateTime.Parse("2019-04-02T11:00:00-07:00"),
                    ValidTo = DateTime.Parse("2019-04-06T11:00:00-07:00"),
                    Type = "Incidentals",
                    SubType = (string)null,
                    Active = true,
                },
            };

            var results = referrals.AsQueryable().Sort(searchQuery.SortBy ?? "ReferralId");
            return await Task.FromResult(Json(new
            {
                RegistrationId = registrationId,
                Referrals = new PaginatedList<ReferralListItem>(results, searchQuery.Offset, searchQuery.Limit)
            }));
        }
    }
}
