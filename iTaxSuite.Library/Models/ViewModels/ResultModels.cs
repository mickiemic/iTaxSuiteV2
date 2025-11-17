using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;

namespace iTaxSuite.Library.Models.ViewModels
{
    public readonly struct Result<TValue, TError>
    {
        private readonly TValue _value;
        private readonly TError _error;

        public bool IsSuccess { get; }
        public bool IsError => !IsSuccess;
        public Result(TValue value) : this()
        {
            IsSuccess = true;
            _value = value;
            _error = default;
        }

        public Result(TError error) : this()
        {
            IsSuccess = false;
            _error = error;
            _value = default;
        }

        public TValue GetValue() => _value;
        public TError GetError() => _error;

        public static implicit operator Result<TValue, TError>(TValue value) => new(value);

        public static implicit operator Result<TValue, TError>(TError error) => new(error);

        public TResult Match<TResult>(
            Func<TValue, TResult> success,
            Func<TError, TResult> failure)
            => IsSuccess ? success(_value!) : failure(_error!);

    }

    public class ApiResponse<T>
    {
        public ApiResponse(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageStatusCode(statusCode);
        }

        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }

        public T Payload { get; set; }

        private string GetDefaultMessageStatusCode(HttpStatusCode statusCode)
        {
            return HttpRespUtils.GetStatusCodeMessage(statusCode);
        }
    }
    public class ApiErrorResp
    {
        public ApiErrorResp(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageStatusCode(statusCode);
        }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        private string GetDefaultMessageStatusCode(HttpStatusCode statusCode)
        {
            return HttpRespUtils.GetStatusCodeMessage(statusCode);
        }

        public bool RecordDuplicate { get; set; } = false;

        public bool HasDayEndError { get; set; } = false;

        public bool RecordInvalid => !RecordDuplicate;
    }

    public enum SortOrder
    {
        [EnumMember(Value = "ASC")]
        [Display(Name = "ASC")]
        ASC = 1,

        [EnumMember(Value = "DESC")]
        [Display(Name = "DESC")]
        DESC = -1
    }
    public record Sorting(string Property, SortOrder Order);
    public abstract class APagedFilter
    {
        public int Skip { get; set; } = 1;
        public int Take { get; set; } = 0;
        public Sorting? Sort { get; set; }
        public IQueryable<T> PageAndOrder<T>(IQueryable<T> queryable) where T : BaseEntity
        {
            if (Take > 0 && Sort != null)
            {
                if (Sort.Order == SortOrder.DESC)
                    queryable = queryable.OrderByDescending(x => EF.Property<object>(x, Sort.Property))
                        .Skip(Skip).Take(Take);
                else
                    queryable = queryable.OrderBy(x => EF.Property<object>(x, Sort.Property))
                        .Skip(Skip).Take(Take);
            }
            return queryable;
        }
        public IQueryable<T> PageAndOrderByStamp<T>(IQueryable<T> queryable, bool Descend = true) where T : BaseEntity
        {
            if (Take > 0)
            {
                if (Descend)
                    queryable = queryable.OrderByDescending(x => EF.Property<object>(x, "CreatedOn"))
                        .Skip(Skip).Take(Take);
                else
                    queryable = queryable.OrderBy(x => EF.Property<object>(x, "CreatedOn"))
                        .Skip(Skip).Take(Take);
            }
            return queryable;
        }

    }
    public class PagedResult<T>
    {
        public int Count { get; set; }
        public List<T> Result { get; set; } = new();
    }

}
