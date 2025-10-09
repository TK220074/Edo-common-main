using Unity.SharpZipLib.GZip;
using System.IO;
using System.Text;

/// <summary>
/// gzip で文字列の圧縮や解凍を行うクラス
/// </summary>
public static class GZipCompressor
{
    /// <summary>
    /// 圧縮します
    /// </summary>
    public static byte[] Compress( string rawString )
    {
        var rawData = Encoding.UTF8.GetBytes( rawString );
        using ( var memoryStream = new MemoryStream() )
        {
            Compress( memoryStream, rawData );
            return memoryStream.ToArray();
        }
    }
    
    /// <summary>
    /// 解凍します
    /// </summary>
    public static string Decompress( byte[] compressedData )
    {
        using ( var memoryStream = new MemoryStream() )
        {
            Decompress( memoryStream, compressedData );
            var bytes = memoryStream.ToArray();
            return Encoding.UTF8.GetString( bytes );
        }
    }
    
    /// <summary>
    /// 圧縮します
    /// </summary>
    private static void Compress( Stream stream, byte[] rawData )
    {
        using ( var gzipOutputStream = new GZipOutputStream( stream ) )
        {
            gzipOutputStream.Write( rawData, 0, rawData.Length );
        }
    }
    
    /// <summary>
    /// 解凍します
    /// </summary>
    private static void Decompress( Stream stream, byte[] compressedData )
    {
        var buffer = new byte[ 4096 ];
        using ( var memoryStream = new MemoryStream( compressedData ) )
        using ( var gzipOutputStream = new GZipInputStream( memoryStream ) )
        {
            for ( int r = -1; r != 0; r = gzipOutputStream.Read( buffer, 0, buffer.Length ) )
            {
                if ( r > 0 )
                {
                    stream.Write( buffer, 0, r );
                }
            }
        }
    }
}