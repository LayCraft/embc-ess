using Gov.Jag.Embc.Public.DataInterfaces;
using Gov.Jag.Embc.Public.Utils;
using Gov.Jag.Embc.Public.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly IDataInterface dataInterface;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IHostingEnvironment env;
        private readonly IUrlHelper urlHelper;
        private readonly IEmailSender emailSender;

        public RegistrationsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IHostingEnvironment env,
            IDataInterface dataInterface,
            IEmailSender emailSender,
            IUrlHelper urlHelper
        )
        {
            this.emailSender = emailSender;
            this.dataInterface = dataInterface;
            this.httpContextAccessor = httpContextAccessor;
            logger = loggerFactory.CreateLogger(typeof(RegistrationsController));
            this.env = env;
            this.urlHelper = urlHelper;
        }

        [HttpGet(Name = nameof(GetAll))]
        public async Task<IActionResult> GetAll([FromQuery] SearchQueryParameters searchQuery)
        {
            var items = await dataInterface.GetRegistrationsAsync(searchQuery);

            return Json(new
            {
                data = items.Items,
                metadata = items.Pagination
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOne(string id)
        {
            var result = await dataInterface.GetRegistrationAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Json(result);
        }

        [HttpGet("{id}/summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOneSummary(string id)
        {
            var result = await dataInterface.GetRegistrationSummaryAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Json(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] ViewModels.Registration item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            item.Id = null;
            item.Active = true;
            var result = await dataInterface.CreateRegistrationAsync(item);
            if (!string.IsNullOrWhiteSpace(result.HeadOfHousehold.Email))
            {
                var registrationEmail = CreateEmailMessageForRegistration(result);
                emailSender.Send(registrationEmail);
            }
            return Json(result);
        }

        private EmailMessage CreateEmailMessageForRegistration(Registration registration)
        {
            var subject = "Registration completed successfully";
            var body = "<h2>Evacuee Registration Success</h2><br/>" + "<b>What you need to know:</b><br/><br/>" +
               $"Your ESS File Number is: <b>{registration.EssFileNumber}</b>";

            if (registration.IncidentTask == null)
            {
                body += "<br/><br/>" +
                   "- If you do not require support services, no further action is needed.<br/> " +
                   "- If services are required, please report to your nearest Reception Centre." +
                   " An updated list of reception centres can be found at <a href='https://www.emergencyinfobc.gov.bc.ca/'>EmergencyInfoBC</a>.<br/>" +
                   "- If you are at a Reception Centre, proceed to one of the ESS team members on site who will be able to assist you with completing your registration.<br/>" +
                   "- Don’t forget to bring your evacuee registration number with you to the Reception Centre.";
            }

            return new EmailMessage(registration.HeadOfHousehold.Email, subject, body);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ViewModels.Registration item, string id)
        {
            if (id != null && item.Id != null && id != item.Id)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await dataInterface.UpdateRegistrationAsync(item);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var result = await dataInterface.DeactivateRegistration(id);
            return Ok();
        }
    }
}
