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
            var existingCartItem =await _cartItemRepository.FindOneAsync(x=> x.ProductId.ToString() == cart.ProductId && x.VariantId.ToString() ==cart.VariantId && x.CartId == existingCart.Id);
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
                VariantId = !string.IsNullOrEmpty(productInfo.VariantId)
    ? Guid.Parse(productInfo.VariantId)
    : (Guid?)null ,
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
                ProductStatus = true,

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

        public async Task<ApiResponse<bool>> DeleteCart(List<Guid> cartItemId)
        {
            //Initiate response
            var result = new ApiResponse<bool>()
            {
                Message = "Xóa sản phẩm trong giỏ hàng thành công",
                Success = true,
            };
            foreach (var item in cartItemId) {

                var existingCartItem = await _cartItemRepository.GetByIdAsync(item.ToString());
                if (existingCartItem == null)
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy sản phẩm cần xóa trong giỏ hàng";
                    return result;
                };
                try
                {
                    await _cartItemRepository.DeleteCartItem(item);
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.Success = false;
                    return result;
                }
            }
            return result;
           
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
                   Discount = (ci.PriceSnapShot - ci.PriceCurrent) / ci.PriceSnapShot * 100
               },
               Quantity = ci.Quantity,
               StockQuantity = ci.Stock,
               Attributes = ci.Attributes,
               PrimaryImage = ci.PrimaryImage,
               ProductStatus = ci.ProductStatus,
           }).ToList(),
           NumberOfProduct = g.Count(),
           TotalPriceInShop = g.Sum(x => x.PriceCurrent),
       }).ToList();
            result.Data = new CartResponeDTO()
            {
                CartId = cartResponse.Id,
                CustomerId = userId,
                CartItemByShop = grouped,
                TotalProduct = cartResponse.Items.Count(),
            };
            return result;
        }

        public async Task<ApiResponse<PreviewOrderResponseDTO>> PreviewOrder(PreviewOrderRequestDTO request)
        {
            var result = new ApiResponse<PreviewOrderResponseDTO>
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

            var cartItems = (await _cartItemRepository.GetAllAsync())
                .Where(ci => request.CartItemId.Contains(ci.Id))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (!cartItems.Any())
            {
                result.Success = false;
                result.Message = "Không tìm thấy giỏ hàng";
                return result;
            }

            var grouped = new List<ProductInShopCart>();

            foreach (var shopGroup in cartItems.GroupBy(ci => new { ci.ShopId, ci.ShopName }))
            {
                var productList = new List<ProductCart>();

                foreach (var ci in shopGroup)
                {
                    // Gọi lại ProductService để lấy kích thước
                    var productInfo = await _productService.GetProductInfoAsync(ci.ProductId.ToString(), ci.VariantId?.ToString());

                    productList.Add(new ProductCart
                    {
                        CartItemId = ci.Id,
                        ProductId = ci.ProductId,
                        VariantID = ci.VariantId,
                        ProductName = ci.ProductName,
                        PriceData = new PriceData
                        {
                            CurrentPrice = ci.PriceCurrent,
                            OriginalPrice = ci.PriceSnapShot,
                            Discount = (ci.PriceSnapShot - ci.PriceCurrent) / ci.PriceSnapShot * 100

                        },
                        Quantity = ci.Quantity,
                        StockQuantity = ci.Stock,
                        Attributes = ci.Attributes,
                        PrimaryImage = ci.PrimaryImage,
                        ProductStatus = ci.ProductStatus,

                        Weight = productInfo?.Weight,
                        Length = productInfo?.Length,
                        Width = productInfo?.Width,
                        Height = productInfo?.Height
                    });
                }

                grouped.Add(new ProductInShopCart
                {
                    ShopId = shopGroup.Key.ShopId,
                    ShopName = shopGroup.Key.ShopName,
                    Products = productList,
                    NumberOfProduct = productList.Sum(p => p.Quantity),
                    TotalPriceInShop = productList.Sum(p => p.PriceData.CurrentPrice * p.Quantity)
                });
            }

            var totalItem = cartItems.Sum(ci => ci.Quantity);
            var totalAmount = cartItems.Sum(ci => ci.PriceCurrent * ci.Quantity);
            var discount = cartItems.Sum(x => x.PriceSnapShot - x.PriceCurrent);
            var subTotal = cartItems.Sum(x => x.PriceSnapShot * x.Quantity);

            result.Data = new PreviewOrderResponseDTO
            {
                TotalAmount = totalAmount,
                Discount = discount,
                TotalItem = totalItem,
                SubTotal = subTotal,
                ListCartItem = grouped
            };

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
            var productInfo = await _productService.GetProductInfoAsync(existingCartItem.ProductId.ToString() , request.VariantId.ToString());
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
