using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface ICoupnService
    {
        Task<ResponseDto?> GetCoupnAsync(string couponCode);
        Task<ResponseDto?> GetCoupnByIdAsync(int id);
        Task<ResponseDto?> GetAllCoupnsAsync();
        Task<ResponseDto?> CreateCoupnAsync(CouponDto couponDto);
        Task<ResponseDto?> UpdateCoupnAsync(CouponDto couponDto);
        Task<ResponseDto?> DeleteCoupnAsync(int id);
    }
}
