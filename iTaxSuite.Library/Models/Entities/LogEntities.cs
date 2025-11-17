using iTaxSuite.Library.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("ApiRequestLog")]
    public class ApiRequestLog
    {
        [Key]
        [Required]
        [StringLength(64)]
        public string RequestID { get; set; }
        [Required]
        [StringLength(32)]
        public string AppName { get; set; } = GeneralConst.APPLICATION_NAME;
        [Required]
        public RequestDirection Direction { get; set; }
        [Required]
        [StringLength(8)]
        public string Method { get; set; }
        [Required]
        [StringLength(128)]
        public string ReqPath { get; set; }
        [StringLength(64)]
        public string IPAddress { get; set; }
        [StringLength(512)]
        public string ReqHeaders { get; set; }
        [StringLength(256)]
        public string QParams { get; set; }
        [StringLength(4000)]
        public string ReqPayload { get; set; }
        [Required]
        public DateTime RequestAt { get; set; } = DateTime.Now;
        [StringLength(64)]
        public string RequestBy { get; set; }
        [Required]
        public int StatusCode { get; set; } = 999;

        // Response Properties
        [StringLength(256)]
        public string RespHeaders { get; set; }
        [StringLength(4000)]
        public string RespPayload { get; set; }
        public DateTime? ResponseAt { get; set; }
        public double Duration { get; set; }

        public ApiRequestLog()
        {
        }
        public ApiRequestLog(RequestDirection direction)
        {
            Direction = direction;
        }
        public ApiRequestLog(RequestDirection direction, string requestId, string method, string path, string ipAddress,
            string headers, string qParams, string reqPaload)
            : this(direction)
        {
            RequestID = requestId;
            Method = method.ToUpper();
            ReqPath = path;
            IPAddress = ipAddress;
            ReqHeaders = headers;
            QParams = qParams;
            ReqPayload = reqPaload;
        }

        public void SetResponse(int statusCode, string respPayload)
        {
            ResponseAt = DateTime.Now;
            StatusCode = statusCode;
            RespPayload = respPayload;
        }

        public string FormatRequest()
        {
            return $"[{RequestID}]: {RequestAt:s}, {Method}, {ReqPath}, {ReqHeaders}, {ReqPayload}";
        }

        public string FormatResponseSuccess()
        {
            return $"[{RequestID}]: {ResponseAt?.ToString("s")}, {StatusCode}, {RespHeaders}, {Duration:F1}, {RespPayload}";
        }
        public string FormatResponseFailed()
        {
            return $"[{RequestID}]: {ResponseAt?.ToString("s")}, {StatusCode}, {RespHeaders}, {Duration:F1}, {RespPayload}";
        }
    }


    public enum RequestDirection
    {
        [EnumMember(Value = "INBOUND")]
        INBOUND = -1,
        [EnumMember(Value = "OUTBOUND")]
        OUTBOUND = 1
    }

}
