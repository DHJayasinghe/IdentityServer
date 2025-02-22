﻿namespace CommonUtil.Hasher
{
    public sealed class ScryptPasswordHasherOptions
    {
        public int IterationCount { get; set; } = 16384;
        public int BlockSize { get; set; } = 8;
        public int ThreadCount { get; set; } = 1;
    }
}
