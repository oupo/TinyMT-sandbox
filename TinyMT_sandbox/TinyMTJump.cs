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
            JumpByPolynomial(tiny, jump_poly);
            tiny.nextState();
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
                                  tiny2.status[0], tiny2.status[1], tiny2.status[2], tiny2.status[3]);
        }
    }
}
