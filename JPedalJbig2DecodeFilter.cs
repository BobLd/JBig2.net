using JBig2.Image;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Filters;
using UglyToad.PdfPig.Tokens;

namespace JBig2
{
    public sealed class JPedalJbig2DecodeFilter : IFilter
    {
        /// <inheritdoc />
        public bool IsSupported => true;

        public Memory<byte> Decode(Memory<byte> input, DictionaryToken streamDictionary, IFilterProvider filterProvider, int filterIndex)
        {
            var decodeParms = DecodeParameterResolver.GetFilterParameters(streamDictionary, filterIndex);

            var dec = new JBIG2Decoder();

            if (decodeParms.TryGet(NameToken.Jbig2Globals, out StreamToken tok) && !tok.Data.IsEmpty)
            {
                dec.setGlobalData(tok.Decode(filterProvider));
            }

            dec.decodeJBIG2(input);

            if (dec.NumberOfPages > 0)
            {
                JBIG2Bitmap page = dec.GetPage(0);
                return page.getData(true);
            }

            return Memory<byte>.Empty;
        }
    }
}
