﻿namespace SQLite.Base.Public.Attributtes
{
    /// <summary>
    /// The collation function to use for this column.
    /// Is used together with the <see cref="CollateAttribute" />.
    /// </summary>
    public enum CollationFunction
    {
        None,

        /// <summary>
        /// Compares string data using memcmp(), regardless of text encoding.
        /// </summary>
        RTrim,

        /// <summary>
        /// The same as binary, except the 26 upper case characters of ASCII are folded to their lower case equivalents before
        /// the comparison is performed. Note that only ASCII characters are case folded. SQLite does not attempt to do full
        /// UTF case folding due to the size of the tables required.
        /// </summary>
        NoCase,

        /// <summary>
        /// The same as binary, except that trailing space characters are ignored.
        /// </summary>
        Binary
    }
}