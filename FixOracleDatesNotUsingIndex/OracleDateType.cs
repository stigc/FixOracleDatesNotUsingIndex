using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Data;

namespace FixOracleDatesNotUsingIndex
{
    public class OracleDateType : IUserType
    {
        public bool IsMutable
        {
            get { return false; }
        }

        public Type ReturnedType
        {
            get { return typeof(DateTime); }
        }

        public SqlType[] SqlTypes
        {
            get { return new[] { NHibernateUtil.Date.SqlType }; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var obj = NHibernateUtil.DateTime.NullSafeGet(rs, names[0]);

            if (obj == null) return null;

            return Convert.ToDateTime(obj);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                ((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            }
            else
            {
                ((IDataParameter)cmd.Parameters[index]).Value = value;
            }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x == null ? typeof(DateTime).GetHashCode() + 473 : x.GetHashCode();
        }
    }
}
