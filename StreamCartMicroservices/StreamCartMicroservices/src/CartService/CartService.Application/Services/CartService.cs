using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Infrastructure.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;

namespace CartService.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IProductService _productService;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ICartRepository _cartRepository;
        public CartService(IProductService productService, ICartItemRepository cartItemRepo,ICartRepository cartRepo)
        {
            _productService = productService;
            _cartItemRepository = cartItemRepo;
            _cartRepository = cartRepo;
        }
        public async Task<ApiResponse<CreateCartDTO>> AddToCart(CreateCartDTO cart, string userId)
        {
            //Initiate Result
            var result = new ApiResponse<CreateCartDTO>()
            {
                Message = "Thêm sản phẩm vào giỏ hàng thành công",
                Success = true,
            };
            var cartItem = new CartItem();
            //Check Cart
            var existingCart = await _cartRepository.FindOneAsync(x => x.CreatedBy == userId);
            if (existingCart == null)
            {
                existingCart = new Cart();
                existingCart.SetCreator(userId);
                try
                {
                    await _cartRepository.InsertAsync(existingCart);

                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = "Lỗi khi tạo mới giỏ hàng";
                    result.Errors = (List<string>)ex.Data;
                    return result;
                }
            }
            //Get product
            var productInfo =await _productService.GetProductInfoAsync(cart.ProductId, cart.VariantId);
            if (productInfo == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy sản phẩm";
                return result;
            }
            
            //Check Stock
            if (productInfo.Stock <= 0)
            {
                result.Success = false;
                result.Message = "Sản phẩm đã hết hàng";
                return result;
            }
            if (productInfo.Stock <= cart.Quantity)
            {
                result.Success = false;
                result.Message = "Tổng sản phẩm khi thêm vượt quá số hàng tồn kho";
                return result;
            }
            //Update cart Item 
            var existingCartItem =await _cartItemRepository.FindOneAsync(x=> x.ProductId == cart.ProductId && x.VariantId==cart.VariantId && x.CartId == existingCart.Id);
            if (existingCartItem != null) {
                existingCartItem.Quantity = existingCartItem.Quantity + cart.Quantity;
                if(existingCartItem.Quantity < 0)
                {
                    result.Success = false;
                    result.Message = "Số lượng sản phẩm trong giỏ hàng phải lớn hơn 0";

                    return result;
                }
                existingCartItem.SetModifier(userId);
                try
                {
                     await _cartItemRepository.ReplaceAsync(existingCartItem.Id.ToString(), existingCartItem);
                    result.Data = cart;
                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = "Lỗi khi thêm sản phẩm vào giỏ hàng";
                    result.Errors = (List<string>)ex.Data;

                    return result;
                }
            }
            //Create cart Item
            var addToCartItem = new CartItem
            {
                ProductId = productInfo.ProductId,
                VariantId = productInfo.VariantId,
                ProductName = productInfo.ProductName,
                ShopId = productInfo.ShopId,
                ShopName = productInfo.ShopName,
                PriceCurrent = productInfo.PriceCurrent,
                PriceSnapShot = productInfo.PriceCurrent,
                Stock = productInfo.Stock,
                Quantity = cart.Quantity,
                PrimaryImage = productInfo.PrimaryImage,
                Attributes = productInfo.Attributes,
                CartId = existingCart.Id,

            };
            addToCartItem.SetCreator(userId);
            try
            {
                await _cartItemRepository.InsertAsync(addToCartItem);
                result.Data = cart;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Lỗi khi thêm sản phẩm vào giỏ hàng";
                result.Errors = (List<string>)ex.Data;

                return result;
            }


        }

        public async Task<ApiResponse<bool>> DeleteCart(Guid cartItemId)
        {
            //Initiate response
            var result = new ApiResponse<bool>()
            {
                Message = "Xóa sản phẩm trong giỏ hàng thành công",
                Success = true,
            };
            var existingCartItem = await _cartItemRepository.GetByIdAsync(cartItemId.ToString());
            if (existingCartItem == null) {
                result.Success = false;
                result.Message = "Không tìm thấy sản phẩm cần xóa trong giỏ hàng";
                return result;
             };
            try
            {
                await _cartItemRepository.DeleteAsync(cartItemId.ToString());
                return result;
            }
            catch (Exception ex) {
                result.Message = ex.Message;
                result.Success = false;
                return result;
            }
        }

        public async Task<ApiResponse<CartResponeDTO>> GetMyCart(string userId) 
        {
            //Initiate response
            var result = new ApiResponse<CartResponeDTO>()
            {
                Message = "Lấy danh sách sản phẩm trong giỏ hàng thành công",
                Success = true,
            };
            //GetCart
            var cartResponse =await _cartRepository.GetMyCart(userId);
            if(cartResponse == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy giỏ hàng";
                return result;
            }
            //SortBy CreatedAt
            var sortedItems = cartResponse.Items.OrderByDescending(x => x.CreatedAt);
            //GroupByShop
            var grouped = sortedItems
            .GroupBy(ci => new { ci.ShopId, ci.ShopName })
             .Select(g => new ProductInShopCart
       {
           ShopId = g.Key.ShopId,
           ShopName = g.Key.ShopName,
           Products = g.Select(ci => new ProductCart
           {
               CartItemId = ci.Id,
               ProductId = ci.ProductId,
               VariantID = ci.VariantId,
               ProductName = ci.ProductName,
               PriceData = new PriceData
               {
                   CurrentPrice = ci.PriceCurrent,
                   OriginalPrice = ci.PriceSnapShot,
                   Discount = ci.PriceSnapShot - ci.PriceCurrent
               },
               Quantity = ci.Quantity,
               StockQuantity = ci.Stock,
               Attributes = ci.Attributes,
               PrimaryImage = ci.PrimaryImage
           }).ToList()
       }).ToList();
            result.Data = new CartResponeDTO()
            {
                CartId = cartResponse.Id,
                CustomerId = userId,
                CartItemByShop = grouped,
                TotalProduct = grouped.Count,
            };
            return result;
        }

        public async Task<ApiResponse<PreviewOrderResponseDTO>> PreviewOrder(PreviewOrderRequestDTO request) { 
            //Initiate response
            var result = new ApiResponse<PreviewOrderResponseDTO>()
            {
                Message = "Tạo PreviewOrder",
                Success = true,
            };
            if (request.CartItemId == null || !request.CartItemId.Any())
            {
                result.Success = false;
                result.Message = "Không tìm thấy sản phẩm nào trong giỏ hàng";
                return result;
            }
            //GetListOrderItem
            var cartItems = await _cartItemRepository.GetAllAsync();
            cartItems = cartItems.Where(ci => request.CartItemId.Contains(ci.Id));
            if (!cartItems.Any())
            {
                result.Success = false;
                result.Message = "Không tìm thấy sản phẩm nào trong giỏ hàng";
                return result;
            }
            //InitiateResponse
            int totalItem = cartItems.Sum(ci => ci.Quantity);
            decimal totalAmount = cartItems.Sum(ci => ci.PriceCurrent * ci.Quantity);
            decimal discount = cartItems.Sum(x => x.PriceSnapShot) - cartItems.Sum(x=> x.PriceCurrent);
            decimal subTotal = cartItems.Sum(x=> x.PriceSnapShot * x.Quantity);
            PreviewOrderResponseDTO response = new PreviewOrderResponseDTO()
            {
                TotalAmount = totalAmount,
                Discount = discount,
                TotalItem = totalItem,
                SubTotal = subTotal
            };
            result.Data = response;
            return result;
        }

        public async Task<ApiResponse<UpdateCartItemDTO>> UpdateCartItem(UpdateCartItemDTO request, string userId)
        {
            //Initiate response
            var result = new ApiResponse<UpdateCartItemDTO>()
            {
                Message = "Cập nhật sản phẩm trong giỏ hàng thành công",
                Success = true,
            };
            var existingCartItem =await _cartItemRepository.GetByIdAsync(request.CartItem.ToString());
            if (existingCartItem.CreatedBy != userId) {
                result.Success = false;
                result.Message = "Bạn không có quyền cập nhật giỏ hàng này";
                return result;
                
            }
            if (existingCartItem == null) {
                result.Success = false;
                result.Message = "SẢn phẩm không tồn tại trong giỏ hàng";
                return result;
            }
            var productInfo = await _productService.GetProductInfoAsync(existingCartItem.ProductId, request.VariantId);
            if (productInfo == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy sản phẩm";
                return result;
            }
            if (productInfo.Stock <= 0)
            {
                result.Success = false;
                result.Message = "Sản phẩm đã hết hàng";
                return result;
            }
            if (productInfo.Stock <= request.Quantity)
            {
                result.Success = false;
                result.Message = "Tổng sản phẩm khi thêm vượt quá số hàng tồn kho";
                return result;
            }
          
            existingCartItem.Quantity = request.Quantity ?? existingCartItem.Quantity;
            existingCartItem.VariantId = request.VariantId ?? existingCartItem.VariantId;
            existingCartItem.SetModifier(userId);
            try
            {
                await _cartItemRepository.ReplaceAsync(existingCartItem.Id.ToString(), existingCartItem);
                result.Data = request;
                return result;
            }catch (Exception ex)
            {
                result.Success = false; 
                result.Message = ex.Message;
                return result;
            };

        }
    }
}
