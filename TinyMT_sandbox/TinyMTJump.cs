using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyMT_sandbox
{
    class TinyMTJump
    {
        public static void Jump(TinyMT tiny, ulong lower_step, ulong upper_step, string poly_str)
        {
            F2Polynomial jump_poly = F2Polynomial.CalculateJumpPolynomial(lower_step, upper_step, poly_str);
            JumpByPolynomial(tiny, jump_poly);
        }

        private static void Add(TinyMT dest, TinyMT src)
        {
            dest.status[0] ^= src.status[0];
            dest.status[1] ^= src.status[1];
            dest.status[2] ^= src.status[2];
            dest.status[3] ^= src.status[3];
        }

        public static void JumpByPolynomial(TinyMT tiny, F2Polynomial jump_poly)
        {
            TinyMT work = new TinyMT(new uint[4] { 0, 0, 0, 0 }, tiny.param);
            ulong x64 = jump_poly.ar[0];
            for (int i = 0; i < 64; i++)
            {
                if ((x64 & 1) != 0)
                {
                    Add(work, tiny);
                }
                tiny.nextState();
                x64 = x64 >> 1;
            }
            x64 = jump_poly.ar[1];
            while (x64 != 0)
            {
                if ((x64 & 1) != 0)
                {
                    Add(work, tiny);
                }
                tiny.nextState();
                x64 = x64 >> 1;
            }
            work.status.CopyTo(tiny.status, 0);
        }


        public static void Main(string[] args)
        {
            var status = new uint[4] { 0x5eadbeef, 0x12345678, 0x11235813, 0x31415926 };
            var param = new TinyMTParameter(0x8f7011ee, 0xfc78ff1f, 0x3793fdff);
            var poly = "d8524022ed8dff4a8dcc50c798faba43"; // characteristic polynomial which depends on TinyMTParameter value
            var tiny = new TinyMT(status, param);

            for (int i = 0; i < 10000; i++)
            {
                tiny.nextState();
            }
            
            Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08}",
                                  tiny.status[0], tiny.status[1], tiny.status[2], tiny.status[3]);

            var tiny2 = new TinyMT(status, param);
            Jump(tiny2, 10000, 0, poly);
            Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08}",
                                  tiny.status[0], tiny.status[1], tiny.status[2], tiny.status[3]);
        }
    }

    class F2Polynomial
    {
        public F2Polynomial(ulong[] ar)
        {
            this.ar = new ulong[2];
            this.ar[0] = ar[0];
            this.ar[1] = ar[1];
        }
        public ulong[] ar;

        public static F2Polynomial StrToPolynomial(string str)
        {
            ulong ar0 = Convert.ToUInt64(str.Substring(16, 16), 16);
            ulong ar1 = Convert.ToUInt64(str.Substring(0, 16), 16);
            return new F2Polynomial(new ulong[2] { ar0, ar1 });
        }

        public override string ToString()
        {
            return string.Format("{0:X016}{1:X016}", ar[1], ar[0]);
        }

        public static F2Polynomial CalculateJumpPolynomial(ulong lower_step, ulong upper_step, string poly_str)
        {
            F2Polynomial characteristic = StrToPolynomial(poly_str);
            F2Polynomial tee = new F2Polynomial(new ulong[2] { 2, 0 });
            return tee.PowerMod(lower_step, upper_step, characteristic);
        }

        public F2Polynomial PowerMod(ulong lower_power, ulong upper_power, F2Polynomial mod)
        {
            LongPoly tmp = LongPoly.FromPoly(this);
            LongPoly result = new LongPoly(new ulong[4] { 1, 0, 0, 0 });
            LongPoly lmod = LongPoly.FromPoly(mod);
            for (int i = 0; i < 64; i++)
            {
                if ((lower_power & 1) != 0)
                {
                    result.Mul(tmp);
                    result.Mod(lmod);
                }
                tmp.Square();
                tmp.Mod(lmod);
                lower_power = lower_power >> 1;
                if ((lower_power == 0) && (upper_power == 0))
                {
                    break;
                }
            }
            while (upper_power != 0)
            {
                if ((upper_power & 1) != 0)
                {
                    result.Mul(tmp);
                    result.Mod(lmod);
                }
                tmp.Square();
                tmp.Mod(lmod);
                upper_power = upper_power >> 1;
            }
            return result.ToPoly();
        }
    }


    class LongPoly
    {
        public LongPoly(ulong[] ar)
        {
            this.ar = new ulong[4];
            this.ar[0] = ar[0];
            this.ar[1] = ar[1];
            this.ar[2] = ar[2];
            this.ar[3] = ar[3];
        }
        public ulong[] ar;

        public static LongPoly FromPoly(F2Polynomial poly)
        {
            return new LongPoly(new ulong[4] { poly.ar[0], poly.ar[1], 0, 0 });
        }

        public void Add(LongPoly other)
        {
            this.ar[0] ^= other.ar[0];
            this.ar[1] ^= other.ar[1];
            this.ar[2] ^= other.ar[2];
            this.ar[3] ^= other.ar[3];
        }

        public void Shiftup1()
        {
            ulong msb0 = this.ar[0] >> 63;
            ulong msb1 = this.ar[1] >> 63;
            ulong msb2 = this.ar[2] >> 63;
            this.ar[0] = this.ar[0] << 1;
            this.ar[1] = (this.ar[1] << 1) | msb0;
            this.ar[2] = (this.ar[2] << 1) | msb1;
            this.ar[3] = (this.ar[3] << 1) | msb2;
        }

        public void Shiftdown1()
        {
            ulong lsb0 = this.ar[1] << 63;
            ulong lsb1 = this.ar[2] << 63;
            ulong lsb2 = this.ar[3] << 63;
            this.ar[0] = (this.ar[0] >> 1) | lsb0;
            this.ar[1] = (this.ar[1] >> 1) | lsb1;
            this.ar[2] = (this.ar[2] >> 1) | lsb2;
            this.ar[3] = (this.ar[3] >> 1);
        }

        public void ShiftupN(int n)
        {
            if (n <= 64)
            {
                ShiftupN0(n);
            }
            else if (n <= 128)
            {
                ShiftupN1(n);
            }
            else if (n <= 192)
            {
                ShiftupN2(n);
            }
            else
            {
                ShiftupN3(n);
            }
        }
        
        void ShiftupN0(int n)
        {
            ulong msb0 = this.ar[0] >> (64 - n);
            ulong msb1 = this.ar[1] >> (64 - n);
            ulong msb2 = this.ar[2] >> (64 - n);
            this.ar[3] = (this.ar[3] << n) | msb2;
            this.ar[2] = (this.ar[2] << n) | msb1;
            this.ar[1] = (this.ar[1] << n) | msb0;
            this.ar[0] = this.ar[0] << n;
        }
        
        void ShiftupN1(int n)
        {
            n -= 64;
            ulong msb0 = this.ar[0] >> (64 - n);
            ulong msb1 = this.ar[1] >> (64 - n);
            this.ar[3] = (this.ar[2] << n) | msb1;
            this.ar[2] = (this.ar[1] << n) | msb0;
            this.ar[1] = (this.ar[0] << n);
            this.ar[0] = 0;
        }
        
        void ShiftupN2(int n)
        {
            n -= 128;
            ulong msb0 = this.ar[0] >> (64 - n);
            this.ar[3] = (this.ar[1] << n) | msb0;
            this.ar[2] = (this.ar[0] << n);
            this.ar[1] = 0;
            this.ar[0] = 0;
        }
        
        void ShiftupN3(int n)
        {
            n -= 192;
            this.ar[3] = this.ar[0] << n;
            this.ar[2] = 0;
            this.ar[1] = 0;
            this.ar[0] = 0;
        }

        public int Deg()
        {
            return DegLazy(255);
        }

        int DegLazy(int pre_deg)
        {

            int deg = pre_deg;
            int index = pre_deg / 64;
            int bit_pos = pre_deg % 64;

            ulong mask = 1ul << bit_pos;
            for (int i = index; i >= 0; i--)
            {
                while (mask != 0)
                {
                    if ((this.ar[i] & mask) != 0)
                    {
                        return deg;
                    }
                    mask = mask >> 1;
                    deg--;
                }
                mask = 1ul << 63;
            }
            return -1;
        }

        public void Mul(LongPoly y)
        {
            LongPoly x = this;
            LongPoly result = new LongPoly(new ulong[4] { 0, 0, 0, 0 });
            ulong y64 = y.ar[0];
            for (int i = 0; i < 64; i++)
            {
                if ((y64 & 1) != 0)
                {
                    result.Add(x);
                }
                x.Shiftup1();
                y64 = y64 >> 1;
                if ((y64 == 0) && (y.ar[1] == 0))
                {
                    break;
                }
            }
            y64 = y.ar[1];
            while (y64 != 0)
            {
                if ((y64 & 1) != 0)
                {
                    result.Add(x);
                }
                x.Shiftup1();
                y64 = y64 >> 1;
            }
            result.ar.CopyTo(this.ar, 0);
        }

        public void Square()
        {
            LongPoly tmp = new LongPoly(this.ar);
            Mul(tmp);
        }

        public void Mod(LongPoly x)
        {
            LongPoly dest = this;
            int deg = x.Deg();
            int dest_deg = dest.Deg();
            int diff = dest_deg - deg;
            int tmp_deg = deg;
            if (diff < 0)
            {
                return;
            }
            LongPoly tmp = new LongPoly(x.ar);
            if (diff == 0)
            {
                dest.Add(tmp);
                return;
            }

            tmp.ShiftupN(diff);
            tmp_deg += diff;
            dest.Add(tmp);
            dest_deg = dest.DegLazy(dest_deg);
            while (dest_deg >= deg)
            {
                tmp.Shiftdown1();
                tmp_deg--;
                if (dest_deg == tmp_deg)
                {
                    dest.Add(tmp);
                    dest_deg = dest.DegLazy(dest_deg);
                }
            }
        }

        public F2Polynomial ToPoly()
        {
            return new F2Polynomial(new ulong[2] { this.ar[0], this.ar[1] });
        }
    }
}
