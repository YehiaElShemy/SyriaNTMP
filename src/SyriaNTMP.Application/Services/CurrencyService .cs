using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SyriaNTMP.Dto;
using SyriaNTMP.Models;
using SyriaNTMP.Options;
using SyriaNTMP.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SyriaNTMP.Services
{
    [Authorize]
    public class CurrencyService : ApplicationService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<NazeelOptions> _options;
        private readonly IRepository<Currency> _repository;

        public CurrencyService(HttpClient httpClient, IOptions<NazeelOptions> options, IRepository<Currency> repository)
        {
            _httpClient = httpClient;
            _options = options;
            _repository = repository;
        }
        public async Task<IEnumerable<CurrencyDTO>> GetAllCurreniesAsync()
        {
            var result = await _repository.ToListAsync();
            if (result.Count > 0)
            {
                var Data = ObjectMapper.Map<List<Currency>, List<CurrencyDTO>>(result);
                return Data;
            }
            else
            {
                var baseUrl = _options.Value.BaseUrl;
                var requestUri = $"{baseUrl}/{SyriaNTMPSettings.GetCurrenies}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var authKey = GenerateAuthKey(_options.Value.NTMPAuthKey);
                request.Headers.Add(SyriaNTMPSettings.AuthKey, authKey);

                var responseMsg = await _httpClient.SendAsync(request);
                if (responseMsg != null && responseMsg.IsSuccessStatusCode)
                {
                    var response = await responseMsg.Content.ReadFromJsonAsync<NazeelResponse<List<CurrencyDTO>>>();
                    if (response != null)
                    {
                        await _repository.UpdateManyAsync(response.Data.Select(c => new Currency
                        {
                            CurrencyId = c.Id,
                            NameAr = c.NameAr,
                            NameEn = c.NameEn,
                            Symbol = c.Symbol,
                            IsActive = c.IsActive,
                            Color = c.Color
                        }));
                        return response.Data;
                    }

                }
                return new List<CurrencyDTO>();
            }


        }

        private string GenerateAuthKey(string authKey)
        {
            string text = authKey + ToDateTimeString(DateTime.UtcNow.Date, "dd/MM/yyyy");
            return EncryptWithMd5(text);
        }
        private string ToDateTimeString(DateTime dateTime, string format)
        {
            if (dateTime == null || string.IsNullOrEmpty(format) || string.IsNullOrWhiteSpace(format))
                return string.Empty;
            return dateTime.ToString(format, DateTimeFormatInfo.InvariantInfo);
        }
        private static string EncryptWithMd5(string input)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(input));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

    }
}