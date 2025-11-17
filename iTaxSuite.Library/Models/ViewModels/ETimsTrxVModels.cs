using iTaxSuite.Library.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace iTaxSuite.Library.Models.ViewModels
{
    public interface IETIMSTransaction
    {
        ETIMSReqType ReqType { get; set; }
        string RequestAddress { get; set; }
        string RequestKey { get; set; }
        string ParentKey { get; set; }
        bool NewRequest { get; set; }
        int NextSeqNumber { get; set; }
        int RetryCount { get; set; }
        string CacheKey { get; }
        RecordStatus RecordStatus { get; }
        Action<IETIMSTransaction, ETIMSBaseResp> OnSuccess { get; set; }
        Action OnFailure { get; set; }
        bool IsValid();
        object GetRequestEntity();
    }

    public class ETIMSTransaction<T> : IETIMSTransaction where T : BaseTransact
    {
        public T RequestEntity { get; set; }
        [Required]
        public ETIMSReqType ReqType { get; set; }
        [StringLength(255)]
        public string RequestAddress { get; set; }
        [Required]
        [StringLength(64)]
        public string RequestKey { get; set; }
        [StringLength(64)]
        public string ParentKey { get; set; }
        public bool NewRequest { get; set; }
        public int NextSeqNumber { get; set; }
        public int RetryCount { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string CacheKey { get => RequestEntity.CacheKey; }
        public RecordStatus RecordStatus => RequestEntity.RecordStatus;
        public Action<IETIMSTransaction, ETIMSBaseResp> OnSuccess { get; set; }
        public Action OnFailure { get; set; }

        private ETIMSTransaction()
        {
        }
        public ETIMSTransaction(ETIMSReqType reqType, string requestAddress,
            T eTIMSRequest, bool newRequest = false, int retryCount = 0)
            : this()
        {
            RequestEntity = eTIMSRequest;
            RequestKey = RequestEntity.CacheKey;
            RequestAddress = requestAddress;
            ReqType = reqType;
            NewRequest = newRequest;
            RetryCount = retryCount;
        }
        public bool IsValid()
        {
            return true;
        }
        public object GetRequestEntity()
        {
            return RequestEntity;
        }
    }

}
