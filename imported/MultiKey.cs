using MoreLinq;

namespace Abnormal_UI.Imported
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
