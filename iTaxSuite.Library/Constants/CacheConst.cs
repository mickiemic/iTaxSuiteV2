namespace iTaxSuite.Library.Constants
{
    public static class CacheConst
    {
        public const string CLIENT_BRANCH = "client-branch";

        public const string CHANL_HASHKEY = "sync-channels";

        public const string SEQNCE_HASHKEY = "etim-sequence";
        public const string ATTRIB_HASHKEY = "attrib-mapper";
        public const string ITEMAP_HASHKEY = "item-map";
        public const string TAXGRP_HASHKEY = "tax-groups";
        public const string TAXAUTH_HASHKEY = "tax-authorities";

        public const string CUSTOMER_HASHKEY = "ar-customers";
        public const string ALL_PRODUCT_HASHKEY = "all-products";
        public const string IC_PRODUCT_HASHKEY = "ic-products";
        public const string AR_PRODUCT_HASHKEY = "ar-products";
        public const string GL_PRODUCT_HASHKEY = "gl-products";

        public const string TXSALES_OEINV_HASHKEY = "tax-oesalesinv";
        public const string TXSALES_OECNT_HASHKEY = "tax-oesalescnt";
        public const string TXSALES_ARINV_HASHKEY = "tax-arsalesinv";
        public const string TXSALES_ARCNT_HASHKEY = "tax-arsalescnt";
        public const string TXPURCHASE_HASHKEY = "tax-purchases";
        public const string TXSTOCKMVT_HASHKEY = "stock-movement";

        public const string ALL_TAXPOSTTRX_HASHKEY = "all-taxposttrx";
    }

    public static class MapperConst
    {
        public const string PackagingUnit = "PACKAGING_UNIT";           // 4.5. Packaging Unit
        public const string QuantityUnit = "QUANTITY_UNIT";             // 4.6. Quantity Unit
    }

    public static class ETIMSConst
    {
        public const string FMT_DATEONLY = "yyyyMMdd";
        public const string FMT_DATETIME = "yyyyMMddHHmmss";
        
        public const string STRUCT_DATETIME = "yyyy-MM-dd HH:mm:ss";

        public const string NOITEM_CLASS = "0000000000";
        public const string NOITEM_CODE = "0000000000000";
        public const string NOUNIT_CODE = "XX";
        
        public const string DEFAULT_USER = "S300ETRBridge";
    }

    public static class DTaxConst
    {
        public const string FMT_DATEONLY = "yyyy-MM-dd";
        public const string FMT_DATETIME = "yyyyMMddHHmmss";

        public const string STRUCT_DATETIME = "yyyy-MM-dd HH:mm:ss";

        public const string NOITEM_CLASS = "0000000000";
        public const string NOITEM_CODE = "0000000000000";
        public const string NOUNIT_CODE = "XX";

        public const string DEFAULT_USER = "S300ETRBridge";
    }

}
