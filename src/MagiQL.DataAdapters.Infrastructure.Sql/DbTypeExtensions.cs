using System;
using System.Collections.Generic;
using System.Data;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public static class DbTypeExtensions
    {
        public static bool IsNumericType(this DbType value)
        {
            switch (value)
            {
                case DbType.Byte: 
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return true;
            }

            return false;
        }

        public static object MinValue(this DbType value)
        {
            switch (value)
            {
                case DbType.Boolean:
                    return 0;
                case DbType.Byte:
                    return Byte.MinValue;
                case DbType.Decimal:
                    return 0; //Int32.MinValue; unknown precision
                case DbType.Double:
                    return 0; //Int32.MinValue; unknown precision
                case DbType.Int16:
                    return Int16.MinValue;
                case DbType.Int32:
                    return Int32.MinValue;
                case DbType.Int64:
                    return Int64.MinValue;
                case DbType.Single:
                    return Single.MinValue;
                case DbType.UInt16:
                    return UInt16.MinValue;
                case DbType.UInt32:
                    return UInt32.MinValue;
                case DbType.UInt64:
                    return UInt64.MinValue;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "'1 Jan 1753'"; // note : small date time is 6 Jun 2079 
            }


            return value.IsNumericType() ? (object)Int32.MaxValue : (object)"''";

        }

        public static object MaxValue(this DbType value)
        {
            switch (value)
            {
                case DbType.Boolean:
                    return 1;
                case DbType.Byte:
                    return Byte.MaxValue;
                case DbType.Decimal:
                    return 0;
                case DbType.Double:
                    return 0;
                case DbType.Int16:
                    return Int16.MaxValue;
                case DbType.Int32:
                    return Int32.MaxValue;
                case DbType.Int64:
                    return Int64.MaxValue;
                case DbType.Single:
                    return Single.MaxValue;
                case DbType.UInt16:
                    return UInt16.MaxValue;
                case DbType.UInt32:
                    return UInt32.MaxValue;
                case DbType.UInt64:
                    return UInt64.MaxValue;
                case DbType.Date: 
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "'6 Jun 2079'"; // note : small date time is 1 Jan 1900
            }


            return value.IsNumericType() ? (object)Int32.MaxValue : (object)"'ZZZZZZZZZZZZZZZZ'";

        }

        public static bool IsStringType(this DbType value)
        {
            switch (value)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return true;
            }

            return false;
        }


        private static readonly Dictionary<DbType, Type> SqlDbTypeToType
        = new Dictionary<DbType, Type>
              {
                  {DbType.AnsiString, typeof (string)},
                  {DbType.Binary , typeof (bool)},
                  {DbType.Byte , typeof (byte)},
                  {DbType.Boolean , typeof (bool)},
                 // {DbType.Currency , typeof ()},
                  {DbType.Date , typeof (DateTime)},
                  {DbType.DateTime , typeof (DateTime)},
                  {DbType.Decimal , typeof (decimal)},
                  {DbType.Double , typeof (double)},
                  {DbType.Guid , typeof (Guid)},
                  {DbType.Int16 , typeof (Int16)},
                  {DbType.Int32 , typeof (Int32)},
                  {DbType.Int64 , typeof (Int64)},
                  {DbType.Object , typeof (object)},
                  {DbType.SByte , typeof (sbyte)},
                  {DbType.Single , typeof (Single)},
                  {DbType.String , typeof (string)},
                  {DbType.Time , typeof (DateTime)},
                  {DbType.UInt16 , typeof (UInt16)},
                  {DbType.UInt32 , typeof (UInt32)},
                  {DbType.UInt64, typeof (UInt64)},
                  //{DbType.VarNumeric, typeof ()},
                  {DbType.AnsiStringFixedLength, typeof (string)},
                  {DbType.StringFixedLength, typeof (string)},
                  {DbType.Xml, typeof (string)},
                  {DbType.DateTime2, typeof (DateTime)},
                  {DbType.DateTimeOffset, typeof (DateTimeOffset)},
              };



        public static Type ToClrType(this DbType sqlDbType)
        {
            Type type;
            if (SqlDbTypeToType.TryGetValue(sqlDbType, out type)) return type;
            throw new ArgumentOutOfRangeException("sqlDbType", sqlDbType, "Cannot map the SqlDbType to Type");
        }
    
    }
}
