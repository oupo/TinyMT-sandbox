using System;

namespace TinyMT_sandbox
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var status = new uint[4] { 0x5eadbeef, 0x12345678, 0x11235813, 0x31415926 };
            // var status = new uint[4] { 0x31415926, 0x11235813, 0x12345678, 0x5eadbeef };
            var tiny = new TinyMT(status, new TinyMTParameter(0x8f7011ee, 0xfc78ff1f, 0x3793fdff));

            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("{0:X08} {1:X08} {2:X08} {3:X08} {4:X08}",
                                  tiny.status[0], tiny.status[1], tiny.status[2],
                                  tiny.status[3], tiny.temper());
                tiny.nextState();
            }
        }
    }
}
