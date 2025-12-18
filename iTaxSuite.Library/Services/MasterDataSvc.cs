using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Data;

namespace iTaxSuite.Library.Services
{
    public interface IMasterDataSvc
    {
        Task<Result<HashSet<string>, string>> GetActiveAuthorities();
        Task<Dictionary<string, EntityAttribute>> GetAtttributeMapAsync(bool reload = false);
        Task<ClientBranch> GetBranchAsync(bool reload = false);
        Task<Dictionary<string, SyncChannel>> GetChannelsAsync(bool reload = false);
        Task<Result<Dictionary<string, S300TaxGroup>, string>> GetTaxGroups();
        Task<Result<Dictionary<string, S300TaxGroup>, string>> InitiateTaxSetup();
        Task InitializeCacheData();
        Task<Result<(string PkgUnitCode, string QtyUnitCode), string>> MapItemAttribs(string pkgUnitCode, string qtyUnitCode);
        Task<bool> SaveSyncChannel(SyncChannel syncChannel);
        Task<bool> UpdateBranchAsync(ClientBranch clientBranch);
        Task<Result<TrnsSalesSaveReq, string>> MapSalesInvcAttribs(TrnsSalesSaveReq salesSaveReq);
    }
    public class MasterDataSvc : IMasterDataSvc
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IDatabase _baseDb;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExtSystConfig _extSystConfig;

        public Dictionary<string, SyncChannel> syncChannelMap { get; private set; } = new();

        public MasterDataSvc(ETimsDBContext dBContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, ExtSystConfig extSystConfig)
        {
            _dbContext = dBContext;
            _baseDb = multiplexer.GetDatabase();
            _httpClientFactory = httpClientFactory;
            _extSystConfig = extSystConfig;
        }

        public async Task InitializeCacheData()
        {
            string _method_ = "InitializeCacheData";

            try
            {
                // SyncChannels
                await _baseDb.KeyDeleteAsync(CacheConst.CHANL_HASHKEY);
                foreach (var chanl in _dbContext.SyncChannels.AsNoTracking())
                {
                    string _hashKey_ = CacheConst.CHANL_HASHKEY;
                    chanl.InitTracker();
                    syncChannelMap[chanl.ChannelId] = chanl;
                    if (!_baseDb.SetHashValue(_hashKey_, chanl.ChannelId, chanl))
                    {
                        throw new Exception($"InitializeCacheData failed for channel code:{chanl.ChannelId}");
                    }
                }

                // Client Branch
                await _baseDb.KeyDeleteAsync(CacheConst.CLIENT_BRANCH);
                var branch = await _dbContext.ClientBranch.Include(e => e.TaxClient).SingleOrDefaultAsync();
                if (branch != null)
                {
                    if (!await _baseDb.SetValueAsync<ClientBranch>(CacheConst.CLIENT_BRANCH, branch))
                    {
                        throw new Exception($"{_method_} failed for branch code:{branch.BranchCode}");
                    }
                }
                else
                {
                    throw new Exception($"InitializeCacheData failed for branch code:{branch.BranchCode}");
                }

                // AttributeMap
                await _baseDb.KeyDeleteAsync(CacheConst.ATTRIB_HASHKEY);
                foreach (var attribute in _dbContext.EntityAttribute.AsNoTracking())
                {
                    string _hashKey_ = CacheConst.ATTRIB_HASHKEY;
                    if (!await _baseDb.SetHashValueAsync(_hashKey_, attribute.CacheKey, attribute))
                    {
                        throw new Exception($"{_method_} failed for attribute code:{attribute.CacheKey}");
                    }
                }

                // Tax Setup
                await _baseDb.KeyDeleteAsync(CacheConst.TAXAUTH_HASHKEY);
                foreach (var authority in _dbContext.S300TaxAuthority.Where(x => x.Active == true).AsNoTracking())
                {
                    if (!await _baseDb.SetHashValueAsync(CacheConst.TAXAUTH_HASHKEY, authority.CacheKey, authority))
                    {
                        UI.Error($"{_method_} failed for authority code:{authority.CacheKey}");
                    }
                }

                // Products
                await _baseDb.KeyDeleteAsync(CacheConst.PRODUCT_HASHKEY);
                foreach (var item in _dbContext.StockItems.Include(s => s.Product).Select(x => new StockItemKey(x)).AsNoTracking())
                {
                    string _hashKey_ = CacheConst.PRODUCT_HASHKEY;
                    if (!_baseDb.SetHashValue(_hashKey_, item.ProductCode, item))
                    {
                        throw new Exception($"{_method_} failed for Product code:{item.ProductCode}");
                    }
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException()}");
                throw;
            }
        }

