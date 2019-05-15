using Gov.Jag.Embc.Public.DataInterfaces;
using Gov.Jag.Embc.Public.PdfUtility;
using Gov.Jag.Embc.Public.Utils;
using Gov.Jag.Embc.Public.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.Controllers
{
    [Route("api/registrations/{registrationId}/[controller]")]
    [Authorize]
    public class ReferralsController : Controller
    {
        private readonly IDataInterface dataInterface;
        private readonly IPdfConverter pdfConverter;

        public ReferralsController(IDataInterface dataInterface, IPdfConverter pdfConverter)
        {
            this.dataInterface = dataInterface;
            this.pdfConverter = pdfConverter;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string registrationId, SearchQueryParameters searchQuery)

        {
            var results = await dataInterface.GetReferralsAsync(registrationId);
            return await Task.FromResult(Json(new
            {
                RegistrationId = registrationId,
                Referrals = new PaginatedList<ReferralListItem>(results.Select(r => r.ToListItem()), searchQuery.Offset, searchQuery.Limit)
            }));
        }

        [HttpGet("{referralId}")]
        public async Task<IActionResult> Get(string registrationId, string referralId)
        {
            var result = await dataInterface.GetReferralAsync(referralId);
            if (result == null || result.RegistrationId != registrationId) return NotFound(new
            {
                registrationId = registrationId,
                referralId = referralId
            });

            return await Task.FromResult(Json(new
            {
                RegistrationId = registrationId,
                Referral = result
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string registrationId, [FromBody] IEnumerable<Referral> newReferrals)
        {
            var referralsList = new List<string>();
            foreach (var referral in newReferrals)
            {
                referralsList.Add(await dataInterface.CreateReferralAsync(referral));
            }

            return await Task.FromResult(Json(new
            {
                RegistrationId = registrationId,
                Referrals = referralsList.Select(r => new { ReferralId = r }).ToArray()
            }));
        }

        [HttpPost("referralPdfs")]
        public async Task<FileContentResult> GetReferralPdfs([FromBody] PrintReferrals printReferrals)
        {
            var content = $@"<!DOCTYPE html><html><body>This is a referral</body></html>";

            return await pdfConverter.ConvertHtmlToPdfAsync(content);
        }

        [HttpDelete("{referralId}")]
        public async Task<IActionResult> Delete(string registrationId, string referralId)
        {
            var result = await dataInterface.DeactivateReferralAsync(referralId);
            if (!result) return NotFound(new
            {
                registrationId = registrationId,
                referralId = referralId
            });

            return Ok();
        }
    }
}
