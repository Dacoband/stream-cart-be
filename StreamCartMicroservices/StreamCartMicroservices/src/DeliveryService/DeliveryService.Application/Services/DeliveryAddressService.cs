using DeliveryService.Application.DTOs.AddressDTOs;
using DeliveryService.Application.DTOs.BaseDTOs;
using DeliveryService.Application.DTOs.DeliveryOrder;
using DeliveryService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shared.Common.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeliveryService.Application.Services
{
    public class DeliveryAddressService : IDeliveryAddressInterface
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GHNSettings _ghnSettings;

        private const string BaseUrl = "https://dev-online-gateway.ghn.vn/shiip/public-api";

        public DeliveryAddressService(
            IHttpClientFactory httpClientFactory,
            IOptions<GHNSettings> ghnOptions)
        {
            _httpClientFactory = httpClientFactory;
            _ghnSettings = ghnOptions.Value;
        }

        public async Task<ApiResponse<CreateOrderResult>> CreateOrderAsync(UserCreateOrderRequest input)
        {
            var response = new ApiResponse<CreateOrderResult>();
            var client = _httpClientFactory.CreateClient();
            CreateOrderResult createOrderResult = new CreateOrderResult();

            try
            {
                // Lấy địa chỉ người gửi
                var fromProvinceId = await FindProvinceIdAsync(client, input.FromProvince)
                    ?? throw new Exception($"Không tìm thấy tỉnh/thành người gửi: {input.FromProvince}");
                var fromDistrictId = await FindDistrictIdAsync(client, input.FromDistrict, fromProvinceId)
                    ?? throw new Exception($"Không tìm thấy quận/huyện người gửi: {input.FromDistrict}");
                var fromWardCode = await FindWardCodeAsync(client, input.FromWard, fromDistrictId)
                    ?? throw new Exception($"Không tìm thấy phường/xã người gửi: {input.FromWard}");

                // Lấy địa chỉ người nhận
                var toProvinceId = await FindProvinceIdAsync(client, input.ToProvince)
                    ?? throw new Exception($"Không tìm thấy tỉnh/thành người nhận: {input.ToProvince}");
                var toDistrictId = await FindDistrictIdAsync(client, input.ToDistrict, toProvinceId)
                    ?? throw new Exception($"Không tìm thấy quận/huyện người nhận: {input.ToDistrict}");
                var toWardCode = await FindWardCodeAsync(client, input.ToWard, toDistrictId)
                    ?? throw new Exception($"Không tìm thấy phường/xã người nhận: {input.ToWard}");

                // Lấy dịch vụ
                var serviceList = await GetServiceIdAsync(client, fromDistrictId, toDistrictId);
                if (!serviceList.Any(s => s.ServiceTypeId == input.ServiceTypeId))
                {
                    throw new Exception("Không tìm thấy dịch vụ vận chuyển phù hợp.");
                }
                //Tính ngày giao dự kiến
                var expectedDeliveryTime = await GetLeadTimeAsync(client,fromDistrictId,fromWardCode!,toDistrictId,toWardCode!,input.ServiceTypeId);
                expectedDeliveryTime = expectedDeliveryTime.AddDays(1);
                createOrderResult.ExpectedDeliveryDate = expectedDeliveryTime;
                // Tính kích thước tổng
                var totalWeight = input.Items.Sum(x => x.Weight);
                var totalLength = input.Items.Sum(x => x.Length);
                var totalWidth = input.Items.Sum(x => x.Width);
                var totalHeight = input.Items.Sum(x => x.Height);

                var ghnItems = input.Items.Select(i => new GHNItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Weight = i.Weight,
                    Length = i.Length,
                    Width = i.Width,
                    Height = i.Height
                }).ToList();

                var payload = new GHNCreateOrderRequest
                {
                    ToName = input.ToName,
                    ToPhone = input.ToPhone,
                    ToProvince = input.ToProvince,
                    ToDistrictId = toDistrictId,
                    ToWardCode = toWardCode,
                    ToAddress = input.ToAddress,

                    FromName = input.FromName,
                    FromPhone = input.FromPhone,
                    FromProvinceName = input.FromProvince,
                    FromDistrictName = input.FromDistrict,
                    FromWardName = input.FromWard,
                    FromAddress = input.FromAddress,

                    ServiceTypeId = input.ServiceTypeId,
                    PaymentTypeId = 1,
                    RequiredNote = DeliveryNoteEnum.KHONGCHOXEMHANG,
                    Items = ghnItems,
                    Weight = totalWeight,
                    Length = totalLength,
                    Width = totalWidth,
                    Height = totalHeight,

                    Note = input.Note,
                    Description = input.Description,
                    CodAmount = input.CodAmount,
                    ReturnPhone = input.FromPhone,
                    ReturnAddress = input.FromAddress,
                    ReturnDistrictId = fromDistrictId,
                    ReturnWardCode = fromWardCode
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/shipping-order/create");
                request.Headers.Add("Token", _ghnSettings.Token);
                request.Headers.Add("ShopId", _ghnSettings.ShopId);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var apiResponse = await client.SendAsync(request);
                var json = await apiResponse.Content.ReadAsStringAsync();

                if (!apiResponse.IsSuccessStatusCode)
                {
                    response.Success = false;
                    response.Message = $"GHN API lỗi: {(int)apiResponse.StatusCode} - {apiResponse.ReasonPhrase}";
                    return response;
                }

                var result = JsonSerializer.Deserialize<GHNResponseDTO<JsonElement>>(json);
                if (result?.Code != 200 || result.Data.ValueKind == JsonValueKind.Null)
                {
                    response.Success = false;
                    response.Message = $"GHN tạo đơn thất bại: {result?.Message ?? "Không rõ lỗi"}";
                    return response;
                }

                var orderCode = result.Data.GetProperty("order_code").GetString();
                createOrderResult.DeliveryId = orderCode;
                response.Data = createOrderResult;
                response.Message = "Tạo đơn vận chuyển thành công";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi tạo đơn: {ex.Message}";
            }

            return response;
        }


        private async Task<int?> FindProvinceIdAsync(HttpClient client, string name)
        {
            var provinces = await GetAsync<List<ProvinceItem>>(client, $"{BaseUrl}/master-data/province");
            return provinces?.FirstOrDefault(p => p.ProvinceName == name)?.ProvinceId;
        }

        private async Task<int?> FindDistrictIdAsync(HttpClient client, string name, int provinceId)
        {
            var districts = await GetAsync<List<DistrictItem>>(client, $"{BaseUrl}/master-data/district?province_id={provinceId}");
            return districts?.FirstOrDefault(d => d.DistrictName == name)?.DistrictId;
        }

        private async Task<string?> FindWardCodeAsync(HttpClient client, string name, int districtId)
        {
            var wards = await GetAsync<List<WardItem>>(client, $"{BaseUrl}/master-data/ward?district_id={districtId}");
            return wards?.FirstOrDefault(w => w.WardName == name)?.WardCode;
        }

        private async Task<List<GHNServicesDTO>> GetServiceIdAsync(HttpClient client, int fromDistrictId, int toDistrictId)
        {

            var payload = new
            {
                shop_id = int.Parse(_ghnSettings.ShopId),
                from_district = fromDistrictId,
                to_district = toDistrictId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/shipping-order/available-services");
            request.Headers.Add("Token", _ghnSettings.Token);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GHNResponseDTO<List<GHNServicesDTO>>>(json);
            return data?.Data?.ToList();
        }

        private async Task<T> GetAsync<T>(HttpClient client, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Token", _ghnSettings.Token);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<GHNResponseDTO<T>>(json);
            if (parsed == null)
                throw new Exception($"GHN response could not be parsed: {json}");

            if (parsed.Code != 200)
                throw new Exception($"GHN API Error {parsed.Code}: {parsed.Message}");

            if (parsed.Data == null)
                throw new Exception($"GHN API returned null data: {json}");

            return parsed.Data;
        }
        private async Task<DateTime> GetLeadTimeAsync(HttpClient client, int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId)
        {
            var payload = new
            {
                from_district_id = fromDistrictId,
                from_ward_code = fromWardCode,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                service_id = serviceId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/shipping-order/leadtime");
            request.Headers.Add("Token", _ghnSettings.Token);
            request.Headers.Add("ShopId", _ghnSettings.ShopId);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"GHN Leadtime API lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}");

            var result = JsonSerializer.Deserialize<GHNResponseDTO<JsonElement>>(json);
            if (result?.Code != 200 || result.Data.ValueKind == JsonValueKind.Null)
                throw new Exception($"GHN trả về lỗi khi tính thời gian giao hàng: {result?.Message ?? "Không rõ lỗi"}");

            if (result.Data.TryGetProperty("leadtime", out var leadTimeProp) &&
                leadTimeProp.ValueKind == JsonValueKind.Number)
            {
                var leadTimeUnix = leadTimeProp.GetInt64();
                return DateTimeOffset.FromUnixTimeSeconds(leadTimeUnix).ToLocalTime().DateTime;
            }
            else
            {
                throw new Exception($"GHN trả về lỗi khi tính thời gian giao hàng: {result?.Message ?? "Không rõ lỗi"}");
            }

        }


        public async Task<ApiResponse<PreviewOrderResponse>> PreviewOrder(UserPreviewOrderRequestDTO input)
        {
            var response = new ApiResponse<PreviewOrderResponse>();
            var client = _httpClientFactory.CreateClient();

            try
            {
                // Lấy địa chỉ người gửi
                var fromProvinceId = await FindProvinceIdAsync(client, input.FromProvince)
                    ?? throw new Exception($"Không tìm thấy tỉnh/thành người gửi: {input.FromProvince}");
                var fromDistrictId = await FindDistrictIdAsync(client, input.FromDistrict, fromProvinceId)
                    ?? throw new Exception($"Không tìm thấy quận/huyện người gửi: {input.FromDistrict}");
                var fromWardCode = await FindWardCodeAsync(client, input.FromWard, fromDistrictId)
                    ?? throw new Exception($"Không tìm thấy phường/xã người gửi: {input.FromWard}");

                // Lấy địa chỉ người nhận
                var toProvinceId = await FindProvinceIdAsync(client, input.ToProvince)
                    ?? throw new Exception($"Không tìm thấy tỉnh/thành người nhận: {input.ToProvince}");
                var toDistrictId = await FindDistrictIdAsync(client, input.ToDistrict, toProvinceId)
                    ?? throw new Exception($"Không tìm thấy quận/huyện người nhận: {input.ToDistrict}");
                var toWardCode = await FindWardCodeAsync(client, input.ToWard, toDistrictId)
                    ?? throw new Exception($"Không tìm thấy phường/xã người nhận: {input.ToWard}");

                // Lấy danh sách dịch vụ GHN trả về
                var serviceList = await GetServiceIdAsync(client, fromDistrictId, toDistrictId);
                if (serviceList == null || serviceList.Count == 0)
                    throw new Exception("Không tìm thấy dịch vụ vận chuyển phù hợp.");

                var result = new PreviewOrderResponse { ServiceResponses = new List<ServiceResponse>() };
                //Tính ngày giao dự kiến
                
                var ghnItems = input.Items.Select(i => new GHNItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Weight = i.Weight,
                    Length = i.Length,
                    Width = i.Width,
                    Height = i.Height
                }).ToList();
                var totalWeight = input.Items.Sum(x => x.Weight);
                var totalLength = input.Items.Sum(x => x.Length);
                var totalWidth = input.Items.Sum(x => x.Width);
                var totalHeight = input.Items.Sum(x => x.Height);
                foreach (var service in serviceList)
                {
                    var feePayload = new GHNCalculateFeeRequest
                    {
                        FromDistrictId = fromDistrictId,
                        ToDistrictId = toDistrictId,
                        FromWardCode = fromWardCode,
                        ToWardCode = toWardCode,
                        ServiceTypeId = service.ServiceTypeId,
                        Items = ghnItems,
                        Weight = totalWeight,
                        Length = totalLength,
                        Width = totalWidth,
                        Height = totalHeight,

                    };

                    var feeRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/shipping-order/fee");
                    feeRequest.Headers.Add("Token", _ghnSettings.Token);
                    feeRequest.Headers.Add("ShopId", _ghnSettings.ShopId);
                    feeRequest.Content = new StringContent(JsonSerializer.Serialize(feePayload), Encoding.UTF8, "application/json");

                    var feeResponse = await client.SendAsync(feeRequest);
                    var json = await feeResponse.Content.ReadAsStringAsync();

                    if (!feeResponse.IsSuccessStatusCode)
                        continue; // bỏ qua dịch vụ nếu lỗi

                    var feeResult = JsonSerializer.Deserialize<GHNResponseDTO<JsonElement>>(json);

                    if (feeResult?.Code != 200 || feeResult.Data.ValueKind == JsonValueKind.Null)
                        continue;

                    int total = feeResult.Data.GetProperty("total").GetInt32();
                    //Tính ngày giao dự kiến
                    var expectedDeliveryTime = await GetLeadTimeAsync(client, fromDistrictId, fromWardCode!, toDistrictId, toWardCode!, service.ServiceTypeId);
                    expectedDeliveryTime = expectedDeliveryTime.AddDays(1);
                    

                    result.ServiceResponses.Add(new ServiceResponse
                    {
                        ServiceTypeId = service.ServiceTypeId,
                        ServiceName = service.ShortName ?? $"Dịch vụ {service.ServiceTypeId}",
                        TotalAmount = total,
                        ExpectedDeliveryDate = expectedDeliveryTime,
                    });
                }
                
                response.Data = result;
                response.Success = true;
                response.Message = "Xem trước chi phí thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi xem trước đơn hàng: {ex.Message}";
            }

            return response;
        }

        public async Task<ApiResponse<OrderLogResponse>> GetDeliveryStatus(string deliveryId)
        {
            var client = _httpClientFactory.CreateClient();
            var requestPayload = new { order_code = deliveryId };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/shipping-order/detail");
            request.Headers.Add("Token", _ghnSettings.Token);
            request.Headers.Add("ShopId", _ghnSettings.ShopId);
            request.Content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<OrderLogResponse>
                {
                    Success = false,
                    Message = $"GHN API Error: {response.StatusCode}",
                };
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            var logs = data.GetProperty("log")
                .EnumerateArray()
                .Select(log => new OrderLogItem
                {
                    Status = log.GetProperty("status").GetString(),
                    UpdatedDate = log.GetProperty("updated_date").GetDateTime()
                })
                .ToList();

            return new ApiResponse<OrderLogResponse>
            {
                Success = true,
                Message = "Lấy lịch sử trạng thái thành công",
                Data = new OrderLogResponse { Logs = logs }
            };
        }

        public async Task<string> CancelDeliveryOrder(string deliveryId)
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                order_codes = new[] { deliveryId }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/switch-status/cancel")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Token", _ghnSettings.Token);
            request.Headers.Add("ShopId", _ghnSettings.ShopId);

            var apiResponse = await client.SendAsync(request);
            var json = await apiResponse.Content.ReadAsStringAsync();

            return json;
        }
    }
    }
