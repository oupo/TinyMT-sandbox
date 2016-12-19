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

        public void jumpState(ulong lower_step, ulong upper_step, string poly_str)
        {
            if (lower_step == 0 && upper_step == 0) return;
            if (lower_step > 0)
            {
                lower_step -= 1;
            }
            else
            {
                lower_step = unchecked((ulong)(-1L));
                upper_step -= 1;
            }
            F2Polynomial jump_poly = F2Polynomial.CalculateJumpPolynomial(lower_step, upper_step, poly_str);
            jumpByPolynomial(jump_poly);
            nextState();
        }

        public void jumpByPolynomial(F2Polynomial jump_poly)
        {
            var work = new TinyMT(new uint[4] { 0, 0, 0, 0 }, param);
            ulong x64 = jump_poly.ar[0];
            for (int i = 0; i < 64; i++)
            {
                if ((x64 & 1) != 0)
                {
                    Add(work, this);
                }
                nextState();
                x64 = x64 >> 1;
            }
            x64 = jump_poly.ar[1];
            while (x64 != 0)
            {
                if ((x64 & 1) != 0)
                {
                    Add(work, this);
                }
                nextState();
                x64 = x64 >> 1;
            }
            work.status.CopyTo(this.status, 0);
        }

        private static void Add(TinyMT dest, TinyMT src)
        {
            dest.status[0] ^= src.status[0];
            dest.status[1] ^= src.status[1];
            dest.status[2] ^= src.status[2];
            dest.status[3] ^= src.status[3];
        }

        private static readonly uint TINYMT32_MASK = 0x7FFFFFFF;
        private static readonly int TINYMT32_SH0 = 1;
        private static readonly int TINYMT32_SH1 = 10;
        private static readonly int TINYMT32_SH8 = 8;

        public uint[] status { get; set; }
        public TinyMTParameter param { get; }
    }
}
