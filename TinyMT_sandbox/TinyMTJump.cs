using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyMT_sandbox
{
    class TinyMTJump
    {
        public static void Jump(TinyMT tiny, ulong lower_step, ulong upper_step, F2Polynomial poly)
        {
            F2Polynomial jump_poly = null;
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
            var tiny = new TinyMT(status, param);

            for (int i = 0; i < 10000; i++)
            {
                tiny.nextState();
            }
            
            Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08}",
                                  tiny.status[0], tiny.status[1], tiny.status[2], tiny.status[3]);

            var tiny2 = new TinyMT(status, param);
            //F2Polynomial poly = new F2Polynomial(new ulong[2] { 0xf8d2e95d70e48af5, 0x19be1fb458012ef9 });
            var poly = F2Polynomial.StrToPolynomial("19be1fb458012ef9f8d2e95d70e48af5");
            JumpByPolynomial(tiny2, poly);
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
            return string.Format("{0:X016}{1:X016}", ar[0], ar[1]);
        }

        public static F2Polynomial CalculateJumpPolynomial(ulong lower_step, ulong upper_step, string poly_str)
        {
            F2Polynomial characteristic = StrToPolynomial(poly_str);
            F2Polynomial tee = new F2Polynomial(new ulong[2] { 2, 0 });
            return tee.PowerMod(lower_step, upper_step, characteristic);
        }

        public F2Polynomial PowerMod(ulong lower_step, ulong upper_step, F2Polynomial mod)
        {
            // TODO
            return null;
        }
    }
}
