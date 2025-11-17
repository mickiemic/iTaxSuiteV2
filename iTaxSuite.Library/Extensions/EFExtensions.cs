using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace iTaxSuite.Library.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CheckMutualXOR : ValidationAttribute
    {
        public string DualFields { get; set; }
        public int ExpectedCount { get; set; } = 1;
        public override bool IsValid(object value)
        {
            int validFields = 0;

            PropertyInfo propertyInfo;
            string[] PropertyList = DualFields.Split(',');
            if (PropertyList.Length != 2)
                return false;

            foreach (string propertyName in PropertyList)
            {
                propertyInfo = value.GetType().GetProperty(propertyName);
                if (propertyInfo != null && propertyInfo.GetValue(value, null) != null)
                    validFields++;
            }

            return (validFields == ExpectedCount);
        }
    }

    public class CheckValues : ValidationAttribute
    {
        public string ValidValues { get; set; }

        public override bool IsValid(object value)
        {
            string[] validList = ValidValues.Split(',');
            bool result = validList.Contains(value);
            //UI.Info("CheckValues [{0}] is in [{1}] = {2}", value, ValidValues, result);
            return result;
        }
    }
    public class NotMinValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime && dateTime == DateTime.MinValue)
            {
                return new ValidationResult("Date cannot be DateTime.MinValue.");
            }
            return ValidationResult.Success;
        }
    }

    public static class DataRowExtensions
    {
        public static T FieldOrDefault<T>(this DataRow row, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return default(T);
            //if (!row.IsNull(columnName))
            //{
            //    Type dType = row[columnName].GetType();
            //    var dValue = row[columnName];
            //    var fCastVal = row.Field<T>(columnName);
            //}
            return row.IsNull(columnName) ? default(T) : row.Field<T>(columnName);
        }

        public static T FieldOrDefault<T>(this DataRow row, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= row.Table.Columns.Count)
                return default(T);
            //if (!row.IsNull(columnName))
            //{
            //    Type dType = row[columnName].GetType();
            //    var dValue = row[columnName];
            //    var fCastVal = row.Field<T>(columnName);
            //}
            return row.IsNull(columnIndex) ? default(T) : row.Field<T>(columnIndex);
        }
        public static T FieldOrStrDefault<T>(this DataRow row, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= row.Table.Columns.Count)
                return default(T);
            string strValue = null;
            if (!row.IsNull(columnIndex))
                strValue = row[columnIndex].ToString();
            return string.IsNullOrWhiteSpace(strValue) ? default(T) : (T)Convert.ChangeType(strValue, typeof(T));
        }

        public static T FieldOrStrDefaultTrim<T>(this DataRow row, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return default(T);
            string strValue = null;
            if (!row.IsNull(columnName))
                strValue = row[columnName].ToString();
            return string.IsNullOrWhiteSpace(strValue) ? default(T) : (T)Convert.ChangeType(strValue.Trim(), typeof(T));
        }
        public static T FieldOrStrDefaultTrim<T>(this DataRow row, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= row.Table.Columns.Count)
                return default(T);
            string strValue = null;
            if (!row.IsNull(columnIndex))
                strValue = row[columnIndex].ToString();
            return string.IsNullOrWhiteSpace(strValue) ? default(T) : (T)Convert.ChangeType(strValue.Trim(), typeof(T));
        }

    }

    public static class DbSetExtensions
    {
        /*public static EntityEntry<TEnt> AddIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, TEnt entity, Func<TEnt, TKey> predicate = null) where TEnt : class, new()
		{
			// var exists = dbSet.Any(c => predicate(entity).Equals(predicate(c)));
			var exists = predicate != null ? dbSet.Any(c => predicate(entity).Equals(predicate(c))) : dbSet.Any();
			return exists
				? null
				: dbSet.Add(entity);
		}
		
		public static void AddRangeIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, IEnumerable<TEnt> entities, Func<TEnt, TKey> predicate) where TEnt : class
		{
			var entitiesExist = from ent in dbSet
								where entities.Any(add => predicate(ent).Equals(predicate(add)))
								select ent;

			dbSet.AddRange(entities.Except(entitiesExist));
		}
		*/

        public static EntityEntry<TEnt> AddIfNotExists<TEnt>(this DbSet<TEnt> dbSet, TEnt entity, Func<TEnt, bool> predicate = null) where TEnt : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return exists ? null : dbSet.Add(entity);
        }

        public static void AddRangeIfNotExists<T>(this DbSet<T> dbSet, IEnumerable<T> entities, Func<T, bool> predicate) where T : class
        {
            var entitiesExist = from ent in dbSet
                                where entities.Any(predicate)
                                select ent;
            dbSet.AddRange(entities.Except(entitiesExist));
        }
        public static int AddRangeIfNotExistsCount<T>(this DbSet<T> dbSet, IEnumerable<T> entities, Func<T, bool> predicate) where T : class
        {
            /*var entitiesExist = from ent in dbSet
								where entities.Any(predicate)
								select ent;*/
            var entitiesExist = dbSet.Where(predicate);
            int result = entities.Count() - entitiesExist.Count();

            //var remainder = entities.Except(entitiesExist);
            var remainder = entities.Where(e => !dbSet.Any(predicate));
            int existCount = entitiesExist.Count();
            int remainCount = remainder.Count();
            foreach (var item in remainder)
            {
                Console.WriteLine($"REMAINDER: {item}");
            }
            dbSet.AddRange(entities.Except(entitiesExist));
            return result;
        }

        /*
		public static void AddRangeIfNotExists<T, TKey>(this DbSet<T> dbSet, IEnumerable<T> entities, Func<T, TKey> predicate) where T : class
		{
			var entitiesExist = from ent in dbSet
								where entities.Any(add => predicate(ent).Equals(predicate(add)))
								select ent;

			dbSet.AddRange(entities.Except(entitiesExist));
		}*/
    }

}
