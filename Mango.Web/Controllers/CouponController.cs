using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICoupnService _coupnService;

        public CouponController(ICoupnService coupnService) 
        {
            _coupnService = coupnService;
        }

        public async Task<IActionResult> Index()
        {
            List<CouponDto>? list = new();
            ResponseDto? response = await _coupnService.GetAllCoupnsAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(list);
        }

        public async Task<IActionResult> CouponCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CouponCreate(CouponDto coupon)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _coupnService.CreateCoupnAsync(coupon);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Coupon created successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            return View(coupon);
        }

        public async Task<IActionResult> CouponDelete(int CouponId)
        {
            ResponseDto? response = await _coupnService.GetCoupnByIdAsync(CouponId);
            if (response != null && response.IsSuccess)
            {
                CouponDto? model = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CouponDelete(CouponDto coupon)
        {
            ResponseDto? response = await _coupnService.DeleteCoupnAsync(coupon.CouponId);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon deleted successfully";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(coupon);
        }
    }
}
