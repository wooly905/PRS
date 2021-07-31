﻿using System.Threading.Tasks;

namespace PRS.FileHandle
{
    public interface IFileReader
    {
        public Task<string> ReadLineAsync();
        public void Dispose();
    }
}
