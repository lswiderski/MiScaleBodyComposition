using System;
using System.Collections.Generic;
using System.Text;

namespace MiScaleBodyComposition.Contracts
{
    public class S400InputData
    {
        public string MacOriginal { get; set; }
        public string AesKey { get; set; }
        public string DataString { get; set; }
        public byte[] Data { get; set; }
    }
}
