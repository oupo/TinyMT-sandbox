using System;
namespace TinyMT_sandbox
{
    public class TinyMT
    {
        public TinyMT(uint[] status, TinyMTParameter param)
        {
            this.status = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                this.status[i] = status[i];
            }
            this.param = param;
        }
        public TinyMT(TinyMT other) : this(other.status, other.param)
        {

        }

        public void nextState()
        {
            uint y = status[3];
            uint x = (status[0] & TINYMT32_MASK) ^ status[1] ^ status[2];
            x ^= (x << TINYMT32_SH0);
            y ^= (y >> TINYMT32_SH0) ^ x;
            status[0] = status[1];
            status[1] = status[2];
            status[2] = x ^ (y << TINYMT32_SH1);
            status[3] = y;

            if (y % 2 == 1)
            {
                status[1] ^= param.mat1;
                status[2] ^= param.mat2;
            }
        }

        public uint temper()
        {
            uint t0 = status[3];
            uint t1 = status[0] + (status[2] >> TINYMT32_SH8);

            t0 ^= t1;
            if (t1 % 2 == 1)
            {
                t0 ^= param.tmat;
            }
            return t0;
        }

        private static readonly uint TINYMT32_MASK = 0x7FFFFFFF;
        private static readonly int TINYMT32_SH0 = 1;
        private static readonly int TINYMT32_SH1 = 10;
        private static readonly int TINYMT32_SH8 = 8;

        public uint[] status { get; set; }
        private TinyMTParameter param;
    }
}
