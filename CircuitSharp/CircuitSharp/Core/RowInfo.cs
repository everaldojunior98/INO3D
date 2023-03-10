namespace CircuitSharp.Core
{
    // Info about each row/column of the matrix for simplification purposes.
    public class RowInfo
    {
        #region Properties

        public enum RowType
        {
            RowNormal,
            RowConst,
            RowEqual
        }

        public int NodeEq;
        public RowType Type;
        public int MapCol;
        public int MapRow;
        public double Value;
        public bool RightSideChanges; // Row's right side changes.
        public bool LeftSideChanges; // Row's left side changes.
        public bool DropRow; // Row is not needed in matrix.

        #endregion

        #region Constructor

        public RowInfo()
        {
            Type = RowType.RowNormal;
        }

        #endregion
    }
}