using AutoMapper;
using Mango.Services.ShoppingCartApi.Data;
using Mango.Services.ShoppingCartApi.Models;
using Mango.Services.ShoppingCartApi.Models.Dto;
using Mango.Services.ShoppingCartApi.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartApi.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _responseDto;
        private readonly AppDbContext _appDbContext;
        private IMapper _mapper;
        private IProductService _prodService;
        private ICouponService _couponService;

        public CartAPIController(AppDbContext appDbContext, IMapper mapper, IProductService productService, ICouponService couponService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            this._responseDto = new ResponseDto();
            _prodService = productService;
            _couponService = couponService;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_appDbContext.CartHeaders.First(h => h.UserId == userId))
                };
                cartDto.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_appDbContext.CartDetails).Where(d => d.CartHeaderId == cartDto.CartHeader.CartHeaderId);
                IEnumerable<ProductDto> productDtos = await _prodService.GetProducts();

                foreach (var item in cartDto.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(p => p.ProductId == item.ProductId);
                    cartDto.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                if (!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                {
                    CouponDto couponDto = await _couponService.GetCouponCode(cartDto.CartHeader.CouponCode);
                    if (couponDto != null)
                    {
                        cartDto.CartHeader.CartTotal -= couponDto.DiscountAmount;
                        cartDto.CartHeader.Discount = couponDto.DiscountAmount;
                    }
                }
                _responseDto.Result = cartDto;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _appDbContext.CartHeaders.FirstAsync(h => h.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                await _appDbContext.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _appDbContext.CartHeaders.FirstAsync(h => h.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = "";
                await _appDbContext.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _appDbContext.CartHeaders.AsNoTracking().FirstOrDefaultAsync(h => h.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {

                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _appDbContext.CartHeaders.Add(cartHeader);
                    await _appDbContext.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _appDbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _appDbContext.SaveChangesAsync();
                }
                else
                {
                    var cartDetailsFromDb = await _appDbContext.CartDetails.AsNoTracking().FirstOrDefaultAsync(d => d.ProductId == cartDto.CartDetails.First().ProductId &&
                    d.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _appDbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _appDbContext.SaveChangesAsync();
                    }
                    else
                    {
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        _appDbContext.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _appDbContext.SaveChangesAsync();
                    }
                }
                _responseDto.Result = cartDto;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDeatilsId)
        {
            try
            {
                var cartDeatils = _appDbContext.CartDetails.First(d => d.CartDetailsId == cartDeatilsId);

                int totalCountOfCartItem = _appDbContext.CartDetails.Where(d => d.CartDetailsId == cartDeatils.CartDetailsId).Count();
                _appDbContext.CartDetails.Remove(cartDeatils);
                if (totalCountOfCartItem == 1)
                {
                    var cardHeaderToRemove = await _appDbContext.CartHeaders.FirstOrDefaultAsync(h => h.CartHeaderId == cartDeatils.CartHeaderId);
                    _appDbContext.CartHeaders.Remove(cardHeaderToRemove);
                }
                await _appDbContext.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }
    }
}
