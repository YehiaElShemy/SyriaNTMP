using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SyriaNTMP.Dto
{
    public class NazeelResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
        public bool Success { get; set; }
    }
}