        public async Task<Dictionary<string, SyncChannel>> GetChannelsAsync(bool reload = false)
        {
            string _method_ = "GetChannelsAsync";
            Dictionary<string, SyncChannel> syncChannelMap = new();
            try
            {
                if (!reload)
                {
                    var channelsArray = await _baseDb.HashGetAllAsync(CacheConst.CHANL_HASHKEY);
                    foreach (var item in channelsArray)
                    {
                        var channel = JsonConvert.DeserializeObject<SyncChannel>(item.Value);
                        syncChannelMap[item.Name] = channel;
                    }
                }
                if (reload || syncChannelMap.Count == 0)
                {
                    var dbResult = await _dbContext.SyncChannels.AsNoTracking().ToListAsync();
                    foreach (var channel in dbResult)
                    {
                        var _hashKey_ = channel.ChannelId;
                        syncChannelMap[_hashKey_] = channel;
                        if (!await _baseDb.SetHashValueAsync(CacheConst.CHANL_HASHKEY, _hashKey_, channel))
                        {
                            UI.Error($"MasterDataSvc::{_method_} failed for channel code:{_hashKey_}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
            return syncChannelMap;
        }

        public async Task<ClientBranch> GetBranchAsync(bool reload = false)
        {
            string _method_ = "GetBranchAsync";
            ClientBranch result = null;
            try
            {
                if (!reload)
                {
                    result = await _baseDb.GetOrDefaultAsync<ClientBranch>(CacheConst.CLIENT_BRANCH);
                }
                if (reload || result == null)
                {
                    var branch = await _dbContext.ClientBranch.Include(e => e.TaxClient).AsNoTracking().FirstOrDefaultAsync();
                    if (branch is not null)
                    {
                        result = branch;
                        if (!await _baseDb.SetValueAsync<ClientBranch>(CacheConst.CLIENT_BRANCH, branch))
                        {
                            UI.Error($"MasterDataSvc::InitializeCacheData failed for branch code:{branch.BranchCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
            return result;
        }
        public async Task<bool> UpdateBranchAsync(ClientBranch clientBranch)
        {
            string _method_ = "UpdateBranchAsync";
            try
            {
                if (!await _baseDb.SetValueAsync<ClientBranch>(CacheConst.CLIENT_BRANCH, clientBranch))
                {
                    throw new Exception($"MasterDataSvc::InitializeCacheData failed for branch code:{clientBranch.BranchCode}");
                }
                int _dbChanges = await _dbContext.ClientBranch.Where(b => b.BranchCode == clientBranch.BranchCode)
                    .ExecuteUpdateAsync(x => x
                    .SetProperty(c => c.ProductSeq, clientBranch.ProductSeq)
                    .SetProperty(c => c.PurchInvoiceSeq, clientBranch.PurchInvoiceSeq)
                    .SetProperty(c => c.SaleInvoiceSeq, clientBranch.SaleInvoiceSeq)
                    .SetProperty(c => c.UpdatedOn, DateTime.Now)
                    .SetProperty(c => c.UpdatedBy, "SYS-ADMIN")
                    );
                return (_dbChanges > 0);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, EntityAttribute>> GetAtttributeMapAsync(bool reload = false)
        {
            string _method_ = "GetAtttributeMapAsync";
            Dictionary<string, EntityAttribute> attributeMap = new();
            try
            {
                if (!reload)
                {
                    var attributesArray = await _baseDb.HashGetAllAsync(CacheConst.ATTRIB_HASHKEY);
                    foreach (var item in attributesArray)
                    {
                        var attribute = JsonConvert.DeserializeObject<EntityAttribute>(item.Value);
                        attributeMap[item.Name] = attribute;
                    }
                }
                if (reload || attributeMap.Count == 0)
                {
                    var dbResult = _dbContext.EntityAttribute.AsNoTracking().ToList();
                    foreach (var attribute in dbResult)
                    {
                        var _hashKey_ = attribute.CacheKey;
                        attributeMap[_hashKey_] = attribute;
                        if (!await _baseDb.SetHashValueAsync(CacheConst.ATTRIB_HASHKEY, _hashKey_, attribute))
                        {
                            UI.Error($"MasterDataSvc::{_method_} failed for attribute code:{_hashKey_}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
            return attributeMap;
        }
        public async Task<Dictionary<string, StockItemKey>> GetProductMap(bool reload = false)
        {
            string _method_ = "GetProductMap";
            Dictionary<string, StockItemKey> itemMap = new();
            try
            {
                if (!reload)
                {
                    var itemsArray = await _baseDb.HashGetAllAsync(CacheConst.PRODUCT_HASHKEY);
                    foreach (var item in itemsArray)
                    {
                        var stockItemKey = JsonConvert.DeserializeObject<StockItemKey>(item.Value);
                        itemMap[item.Name] = stockItemKey;
                    }
                }
                if (reload || itemMap.Count == 0)
                {
                    var dbResult = _dbContext.StockItems.Include(s => s.Product).Select(x => new StockItemKey(x)).AsNoTracking().ToList();
                    foreach (var stockItemKey in dbResult)
                    {
                        var _hashKey_ = stockItemKey.ProductCode;
                        itemMap[_hashKey_] = stockItemKey;
                        if (!await _baseDb.SetHashValueAsync(CacheConst.PRODUCT_HASHKEY, _hashKey_, stockItemKey))
                        {
                            UI.Error($"MasterDataSvc::{_method_} failed for StockItem code:{_hashKey_}");
                        }
                    }
                }
                return itemMap;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                return null;
            }
        }
        public async Task<Result<(string PkgUnitCode, string QtyUnitCode), string>> MapItemAttribs(string pkgUnitCode, string qtyUnitCode)
        {
            string _method_ = "MapItemAttribs";
            (string PkgUnitCode, string QtyUnitCode) unitCodes = (string.Empty, string.Empty);
            try
            {
                var attributeMap = await GetAtttributeMapAsync();
                var pkgUnitKey = $"{MapperConst.PackagingUnit}:{pkgUnitCode}";
                if (!attributeMap.ContainsKey(pkgUnitKey))
                {
                    throw new Exception($"Missing Map Attribute for {pkgUnitKey}");
                }
                unitCodes.PkgUnitCode = attributeMap[pkgUnitKey].EntityKey;

                var qtyUnitKey = $"{MapperConst.QuantityUnit}:{qtyUnitCode}";
                if (!attributeMap.ContainsKey(qtyUnitKey))
                {
                    throw new Exception($"Missing Map Attribute for {qtyUnitKey}");
                }
                unitCodes.QtyUnitCode = attributeMap[qtyUnitKey].EntityKey;

                return unitCodes;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} PkgUnitCode:{pkgUnitCode}, QtyUnitCode:{qtyUnitCode}, error: {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<TrnsSalesSaveReq, string>> MapSalesInvcAttribs(TrnsSalesSaveReq salesSaveReq)
        {
            string _method_ = "MapSalesInvcAttribs";
            Dictionary<string, StockItemKey> itemMap = null;
            try
            {
                itemMap = await GetProductMap();
                //Console.WriteLine($"itemMap length:{itemMap.Count}");
                var invalidSatii = new List<RecordStatus>() { RecordStatus.INVALID, RecordStatus.DEPENDS, RecordStatus.NONE };

                for (int i = 0; i < salesSaveReq.ItemList.Count; i++)
                {
                    if (!itemMap.ContainsKey(salesSaveReq.ItemList[i]._icItemNumber))
                    {
                        return $"No Mapping for IC ItemNumber {salesSaveReq.ItemList[i]._icItemNumber}";
                    }
                    var stockItemKey = itemMap[salesSaveReq.ItemList[i]._icItemNumber];
                    if (invalidSatii.Contains(stockItemKey.RecordStatus))
                    {
                        UI.Warn($"Invalid Status for IC ItemNumber {salesSaveReq.ItemList[i]._icItemNumber}");
                        //return $"Invalid Status for IC ItemNumber {salesSaveReq.ItemList[i]._icItemNumber}";
                        salesSaveReq.RecordStatus = RecordStatus.DEPENDS;
                    }
                    salesSaveReq.ItemList[i].ItemSeqNumber = stockItemKey.EtrSeqNumber;
                    salesSaveReq.ItemList[i].ItemCode = stockItemKey.TaxItemCode;
                    //salesSaveReq.ItemList[i].ItemTypeCode = stockItemMap.ItemTypeCode.ToString();
                    salesSaveReq.ItemList[i].PkgUnitCode = stockItemKey.PackageUnit;
                    salesSaveReq.ItemList[i].QtyUnitCode = stockItemKey.QuantityUnit;
                    salesSaveReq.ItemList[i].ItemClassCode = stockItemKey.ItemClassCode;
                }

                return salesSaveReq;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} TraderInvoiceNo:{salesSaveReq.TraderInvoiceNo}, error: {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<bool> SaveSyncChannel(SyncChannel syncChannel)
        {
            string _method_ = "SaveSyncChannel";
            try
            {
                _dbContext.ChangeTracker.Clear();

                var entity = await _dbContext.SyncChannels.OrderBy(e => e.CreatedOn)
                    .FirstOrDefaultAsync(s => s.ChannelId == syncChannel.ChannelId);
                if (entity == null)
                    return false;

                entity.LastTracked = entity.UpdatedOn = DateTime.Now;
                entity.UpdatedBy = GeneralConst.SYSNAME_MIDWARE;
                if (syncChannel.Tracker != null)
                    entity.SyncTrackExpr = JsonConvert.SerializeObject(syncChannel.Tracker);

                int changes = await _dbContext.SaveChangesAsync();
                if (changes > 0)
                {
                    await UpdateSyncTracker(syncChannel);
                }
                return (changes > 0);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }
        }

        private async Task<bool> UpdateSyncTracker(SyncChannel syncChannel, bool resetOffset = false)
        {
            string _method_ = "UpdateSyncTracker";
            try
            {
                if (syncChannel == null)
                {
                    UI.Error($"RCacheService::{_method_} failed for channel is null");
                    return false;
                }

                var _hashKey_ = syncChannel.ChannelId;
                if (!syncChannelMap.ContainsKey(_hashKey_))
                    return false;

                if (resetOffset && syncChannel.Tracker != null)
                {
                    if (!syncChannel.ResetTracker())
                    {
                        UI.Error($"MasterDataSvc::{_method_} failed resetting the tracker");
                    }
                    else
                    {                        
                        string syncTrackExpr = JsonConvert.SerializeObject(syncChannel.Tracker);
                        int _dbChanges = await _dbContext.SyncChannels.Where(c => c.ChannelId == syncChannel.ChannelId)
                            .ExecuteUpdateAsync(x => x.SetProperty(c => c.SyncTrackExpr, syncTrackExpr));
                        if (_dbChanges < 1)
                        {
                            UI.Error($"MasterDataSvc::{_method_} failed resetting the tracker Id {syncChannel.ChannelId}");
                        }
                    }
                }

                syncChannelMap[_hashKey_] = syncChannel;
                if (!await _baseDb.SetHashValueAsync(CacheConst.CHANL_HASHKEY, _hashKey_, syncChannel))
                {
                    UI.Error($"MasterDataSvc::{_method_} failed for channel code:{_hashKey_}");
                }
                return true;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
        }

        public async Task<Result<Dictionary<string, S300TaxGroup>, string>> InitiateTaxSetup()
        {
            string _method_ = "GetTaxSetup";
            string _strError = null;
            Dictionary<string, S300TaxGroup> result = new();
            Dictionary<string, S300TaxAuthority> authorityMap = new();
            try
            {
                var tGroupsResult = await GetTaxGroups();
                if (tGroupsResult.IsSuccess && tGroupsResult.GetValue().Count > 0)
                {
                    result = tGroupsResult.GetValue();
                    return result;
                }

                var qParams = new Dictionary<string, string>();
                var client = _httpClientFactory.CreateClient();

                // get groups
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/TX/TXTaxGroups");
                var taxGroups = await client.ProcessGetReqBasicAsync<TXTaxGroups>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (taxGroups == null && !taxGroups.TaxGroups.Any())
                {
                    _strError = "Not Found TaxGroups response from Sage300c ERP";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                // get authorities
                _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/TX/TXTaxAuthorities");
                var authorities = await client.ProcessGetReqBasicAsync<TXTaxAuthorities>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (authorities == null && !authorities.Authories.Any())
                {
                    _strError = $"Not Found TXTaxAuthorities response from Sage300c ERP";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                // get rates
                _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/TX/TXTaxRates");
                var rates = await client.ProcessGetReqBasicAsync<TXTaxRates>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (rates == null && !rates.Rates.Any())
                {
                    _strError = $"Not Found TXTaxRates response from Sage300c ERP";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                authorities.Authories.ForEach(authority =>
                {
                    var taxAuthority = new S300TaxAuthority(authority);
                    if (_dbContext.S300TaxAuthority.AddIfNotExists(taxAuthority, p => p.AuthorityKey == taxAuthority.AuthorityKey) == null)
                    {
                        UI.Warn($"S300TaxAuthority {taxAuthority.AuthorityKey} Already Exists");
                    }
                    else
                    {
                        if (_dbContext.SaveChanges() < 1)
                        {
                            throw new Exception($"S300TaxAuthority {taxAuthority.AuthorityKey} saving to database failed");
                        }
                    }
                    
                    var _rates = rates.Rates.Where(r => r.TaxAuthority.Equals(taxAuthority.AuthorityKey)).ToList();
                    if (_rates != null && _rates.Any())
                    {
                        _rates.ForEach(rate =>
                        {
                            taxAuthority.Rates.Add(new S300TaxRate(rate));
                        });
                    }
                    authorityMap.Add(taxAuthority.CacheKey, taxAuthority);

                });

                taxGroups.TaxGroups.ForEach(group =>
                {
                    var _group = new S300TaxGroup(group);
                    if (!string.IsNullOrWhiteSpace(group.TaxAuthority1) && authorityMap.ContainsKey(group.TaxAuthority1))
                    {
                        _group.Authorities.Add(authorityMap[group.TaxAuthority1].CacheKey, authorityMap[group.TaxAuthority1]);
                    }
                    if (!string.IsNullOrWhiteSpace(group.TaxAuthority2) && authorityMap.ContainsKey(group.TaxAuthority2))
                    {
                        _group.Authorities.Add(authorityMap[group.TaxAuthority2].CacheKey, authorityMap[group.TaxAuthority2]);
                    }
                    if (!string.IsNullOrWhiteSpace(group.TaxAuthority3) && authorityMap.ContainsKey(group.TaxAuthority3))
                    {
                        _group.Authorities.Add(authorityMap[group.TaxAuthority3].CacheKey, authorityMap[group.TaxAuthority3]);
                    }
                    if (!string.IsNullOrWhiteSpace(group.TaxAuthority4) && authorityMap.ContainsKey(group.TaxAuthority4))
                    {
                        _group.Authorities.Add(authorityMap[group.TaxAuthority4].CacheKey, authorityMap[group.TaxAuthority4]);
                    }
                    if (!string.IsNullOrWhiteSpace(group.TaxAuthority5) && authorityMap.ContainsKey(group.TaxAuthority5))
                    {
                        _group.Authorities.Add(authorityMap[group.TaxAuthority5].CacheKey, authorityMap[group.TaxAuthority5]);
                    }
                    result.Add(_group.CacheKey, _group);
                });

                string strJosn = JsonConvert.SerializeObject(result);
                await SetupTaxCache(result);
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<bool,string>> SetupTaxCache(Dictionary<string, S300TaxGroup> taxGroupMap)
        {
            string _method_ = "SetupTaxCache";
            try
            {
                await _baseDb.KeyDeleteAsync(CacheConst.TAXGRP_HASHKEY);
                foreach (var taxGroup in taxGroupMap)
                {
                    if (!await _baseDb.SetHashValueAsync(CacheConst.TAXGRP_HASHKEY, taxGroup.Key, taxGroup.Value))
                    {
                        throw new Exception($"MasterDataSvc::{_method_} failed for taxgroup key:{taxGroup.Key}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<Dictionary<string, S300TaxGroup>, string>> GetTaxGroups()
        {
            string _method_ = "GetTaxGroups";
            Dictionary<string, S300TaxGroup> result = new();
            try
            {
                var taxGroups = await _baseDb.HashGetAllAsync(CacheConst.TAXGRP_HASHKEY);
                foreach (var taxGroup in taxGroups)
                {
                    var _group = JsonConvert.DeserializeObject<S300TaxGroup>(taxGroup.Value);
                    result[taxGroup.Name] = _group;
                }
                return result;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<HashSet<string>, string>> GetActiveAuthorities()
        {
            string _method_ = "GetActiveAuthorities";
            HashSet<string> result = new();
            try
            {
                var authorities = await _baseDb.HashGetAllAsync(CacheConst.TAXAUTH_HASHKEY);
                foreach (var authority in authorities)
                {
                    result.Add(authority.Name);
                }
                return result;
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }


        /*public async Task<MyEntity> GetEntityAsync(int id)
        {
            var cacheKey = $"MyEntity:{id}";
            var cachedEntity = await _cache.StringGetAsync(cacheKey);

            if (cachedEntity.HasValue)
            {
                return JsonSerializer.Deserialize<MyEntity>(cachedEntity);
            }

            var entity = await _dbContext.MyEntities.FindAsync(id);
            if (entity != null)
            {
                await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(entity), TimeSpan.FromMinutes(5));
            }

            return entity;
        }*/

        /*public async Task UpdateEntityAsync(MyEntity entity)
        {
            // Write to the database first
            _dbContext.MyEntities.Update(entity);
            await _dbContext.SaveChangesAsync();

            // Now update the cache
            var cacheKey = $"MyEntity:{entity.Id}";
            await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(entity), TimeSpan.FromMinutes(5));
        }*/

        /*public async Task DeleteEntityAsync(int id)
        {
            // Delete from the database first
            var entity = await _dbContext.MyEntities.FindAsync(id);
            if (entity != null)
            {
                _dbContext.MyEntities.Remove(entity);
                await _dbContext.SaveChangesAsync();

                // Now remove from the cache
                var cacheKey = $"MyEntity:{id}";
                await _cache.KeyDeleteAsync(cacheKey);
            }
        }*/

    }
}
