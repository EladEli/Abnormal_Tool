using Microsoft.Tri.Infrastructure;
using MoreLinq;

namespace Microsoft.Tri.Common.Data.Common
{
    public sealed class MultiKey<TKey> where TKey : class
    {
        #region Properties

        public TKey Key { get; }
        public bool IsUnique { get; }

        #endregion

        #region Constructors

        public MultiKey(TKey key, bool isUnique)
        {
            Contract.Requires(key != null);

            Key = key;
            IsUnique = isUnique;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return new[]
            {
                $"{nameof(Key)}={Key}",
                $"{nameof(IsUnique)}={IsUnique}"
            }.ToDelimitedString(" ");
        }

        #endregion
    }
}
