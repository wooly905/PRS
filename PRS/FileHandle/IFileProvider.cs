namespace PRS.FileHandle
{
    internal interface IFileProvider
    {
        public IFileReader GetFileReader(string file);

        public IFileWriter GetFileWriter(string file);
    }
}
